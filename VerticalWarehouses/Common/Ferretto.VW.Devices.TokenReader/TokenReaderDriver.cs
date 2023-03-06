using System;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace Ferretto.VW.Devices.TokenReader
{
    public sealed class TokenReaderDriver : SerialPortDriver, ITokenReaderDriver
    {
        #region Fields

        private const byte DataLinkEscape = 0x10;

        private const byte NegativeAcknowledgement = 0x15;

        private const byte StartOfText = 0x02;

        private const int TokenPresencePollMilliseconds = 500;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public int Delay { get; set; }
        #endregion

        #region Events

        public event EventHandler<TokenStatusChangedEventArgs> TokenStatusChanged;

        #endregion

        #region Methods

        protected override Task OnSerialPortClosedAsync()
        {
            this.TokenStatusChanged?.Invoke(this, new TokenStatusChangedEventArgs(false, null));

            return Task.CompletedTask;
        }

        protected override async Task OnSerialPortOpenedAsync()
        {
            do
            {
                try
                {
                    this.logger.Debug($"Port {this.SerialPort.PortName}: waiting for token to be inserted...");

                    while (this.SerialPort.IsOpen && !this.SerialPort.CtsHolding)
                    {
                        await Task.Delay(TokenPresencePollMilliseconds);
                    }

                    if (!this.SerialPort.IsOpen)
                    {
                        this.logger.Debug($"Port {this.SerialPort.PortName}: port was closed. Polling stopped.");
                        return;
                    }

                    this.logger.Debug($"Port {this.SerialPort.PortName}: token is inserted. Querying serial number ...");

                    this.RaiseTokenStatusChanged(true, null);

                    var queryMessage = new Message(
                        Command.TL,
                        deviceAddress: 1,
                        userDataByteCount: Message.SerialNumberByteCount,
                        userDataStartAddress: Message.SerialNumberDataAddress);

                    var responseMessage = this.SendMessage(queryMessage);
                    if (responseMessage is DataMessage dataMessage)
                    {
                        var serialNumber = BitConverter.ToUInt64(dataMessage.UserData.ToArray(), 0);
                        this.logger.Debug($"Port {this.SerialPort.PortName}: token serial number is {serialNumber}.");

                        var serialNumberString = serialNumber.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        this.RaiseTokenStatusChanged(true, serialNumberString);
                    }
                    else
                    {
                        this.logger.Warn($"Port {this.SerialPort.PortName}: unexpected message received from the device.");
                    }

                    this.logger.Debug($"Port {this.SerialPort.PortName}: waiting for token to be removed ...");
                    while (this.SerialPort.IsOpen && this.SerialPort.CtsHolding)
                    {
                        await Task.Delay(TokenPresencePollMilliseconds);
                    }

                    this.RaiseTokenStatusChanged(false, null);
                }
                catch (Exception ex)
                {
                    this.logger.Error($"Port {this.SerialPort.PortName}: error. {ex.Message}");

                    this.SerialPort.Close();
                    this.SerialPort.Open();
                    if (this.SerialPort.IsOpen && !this.SerialPort.CtsHolding)
                    {
                        this.RaiseTokenStatusChanged(false, null);
                    }
                }


                await Task.Delay(this.Delay);
            }
            while (this.SerialPort.IsOpen);
        }

        private void RaiseTokenStatusChanged(bool isInserted, string serialNumber)
        {
            this.TokenStatusChanged?.Invoke(this, new TokenStatusChangedEventArgs(isInserted, serialNumber));
        }

        private Message SendMessage(Message message)
        {
            this.logger.Debug($"Port {this.SerialPort.PortName}: initiating message transfer ...");
            this.SerialPort.WriteByte(StartOfText);
            var response = this.SerialPort.ReadByte();

            this.SerialPort.EnsureSuccessResponse(response);

            var messageBytes = message.ToByteArray();
            this.SerialPort.Write(messageBytes, 0, messageBytes.Length);

            this.logger.Debug($"Port {this.SerialPort.PortName}: waiting for response ...");
            response = this.SerialPort.ReadByte();

            this.SerialPort.EnsureSuccessResponse(response);

            this.logger.Debug($"Port {this.SerialPort.PortName}: positive response, waiting for response message transfer initiation ...");

            response = this.SerialPort.ReadByte();

            this.SerialPort.EnsureSuccessResponse(response);

            this.logger.Debug($"Port {this.SerialPort.PortName}: positive response, waiting for message ...");

            var buffer = new byte[this.SerialPort.ReadBufferSize];
            var totalReadBytes = 0;
            int readBytes;
            do
            {
                readBytes = this.SerialPort.Read(buffer, totalReadBytes, buffer.Length - totalReadBytes);

                if (readBytes == 1 && buffer[totalReadBytes] is StartOfText)
                {
                    this.SerialPort.WriteByte(DataLinkEscape);
                    break;
                }

                totalReadBytes += readBytes;
            } while (readBytes > 0);

            this.logger.Debug($"Port {this.SerialPort.PortName}: received a total of {totalReadBytes} bytes.");

            try
            {
                var responseMessage = Message.FromBytes(buffer, 0, totalReadBytes);

                this.logger.Debug($"Port {this.SerialPort.PortName}: sending end-of-message acknowledgement.");

                this.SerialPort.WriteByte(DataLinkEscape);

                // response = port.ReadByte();
                // Console.WriteLine($"Port {port.PortName}: received response 0x{response:X2}.");
                /*       totalReadBytes = 0;

                       do
                       {
                           readBytes = this.SerialPort.Read(buffer, totalReadBytes, buffer.Length - totalReadBytes);
                           totalReadBytes += readBytes;

                           this.logger.Debug($"Port {this.SerialPort.PortName}: len={totalReadBytes} buffer {new string(buffer.Take(totalReadBytes).SelectMany(b => $" {(int)b}").ToArray())}.");

                           if (readBytes == 1 && buffer[totalReadBytes] is StartOfText)
                           {
                               this.logger.Debug($"Port {this.SerialPort.PortName}: received STX.");

                               this.SerialPort.WriteByte(DataLinkEscape);
                               break;
                           }
                       } while (readBytes > 0);
                */
                return responseMessage;
            }
            catch
            {
                this.SerialPort.WriteByte(NegativeAcknowledgement);
                throw;
            }
        }

        #endregion
    }
}

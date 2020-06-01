using System;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Ferretto.VW.CommonUtils;
using NLog;

namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    internal sealed class BarcodeReader : IBarcodeReaderDriver, IQueryableDevice, IDisposable
    {
        #region Fields

        private const int DefaultReadTimeout = 6000;

        private static readonly Regex responseRegex = new Regex(
            @"@@@@!(?<command>\w+);(&{(?<params>((?<param>[^;]+);)+)\|\w+})?\^\^\^\^", RegexOptions.Compiled);

        private readonly DeviceInformation information = new DeviceInformation();

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly SerialPort serialPort = new SerialPort();

        private bool isDisposed;

        #endregion

        #region Events

        public event EventHandler<ActionEventArgs> BarcodeReceived;

        #endregion

        #region Properties

        public DeviceInformation Information => this.information;

        #endregion

        #region Methods

        public void Connect(ConfigurationOptions options)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(BarcodeReader));
            }

            if (this.serialPort.IsOpen)
            {
                if (options.PortName == this.serialPort.PortName)
                {
                    this.logger.Warn($"Serial port {this.serialPort.PortName} is already open.");

                    return;
                }
                else
                {
                    this.Disconnect();
                }
            }

            this.serialPort.PortName = options.PortName;
            this.serialPort.BaudRate = options.BaudRate;
            this.serialPort.Parity = options.Parity;
            this.serialPort.StopBits = options.StopBits;
            this.serialPort.ReadTimeout = DefaultReadTimeout;

            this.logger.Debug($"Opening serial port {this.serialPort.PortName}.");

            this.serialPort.Open();

            this.RetrieveDeviceInfo();

            new Thread(this.ReadLoop).Start();
        }

        public void Disconnect()
        {
            this.logger.Debug($"Closing serial port {this.serialPort.PortName}.");

            this.serialPort.Close();
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing)
            this.Dispose(true);
        }

        public void SendCommand(string command)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var commandString = new CommandString(command);
            this.serialPort.Write(commandString.ToString());
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.Disconnect();
            }

            this.isDisposed = true;
        }

        private void ReadBytes()
        {
            try
            {
                var buffer = new byte[this.serialPort.ReadBufferSize];

                var byteCount = this.serialPort.Read(buffer, 0, buffer.Length);

                var barcode = Encoding.ASCII.GetString(buffer, 0, byteCount);

                this.logger.Debug($"Received barcode '{barcode.Replace("\r", "<CR>").Replace("\n", "<LF>")}'.");

                this.BarcodeReceived?.Invoke(this, new ActionEventArgs(barcode));
            }
            catch (System.IO.IOException)
            {
                this.logger.Trace($"Serial port '{this.serialPort.PortName}': read timeout.");
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
        }

        private void ReadLoop()
        {
            do
            {
                this.ReadBytes();
            }
            while (this.serialPort.IsOpen && !this.isDisposed);
        }

        private void RetrieveDeviceInfo()
        {
            try
            {
                var commandCode = (int)BarcodeCommand.InquireAllInformation;
                var command = $"$$$$#{commandCode};%%%%";
                this.serialPort.Write(command);

                var buffer = new byte[this.serialPort.ReadBufferSize];
                var byteCount = this.serialPort.Read(buffer, 0, buffer.Length);
                var response = Encoding.ASCII.GetString(buffer, 0, byteCount);

                var match = responseRegex.Match(response);
                if (!match.Success)

                {
                    this.logger.Warn("Unable to retrieve device info: failed to enter programming mode.");
                    return;
                }

                for (var i = 0; i < match.Groups["param"].Captures.Count; i++)
                {
                    var parameter = match.Groups["param"].Captures[i].Value;

                    const string ModelNumberParameter = "Product Name:";
                    const string SerialNumberParameter = "Product ID:";
                    const string ManufactureDateParameter = "!!";
                    const string FirmwareVersionParameter = "Firmware";

                    if (parameter.StartsWith(ModelNumberParameter, StringComparison.Ordinal))
                    {
                        this.information.ModelNumber = parameter.Replace(ModelNumberParameter, "").Trim();
                    }
                    else if (parameter.StartsWith(SerialNumberParameter, StringComparison.Ordinal))
                    {
                        this.information.SerialNumber = parameter.Replace(SerialNumberParameter, "").Trim();
                    }
                    else if (parameter.StartsWith(FirmwareVersionParameter, StringComparison.Ordinal))
                    {
                        this.information.FirmwareVersion = parameter.Replace(FirmwareVersionParameter, "").Trim();
                    }
                    else if (parameter.StartsWith(ManufactureDateParameter, StringComparison.Ordinal))
                    {
                        var dateTimeString = parameter.Replace(ManufactureDateParameter, "");
                        if (DateTime.TryParse(dateTimeString, out var dateTime))
                        {
                            this.information.ManufactureDate = dateTime;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Warn($"Unable to retrieve device info: {ex.Message}.");
            }
        }

        #endregion
    }
}

using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.Devices.AlphaNumericBar
{
    public class AlphaNumericBarDriver : IAlphaNumericBarDriver
    {
        #region Fields

        private readonly Queue messagesReceivedQueue;

        private readonly Queue messagesToBeSendQueue;

        private readonly int tcpTimeout = 3000;

        private bool barEnabled = false;

        private IPAddress ipAddress;

        private int port = 2020;

        private AlphaNumericBarSize size = AlphaNumericBarSize.Medium;

        #endregion

        #region Constructors

        public AlphaNumericBarDriver()
        {
            this.StepLedBar = 4.75;
            this.messagesToBeSendQueue = new Queue();
        }

        #endregion

        #region Properties

        public IPAddress IpAddress => this.ipAddress;

        public Queue MessagesReceived => this.messagesReceivedQueue;

        public int NumberOfLeds => ((int)this.size) * 8;

        public int Port => this.port;

        public AlphaNumericBarSize Size => this.size;

        public double StepLedBar { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <param name="loadUnitlengthInMM"></param>
        /// <param name="itemPositionXInMM"></param>
        /// <returns></returns>
        public int CalculateArrowPosition(double loadingUnitWidth, double itemXPosition)
        {
            var arrowPosition = (loadingUnitWidth / 2) + itemXPosition;
            var pixelOffset = (arrowPosition / this.StepLedBar) + 2;

            return (int)pixelOffset;
        }

        public int CalculateOffset(int offset, string message)
        {
            int result;

            var textInPixels = (message ?? "").Length * 6;                     //convert the string length into numbers of pixels

            if ((textInPixels + offset) > this.NumberOfLeds)
            {
                var auxCrt = offset - 6;

                if (auxCrt > textInPixels)
                {
                    result = (offset - textInPixels) - 6;           //tolgo i sei pxl che ho aggiunto un quanto la scritta deve essere prima della freccia
                }
                else
                {
                    result = -1;
                }
            }
            else
            {
                var auxCrt = (this.NumberOfLeds - offset);

                if (auxCrt > textInPixels)
                {
                    result = offset;
                }
                else
                {
                    result = -2;
                }
            }

            return result;
        }

        public async Task<bool> ClearAsync()
        {
            this.messagesToBeSendQueue.Clear();
            if (!this.barEnabled)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_ON);
            }
            this.EnqueueCommand(AlphaNumericBarCommands.Command.CLEAR);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="size"></param>
        public bool Configure(IPAddress ipAddress, int port, AlphaNumericBarSize size)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.size = size;

            return true;
        }

        public async Task<bool> CustomAsync(string hexval)
        {
            this.EnqueueCommand(AlphaNumericBarCommands.Command.CUSTOM, hexval);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        public async Task<bool> HelpAsync()
        {
            this.messagesToBeSendQueue.Clear();
            this.EnqueueCommand(AlphaNumericBarCommands.Command.HELP);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        public async Task<bool> SetAndWriteArrowAsync(int arrowPosition, bool forceClear = true)
        {
            this.messagesToBeSendQueue.Clear();
            if (!this.barEnabled)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_ON);
            }

            if (forceClear)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.CLEAR);
            }

            this.EnqueueCommand(AlphaNumericBarCommands.Command.CSTSET, "", arrowPosition, 0);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        public async Task<bool> SetAndWriteMessageAsync(string message, int offset = 0, bool forceClear = true)
        {
            this.messagesToBeSendQueue.Clear();
            if (!this.barEnabled)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_ON);
            }

            if (forceClear)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.CLEAR);
            }

            this.EnqueueCommand(AlphaNumericBarCommands.Command.SET, message, offset);
            this.EnqueueCommand(AlphaNumericBarCommands.Command.WRITE);

            return await this.ExecuteCommandsAsync().ConfigureAwait(true); ;
        }

        public async Task<bool> SetAndWriteMessageScrollAsync(string message, int offset = 0, int scrollEnd = 0, bool forceClear = true)
        {
            this.messagesToBeSendQueue.Clear();
            if (!this.barEnabled)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_ON);
            }

            if (forceClear)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.CLEAR);
            }

            this.EnqueueCommand(AlphaNumericBarCommands.Command.SCROLL_ON, message, offset, scrollEnd);
            this.EnqueueCommand(AlphaNumericBarCommands.Command.WRITE);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        public async Task<bool> SetEnabledAsync(bool enable)
        {
            this.messagesToBeSendQueue.Clear();

            if (enable)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_ON);
            }
            else
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_OFF);
            }

            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        public async Task<bool> SetLuminosityAsync(int luminosity)
        {
            this.messagesToBeSendQueue.Clear();
            if (luminosity > 15)
            {
                luminosity = 15;
            }
            else if (luminosity < 0)
            {
                luminosity = 0;
            }

            this.EnqueueCommand(AlphaNumericBarCommands.Command.SET_LUM, AlphaNumericBarCommands.Command.SET_LUM.ToString(), luminosity);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        public async Task<bool> SetScrollEnabledAsync(bool enable)
        {
            this.messagesToBeSendQueue.Clear();

            if (enable)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.SCROLL_ON);
            }
            else
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.SCROLL_OFF);
            }

            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        public async Task<bool> SetTestAsync(bool enable)
        {
            this.messagesToBeSendQueue.Clear();

            if (enable)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.TEST_ON);
            }
            else
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.TEST_OFF);
            }

            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        private string EnqueueCommand(AlphaNumericBarCommands.Command command, string message = null, int offset = 0, int scrollEnd = 0)
        {
            var strCommand = command.ToString();

            switch (command)
            {
                case AlphaNumericBarCommands.Command.ENABLE_ON:
                    strCommand = "ENABLE ON";
                    this.barEnabled = true;
                    break;

                case AlphaNumericBarCommands.Command.ENABLE_OFF:
                    strCommand = "ENABLE OFF";
                    this.barEnabled = false;
                    break;

                case AlphaNumericBarCommands.Command.TEST_ON:
                    strCommand = "TEST ON";
                    break;

                case AlphaNumericBarCommands.Command.TEST_OFF:
                    strCommand = "TEST OFF";
                    break;

                case AlphaNumericBarCommands.Command.DIM:                     // DIM <valueX>
                case AlphaNumericBarCommands.Command.OFFSET:                  // OFFSET <value>
                    strCommand += " " + offset;
                    break;

                case AlphaNumericBarCommands.Command.SET:                       // SET <offset> <string>
                    strCommand += " " + offset;
                    if (message != null)
                    {
                        strCommand += " " + message;
                    }
                    break;

                case AlphaNumericBarCommands.Command.CUSTOM:                    // CUSTOM <index> <hexval>
                    if (message != null)
                    {
                        strCommand += " " + message;
                    }
                    break;

                case AlphaNumericBarCommands.Command.CSTSET:                    // CSTSET <offset> <index>
                    strCommand += " " + offset + " " + scrollEnd;
                    break;

                case AlphaNumericBarCommands.Command.SCROLL_ON:                 // TODO: must be cheked
                    strCommand = "SCROLL ON " + offset + " " + scrollEnd;
                    if (message != null)
                    {
                        strCommand += " " + message;
                    }
                    break;

                case AlphaNumericBarCommands.Command.SCROLL_OFF:
                    strCommand = "SCROLL OFF";
                    break;

                case AlphaNumericBarCommands.Command.SET_LUM:
                    strCommand = "SETLUM " + offset;
                    break;
            }

            strCommand += "\r\n";

            this.messagesToBeSendQueue.Enqueue(strCommand);

            return strCommand;
        }

        /// <summary>
        /// Send all commands in the queue to alpanumeric bar
        /// </summary>
        /// <returns>True if all commands are send and the response is OK, else false</returns>
        private bool ExecuteCommands()
        {
            var result = false;
            try
            {
                using (var client = new TcpClient())
                {
                    this.messagesReceivedQueue.Clear();
                    client.SendTimeout = this.tcpTimeout;
                    client.Connect(this.IpAddress, this.Port);
                    var stream = client.GetStream();

                    foreach (string sendMessage in this.messagesToBeSendQueue)
                    {
                        var data = Encoding.ASCII.GetBytes(sendMessage);
                        stream = client.GetStream();
                        stream.Write(data, 0, data.Length);
                        System.Diagnostics.Debug.WriteLine("ExecuteCommands();Sent: {0}", sendMessage);

                        data = new byte[256];
                        var bytes = stream.Read(data, 0, data.Length);
                        var responseMessage = Encoding.ASCII.GetString(data, 0, bytes);
                        this.messagesReceivedQueue.Enqueue(responseMessage);
                        if (!this.IsResponseOk(sendMessage, responseMessage))
                        {
                            throw new System.ArgumentException($"AplhaNumericBar;ExecuteCommands;ArgumentException;{sendMessage},{responseMessage}");
                        }

                        System.Diagnostics.Debug.WriteLine("ExecuteCommands();Received: {0}", responseMessage);
                    }

                    stream.Close();
                    client.Close();
                }

                result = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return result;
        }

        private async Task<bool> ExecuteCommandsAsync()
        {
            var result = false;
            try
            {
                using (var client = new TcpClient())
                {
                    client.SendTimeout = this.tcpTimeout;
                    await client.ConnectAsync(this.IpAddress, this.Port).ConfigureAwait(true);
                    var stream = client.GetStream();

                    foreach (string sendMessage in this.messagesToBeSendQueue)
                    {
                        var data = Encoding.ASCII.GetBytes(sendMessage);
                        stream = client.GetStream();
                        stream.Write(data, 0, data.Length);
                        System.Diagnostics.Debug.WriteLine("ExecuteCommands();Sent: {0}", sendMessage);

                        data = new byte[256];
                        var bytes = stream.Read(data, 0, data.Length);
                        var responseMessage = Encoding.ASCII.GetString(data, 0, bytes);

                        if (!this.IsResponseOk(sendMessage, responseMessage))
                        {
                            throw new System.ArgumentException($"AplhaNumericBar;ExecuteCommands;ArgumentException;{sendMessage},{responseMessage}");
                        }

                        System.Diagnostics.Debug.WriteLine("ExecuteCommands();Received: {0}", responseMessage);
                    }

                    stream.Close();
                    client.Close();
                }

                result = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return result;
        }

        /// <summary>
        /// Check the string response from alpha numeric bar is OK
        /// </summary>
        /// <param name="msgSend"></param>
        /// <param name="msgReceive"></param>
        /// <returns></returns>
        private bool IsResponseOk(string msgSend, string msgReceive)
        {
            var result = false;
            msgReceive = msgReceive.Trim();

            if (msgSend.StartsWith("GET", StringComparison.Ordinal))
            {
                if (msgReceive.StartsWith("GET", StringComparison.Ordinal))
                {
                    result = true;
                }
            }
            else if (msgSend.StartsWith("TEST", StringComparison.Ordinal))
            {
                if (msgReceive == "TEST ON OK" ||
                    msgReceive == "TEST OFF OK")
                {
                    result = true;
                }
            }
            else if (msgSend.StartsWith("HELP", StringComparison.Ordinal))
            {
                if (msgReceive.StartsWith("DIM", StringComparison.Ordinal))
                {
                    result = true;
                }
            }
            else
            {
                if (msgReceive == AlphaNumericBarCommands.OK)
                {
                    result = true;
                }
            }
            return result;
        }

        #endregion
    }
}

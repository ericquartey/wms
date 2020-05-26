using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using static Ferretto.VW.Devices.AlphaNumericBar.AlphaNumericBarCommands;

namespace Ferretto.VW.Devices.AlphaNumericBar
{
    public class AlphaNumericBarDriver : IAlphaNumericBarDriver
    {
        #region Fields

        public const int PORT_DEFAULT = 2020;

        private const string NEW_LINE = "\r\n";

        private readonly ConcurrentQueue<string> errorsQueue;

        private readonly ConcurrentQueue<string> messagesReceivedQueue;

        private readonly ConcurrentQueue<string> messagesToBeSendQueue;

        private readonly int tcpTimeout = 2000;

        private bool barEnabled = false;

        private IPAddress ipAddress;

        private int port = PORT_DEFAULT;

        private AlphaNumericBarSize size = AlphaNumericBarSize.Medium;

        private bool testEnabled = false;

        private bool testScrollEnabled = false;

        #endregion

        #region Constructors

        public AlphaNumericBarDriver()
        {
            this.StepLedBar = 4.75;
            this.messagesToBeSendQueue = new ConcurrentQueue<string>();
            this.messagesReceivedQueue = new ConcurrentQueue<string>();
            this.errorsQueue = new ConcurrentQueue<string>();
        }

        #endregion

        #region Properties

        public IPAddress IpAddress => this.ipAddress;

        public ConcurrentQueue<string> MessagesReceived => this.messagesReceivedQueue;

        public int NumberOfLeds => ((int)this.size) * 8;

        public int Port => this.port;

        public AlphaNumericBarSize Size => this.size;

        public double StepLedBar { get; set; }

        public bool TestEnabled => this.testEnabled;

        public bool TestScrollEnabled => this.testScrollEnabled;

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

        /// <summary>
        ///
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="message"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Return the offset to put the row in the middel position
        /// </summary>
        /// <returns></returns>
        public int CalculateOffsetArrowMiddlePosition(int delta = 1)
        {
            var result = (((((int)this.size) * 8) - 8) / 2) + delta;

            if (result < 0)
            {
                result = 0;
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ClearAsync()
        {
            this.ClearConcurrentQueue(this.messagesToBeSendQueue);
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

        /// <summary>
        /// Send a CUSTOM command.
        /// </summary>
        /// <param name="hexval"></param>
        /// <returns></returns>
        public async Task<bool> CustomAsync(string hexval)
        {
            this.EnqueueCommand(AlphaNumericBarCommands.Command.CUSTOM, hexval);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Send a DIM command.
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public async Task<bool> DimAsync(int dimension)
        {
            this.ClearConcurrentQueue(this.errorsQueue);
            this.EnqueueCommand(AlphaNumericBarCommands.Command.DIM, null, dimension);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Send a ENABLE command.
        ///
        /// N.B. Not use enable=true, because duplicate message
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public async Task<bool> EnabledAsync(bool enable)
        {
            this.ClearConcurrentQueue(this.errorsQueue);

            if (enable)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_ON);     // deprecated, not use
            }
            else
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_OFF);
            }

            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Send a HELP command.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HelpAsync()
        {
            this.ClearConcurrentQueue(this.errorsQueue);
            this.EnqueueCommand(AlphaNumericBarCommands.Command.HELP);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Send a LUM (luminosity) command.
        /// </summary>
        /// <param name="luminosity"></param>
        /// <returns></returns>
        public async Task<bool> LuminosityAsync(int luminosity)
        {
            this.ClearConcurrentQueue(this.errorsQueue);
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public async Task<bool> ScrollDirAsync(ScrollDirection direction)
        {
            this.EnqueueCommand(AlphaNumericBarCommands.Command.SET_SCROLL_DIR, null, (int)direction);

            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Send a scroll speed command.
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        public async Task<bool> ScrollSpeedAsync(int speed)
        {
            this.EnqueueCommand(AlphaNumericBarCommands.Command.SET_SCROLL_SPEED, null, speed);

            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Send a sequence of SET and WRITE command to show an arrow symbol.
        /// </summary>
        /// <param name="arrowPosition"></param>
        /// <param name="forceClear"></param>
        /// <returns></returns>
        public async Task<bool> SetAndWriteArrowAsync(int arrowPosition, bool forceClear = true)
        {
            this.ClearConcurrentQueue(this.errorsQueue);

            if (forceClear)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.CLEAR);
            }

            this.EnqueueCommand(AlphaNumericBarCommands.Command.CSTSET, "", arrowPosition, 0);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Send a sequence of SET and WRITE command.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="offset"></param>
        /// <param name="forceClear"></param>
        /// <returns></returns>
        public async Task<bool> SetAndWriteMessageAsync(string message, int offset = 0, bool forceClear = true)
        {
            this.ClearConcurrentQueue(this.errorsQueue);

            //this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_OFF);    // mandatory, otherwise see duplicated message

            if (forceClear)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.CLEAR);
            }

            this.EnqueueCommand(AlphaNumericBarCommands.Command.SCROLL_OFF, message, offset);
            this.EnqueueCommand(AlphaNumericBarCommands.Command.SET, message, offset);
            this.EnqueueCommand(AlphaNumericBarCommands.Command.WRITE);

            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Send a sequence of SET and WRITE command width scroll.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="offset"></param>
        /// <param name="scrollEnd"></param>
        /// <param name="forceClear"></param>
        /// <returns></returns>
        public async Task<bool> SetAndWriteMessageScrollAsync(string message, int offset = 0, int scrollEnd = 0, bool forceClear = true)
        {
            this.ClearConcurrentQueue(this.errorsQueue);
            //this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_OFF);    // mandatory, otherwise see duplicated message

            if (forceClear)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.CLEAR);
            }

            this.EnqueueCommand(AlphaNumericBarCommands.Command.SCROLL_ON, message, offset, scrollEnd);
            this.EnqueueCommand(AlphaNumericBarCommands.Command.WRITE);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <param name="offset"></param>
        /// <param name="forceClear"></param>
        /// <returns></returns>
        public async Task<bool> SetCustomCharacterAsync(int index, int offset, bool forceClear = true)
        {
            this.ClearConcurrentQueue(this.errorsQueue);

            if (forceClear)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.CLEAR);
            }

            this.EnqueueCommand(AlphaNumericBarCommands.Command.CSTSET, null, offset, index);
            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public async Task<bool> SetScrollEnabledAsync(bool enable)
        {
            this.ClearConcurrentQueue(this.errorsQueue);

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

        /// <summary>
        /// Send a TEST command.
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public async Task<bool> TestAsync(bool enable)
        {
            this.ClearConcurrentQueue(this.errorsQueue);

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

        /// <summary>
        /// Send a TEST SCROLL command.
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public async Task<bool> TestScrollAsync(bool enable)
        {
            this.ClearConcurrentQueue(this.errorsQueue);

            if (enable)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.TEST_SCROLL_ON);
            }
            else
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.TEST_SCROLL_OFF);
            }

            return await this.ExecuteCommandsAsync().ConfigureAwait(true);
        }

        private bool ClearConcurrentQueue(ConcurrentQueue<string> concurrentQueure)
        {
            while (concurrentQueure.TryDequeue(out var sendMessage)) { }
            return true;
        }

        /// <summary>
        /// Encode a trinh into the HTPP protocol.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string Encode(string str)
        {
            //return string.IsNullOrEmpty(str) ? Uri.EscapeDataString(" ") : Uri.EscapeDataString(str);
            return string.IsNullOrEmpty(str) ? Uri.EscapeDataString("+") : Uri.EscapeDataString(str).Replace("%20", "+");
            //return string.IsNullOrEmpty(str) ? HttpUtility.UrlEncode(" ", Encoding.UTF8) : HttpUtility.UrlEncode(str, Encoding.UTF8);
            //return string.IsNullOrEmpty(str) ? WebUtility.HtmlEncode(" ") : WebUtility.HtmlEncode(str);
        }

        /// <summary>
        /// Trasform a command into a message with the right parameters and put into a queue.
        /// The stringe "message" is encode in HTTP format.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="message"></param>
        /// <param name="offset"></param>
        /// <param name="scrollEnd"></param>
        /// <returns></returns>
        private bool EnqueueCommand(AlphaNumericBarCommands.Command command, string message = null, int offset = 0, int scrollEnd = 0)
        {
            var result = false;
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

                case AlphaNumericBarCommands.Command.TEST_ON:                   // deprecated, not use
                    strCommand = "TEST ON";
                    this.testEnabled = true;
                    break;

                case AlphaNumericBarCommands.Command.TEST_OFF:
                    strCommand = "TEST OFF";
                    this.testEnabled = false;
                    break;

                case AlphaNumericBarCommands.Command.TEST_SCROLL_ON:
                    strCommand = "TESTSCROLL ON";
                    this.testScrollEnabled = true;
                    break;

                case AlphaNumericBarCommands.Command.TEST_SCROLL_OFF:
                    strCommand = "TESTSCROLL OFF";
                    this.testScrollEnabled = false;
                    break;

                case AlphaNumericBarCommands.Command.DIM:                     // DIM <valueX>
                case AlphaNumericBarCommands.Command.OFFSET:                  // OFFSET <value>
                    strCommand += " " + offset;
                    break;

                case AlphaNumericBarCommands.Command.SET:                       // SET <offset> <string>
                    strCommand += " " + offset + " " + this.Encode(message);
                    break;

                case AlphaNumericBarCommands.Command.CUSTOM:                    // CUSTOM <index> <hexval>
                    strCommand += " " + this.Encode(message);
                    break;

                case AlphaNumericBarCommands.Command.CSTSET:                    // CSTSET <offset> <index>
                    strCommand += " " + offset + " " + scrollEnd;
                    break;

                case AlphaNumericBarCommands.Command.SCROLL_ON:                 // TODO: must be cheked
                    strCommand = "SCROLL ON " + offset + " " + scrollEnd + " " + this.Encode(message);
                    break;

                case AlphaNumericBarCommands.Command.SCROLL_OFF:
                    strCommand = "SCROLL OFF";
                    break;

                case AlphaNumericBarCommands.Command.SET_SCROLL_SPEED:
                    strCommand = "SETSCROLLSPEED " + offset;
                    break;

                case AlphaNumericBarCommands.Command.SET_SCROLL_DIR:
                    strCommand = "SETSCROLLDIR " + offset;
                    break;

                case AlphaNumericBarCommands.Command.SET_LUM:
                    strCommand = "SETLUM " + offset;
                    break;
            }

            strCommand += NEW_LINE;

            this.messagesToBeSendQueue.Enqueue(strCommand);
            result = true;

            return result;
        }

        /// <summary>
        /// Send the messages int the queue, in the right order.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ExecuteCommandsAsync()
        {
            var result = false;
            try
            {
                this.ClearConcurrentQueue(this.errorsQueue);
                var client = new TcpClient();
                NetworkStream stream = null;

                while (!this.messagesToBeSendQueue.IsEmpty)
                {
                    try
                    {
                        if (this.messagesToBeSendQueue.TryDequeue(out var sendMessage))
                        {
                            if (!client.Connected)
                            {
                                client.SendTimeout = this.tcpTimeout;
                                await client.ConnectAsync(this.IpAddress, this.Port).ConfigureAwait(true);
                                stream = client.GetStream();
                            }

                            var data = Encoding.ASCII.GetBytes(sendMessage);
                            stream = client.GetStream();
                            stream.ReadTimeout = this.tcpTimeout;
                            stream.Write(data, 0, data.Length);
                            System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss};AplhaNumericBarDriver;ExecuteCommands();Sent: {sendMessage}");

                            if (this.IsWaitResponse(sendMessage))
                            {
                                data = new byte[client.ReceiveBufferSize];
                                var bytes = stream.Read(data, 0, data.Length);
                                var responseMessage = Encoding.ASCII.GetString(data, 0, bytes);

                                this.messagesReceivedQueue.Enqueue(responseMessage);
                                System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss};AplhaNumericBarDriver;ExecuteCommands();Received: {responseMessage}");
                                if (!this.IsResponseOk(sendMessage, responseMessage))
                                {
                                    System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss)};AplhaNumericBarDriver;ExecuteCommands;ArgumentException;{sendMessage},{responseMessage}");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss)};AplhaNumericBarDriver;ExecuteCommands;ArgumentException;no wait {sendMessage}");
                                this.messagesReceivedQueue.Enqueue("");
                            }
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss};AplhaNumericBarDriver;{e.Message}");
                    }
                }

                stream?.Close();
                client?.Close();
                result = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss};AplhaNumericBarDriver;{e.Message}");
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

            if (string.IsNullOrEmpty(msgSend) || string.IsNullOrEmpty(msgReceive))
            {
                return true;
            }

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

        /// <summary>
        /// Check if the client must wait the response from the server.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool IsWaitResponse(string message)
        {
            var result = true;

            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            if (message.StartsWith("CLEAR", StringComparison.Ordinal) || message.StartsWith("ENABLE", StringComparison.Ordinal) || message.StartsWith("TEST OFF", StringComparison.Ordinal))
            {
                result = false;
            }

            return result;
        }

        #endregion
    }
}

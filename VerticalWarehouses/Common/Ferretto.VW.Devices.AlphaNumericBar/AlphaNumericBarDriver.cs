﻿using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using NLog;
using static Ferretto.VW.Devices.AlphaNumericBar.AlphaNumericBarCommands;

namespace Ferretto.VW.Devices.AlphaNumericBar
{
    internal sealed class AlphaNumericBarDriver : IAlphaNumericBarDriver
    {
        #region Fields

        public const int PORT_DEFAULT = 2020;

        private const string NEW_LINE = "\r\n";

        private readonly ConcurrentQueue<string> errorsQueue = new ConcurrentQueue<string>();

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentQueue<string> messagesReceivedQueue = new ConcurrentQueue<string>();

        private readonly ConcurrentQueue<string> messagesToBeSendQueue = new ConcurrentQueue<string>();

        private readonly int tcpTimeout = 2000;

        private bool barEnabled;

        private IPAddress ipAddress;

        private AlphaNumericBarSize size = AlphaNumericBarSize.Medium;

        private bool testEnabled;

        private bool testScrollEnabled;

        #endregion

        #region Properties

        public IPAddress IpAddress => this.ipAddress;

        public ConcurrentQueue<string> MessagesReceived => this.messagesReceivedQueue;

        public int NumberOfLeds => ((int)this.size) * 8;

        public int Port { get; private set; } = PORT_DEFAULT;

        public AlphaNumericBarSize Size => this.size;

        public double StepLedBar { get; set; } = 4.75;

        public bool TestEnabled => this.testEnabled;

        public bool TestScrollEnabled => this.testScrollEnabled;

        #endregion

        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <param name="compartmentWidth"></param>
        /// <param name="itemXPosition"></param>
        /// <returns></returns>
        public int CalculateArrowPosition(double compartmentWidth, double itemXPosition)
        {
            var arrowPosition = (compartmentWidth / 2) + itemXPosition;
            var pixelOffset = (arrowPosition / this.StepLedBar) + 2;
            return (int)Math.Round((pixelOffset));
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

        public void Configure(IPAddress ipAddress, int port, AlphaNumericBarSize size)
        {
            this.ipAddress = ipAddress;
            this.Port = port;
            this.size = size;
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
            if (enable == this.barEnabled)
            {
                return true;
            }

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

            this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_OFF);

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
            while (concurrentQueure.TryDequeue(out _)) { }
            return true;
        }

        /// <summary>
        /// Replace the Ascii characters out side the available range width a space (char '+')
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string Encode(string str)
        {
            var result = "";

            foreach (var c in str)
            {
                if (((int)c > 32 && (int)c < 126) || (int)c == 176 || (int)c == 216)
                {
                    result += Uri.EscapeDataString(c.ToString());
                }
                else
                {
                    result += Uri.EscapeDataString(" ");
                }
            }

            return result;
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
            bool result;
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

                case AlphaNumericBarCommands.Command.WRITE:
                    this.barEnabled = true;
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
                            this.logger.Debug($"ExecuteCommands();Sent: {sendMessage}");

                            if (this.IsWaitResponse(sendMessage))
                            {
                                data = new byte[client.ReceiveBufferSize];
                                var bytes = stream.Read(data, 0, data.Length);
                                var responseMessage = Encoding.ASCII.GetString(data, 0, bytes);

                                this.messagesReceivedQueue.Enqueue(responseMessage);
                                this.logger.Debug($"ExecuteCommands();Received: {responseMessage}");
                                if (!this.IsResponseOk(sendMessage, responseMessage))
                                {
                                    this.logger.Debug($"ExecuteCommands;ArgumentException;{sendMessage},{responseMessage}");
                                }
                            }
                            else
                            {
                                this.logger.Debug($"ArgumentException;no wait {sendMessage}");
                                this.messagesReceivedQueue.Enqueue("");
                            }
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error(ex);
                    }
                }

                stream?.Close();
                client?.Close();
                result = true;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
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

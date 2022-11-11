using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using NLog;
using static Ferretto.VW.Devices.AlphaNumericBar.AlphaNumericBarCommands;

namespace Ferretto.VW.Devices.AlphaNumericBar
{
    internal sealed class AlphaNumericBarDriver : IAlphaNumericBarDriver
    {
        #region Fields

        public const int LED_INTO_CHAR = 6;

        public const int PORT_DEFAULT = 2020;

        //private const int MAX_MESSAGE_LENGTH = 125;

        private const string NEW_LINE = "\r\n";

        //private readonly ConcurrentQueue<string> errorsQueue = new ConcurrentQueue<string>();

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentQueue<string> messagesReceivedQueue = new ConcurrentQueue<string>();

        private readonly ConcurrentQueue<string> messagesToBeSendQueue = new ConcurrentQueue<string>();

        private readonly int tcpTimeout = 2000;

        private bool barEnabled;

        private bool clearOnClose;

        private TcpClient client;

        private int errorCount;

        private IPAddress ipAddress;

        private int ledHideOnLeftSide;

        private int ledHideOnRightSide;

        private double loadingUnitWidth;

        private int maxMessageLength;

        private List<string> messageFields;

        private int savedOffset;

        private string savedScrollMsg;

        private string savedSetMsg;

        private AlphaNumericBarSize size = AlphaNumericBarSize.ExtraLarge;

        private NetworkStream stream = null;

        //private double? selectedPosition;
        private bool testEnabled;

        //private string selectedMessage;
        private bool testScrollEnabled;

        private bool useGet;

        #endregion

        #region Properties

        public bool HasGetErrors { get; set; }

        public IPAddress IpAddress => this.ipAddress;

        public bool IsConnected => this.client?.Connected ?? false;

        public bool IsTestLoop { get; set; }

        public int MaxMessageLength => this.maxMessageLength;

        public ConcurrentQueue<string> MessagesReceived => this.messagesReceivedQueue;

        public int NumberOfLeds => ((int)this.size) * 8;

        public int Port { get; private set; } = PORT_DEFAULT;

        public string SelectedMessage { get; set; }

        public double? SelectedPosition { get; set; }

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

        public void ClearCommands()
        {
            ClearConcurrentQueue(this.messagesToBeSendQueue);
            ClearConcurrentQueue(this.messagesReceivedQueue);
            //this.ClearConcurrentQueue(this.errorsQueue);
            this.SelectedMessage = null;
            this.savedSetMsg = null;
            this.savedScrollMsg = null;
        }

        public void Configure(
            IPAddress ipAddress,
            int port,
            AlphaNumericBarSize size,
            bool bayIsExternal = false,
            int maxMessageLength = 125,
            bool clearOnClose = false,
            bool useGet = false,
            List<string> messageFields = null)
        {
            this.ipAddress = ipAddress;
            this.Port = port;
            this.size = size;
            this.maxMessageLength = maxMessageLength;
            this.clearOnClose = clearOnClose;
            this.useGet = useGet;
            this.messageFields = messageFields;

            switch (size)
            {
                case AlphaNumericBarSize.ExtraExtraSmall:
                    this.loadingUnitWidth = 1650;
                    break;

                case AlphaNumericBarSize.ExtraSmall:
                    this.loadingUnitWidth = 1950;
                    break;

                case AlphaNumericBarSize.Small:
                    this.loadingUnitWidth = 2450;
                    break;

                case AlphaNumericBarSize.Medium:
                    this.loadingUnitWidth = 3050;
                    break;

                case AlphaNumericBarSize.Large:
                    this.loadingUnitWidth = 3650;
                    break;

                default:    // ExtraLarge
                    this.loadingUnitWidth = 4250;
                    break;
            }

            if (!bayIsExternal)         // for BIG, BIS and BID bay (no external), the first 8 leds are hide
            {
                if (size == AlphaNumericBarSize.ExtraSmall)  // for XS width, all led are available
                {
                    this.ledHideOnLeftSide = 0;
                }
                else
                {
                    this.ledHideOnLeftSide = 8;
                }

                this.ledHideOnRightSide = 0;
            }
        }

        public async Task ConnectAsync()
        {
            try
            {
                if (this.client is null)
                {
                    this.client = new TcpClient();
                    this.stream = null;
                }
                if (!this.IsConnected)
                {
                    this.logger.Debug($"Connect");
                    this.client.SendTimeout = this.tcpTimeout;
                    await this.client.ConnectAsync(this.IpAddress, this.Port).ConfigureAwait(true);
                    this.stream = this.client.GetStream();
                    this.logger.Debug($"Connected");
                }
            }
            catch (Exception e)
            {
                this.logger.Error(e);
                this.Disconnect();
            }
        }

        /// <summary>
        /// Send a CUSTOM command.
        /// </summary>
        /// <param name="hexval"></param>
        /// <returns></returns>
        public async Task<bool> CustomAsync(string hexval)
        {
            this.EnqueueCommand(AlphaNumericBarCommands.Command.CUSTOM, hexval);
            return true;
        }

        /// <summary>
        /// Send a DIM command.
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public async Task<bool> DimAsync(int dimension)
        {
            //this.ClearConcurrentQueue(this.errorsQueue);
            this.EnqueueCommand(AlphaNumericBarCommands.Command.DIM, null, dimension);
            return true;
        }

        public void Disconnect()
        {
            try
            {
                this.logger.Debug($"Disconnect");
                this.stream?.Close();
                this.client?.Close();
                this.client = null;
            }
            catch (Exception e)
            {
                this.logger.Error(e);
            }
        }

        /// <summary>
        ///  Send a ENABLE command.
        /// </summary>
        /// <param name="enable">Set the enable</param>
        /// <param name="force">If true force the enable command</param>
        /// <returns></returns>
        public async Task<bool> EnabledAsync(bool enable, bool force = true)
        {
            //this.ClearConcurrentQueue(this.errorsQueue);
            ClearConcurrentQueue(this.messagesReceivedQueue);
            this.savedSetMsg = null;
            this.savedScrollMsg = null;

            if (enable == this.barEnabled && !force)
            {
                return true;
            }

            if (enable)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_ON);     // deprecated, not use
            }
            else
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_OFF);
            }

            return true;
        }

        /// <summary>
        /// Send the messages int the queue, in the right order.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ExecuteCommandsAsync(CancellationToken? cancellationToken)
        {
            var result = false;
            try
            {
                //this.ClearConcurrentQueue(this.messagesReceivedQueue);
                //this.ClearConcurrentQueue(this.errorsQueue);
                int errors = 0;

                while (!this.messagesToBeSendQueue.IsEmpty
                    && (!cancellationToken.HasValue || !cancellationToken.Value.IsCancellationRequested)
                    )
                {
                    if (this.messagesToBeSendQueue.TryPeek(out var sendMessage))
                    {
                        var ackReceived = false;
                        if (!this.IsConnected)
                        {
                            await this.ConnectAsync();
                        }

                        if (this.IsConnected)
                        {
                            Thread.Sleep(10);
                            this.logger.Trace($"ExecuteCommands();Write");
                            var data = Encoding.ASCII.GetBytes(sendMessage);
                            this.stream = this.client.GetStream();
                            this.stream.ReadTimeout = this.tcpTimeout;
                            this.stream.WriteTimeout = this.tcpTimeout;
                            this.stream.Write(data, 0, data.Length);
                            this.logger.Debug($"ExecuteCommands();Sent: {sendMessage.Replace("\r", "<CR>").Replace("\n", "<LF>")}");

                            if (this.IsWaitResponse(sendMessage))
                            {
                                data = new byte[this.client.ReceiveBufferSize];
                                var bytes = 0;
                                var responseMessage = "";
                                try
                                {
                                    bytes = this.stream.Read(data, 0, data.Length);
                                    responseMessage = Encoding.ASCII.GetString(data, 0, bytes);

                                    this.messagesReceivedQueue.Enqueue(responseMessage);
                                    this.logger.Debug($"ExecuteCommands();Received: {responseMessage.Replace("\r", "<CR>").Replace("\n", "<LF>")}");
                                }
                                catch (Exception e)
                                {
                                    this.logger.Debug(e);
                                }
                                if (bytes <= 0 || !this.IsResponseOk(sendMessage, responseMessage))
                                {
                                    this.logger.Debug($"ExecuteCommands;Response error;{sendMessage.Replace("\r", "<CR>").Replace("\n", "<LF>")},{responseMessage.Replace("\r", "<CR>").Replace("\n", "<LF>")}");
                                    if (errors++ > 5)
                                    {
                                        this.ClearCommands();
                                        this.logger.Error($"ExecuteCommands: too many errors!");
                                        this.Disconnect();
                                        Thread.Sleep(400);
                                        break;
                                    }
                                }
                                else
                                {
                                    ackReceived = true;
                                }
                            }
                            else
                            {
                                //this.logger.Debug($"ArgumentException;no wait {sendMessage}");
                                this.messagesReceivedQueue.Enqueue("");
                                ackReceived = true;
                            }
                        }
                        else
                        {
                            // retry
                            Thread.Sleep(1000);
                            await this.ConnectAsync();
                            if (!this.IsConnected)
                            {
                                this.logger.Error("Device not ready");
                                this.ClearCommands();
                                break;
                            }
                        }
                        if (ackReceived)
                        {
                            this.messagesToBeSendQueue.TryDequeue(out _);
                            errors = 0;
                        }
                    }
                    else
                    {
                        this.logger.Debug("queue locked");
                        break;
                    }
                }

                result = true;
            }
            catch (Exception e)
            {
                this.ClearCommands();
                this.logger.Error(e);
                this.Disconnect();
                Thread.Sleep(400);
            }

            return result;
        }

        public string GetMessageFromWmsOperation(MissionOperation operation)
        {
            var message = "?";

            switch (operation.Type)
            {
                case MissionOperationType.Pick:
                    message = "-";
                    break;

                case MissionOperationType.Put:
                    message = "+";
                    break;
            }
            message += (operation.RequestedQuantity - operation.DispatchedQuantity);

            if (this.messageFields != null && this.messageFields.Any())
            {
                foreach (var field in this.messageFields)
                {
                    if (!string.IsNullOrEmpty(field))
                    {
                        switch (field)
                        {
                            case "ItemCode":
                                message += " " + operation.ItemCode;
                                break;

                            case "ItemDescription":
                                message += " " + operation.ItemDescription;
                                break;

                            case "Destination":
                                message += " " + operation.Destination;
                                break;

                            case "ItemListCode":
                                message += " " + operation.ItemListCode;
                                break;

                            case "ItemListDescription":
                                message += " " + operation.ItemListDescription;
                                break;

                            case "ItemListRowCode":
                                message += " " + operation.ItemListRowCode;
                                break;

                            case "ItemNotes":
                                message += " " + operation.ItemNotes;
                                break;

                            case "Lot":
                                message += " " + operation.Lot;
                                break;

                            case "SerialNumber":
                                message += " " + operation.SerialNumber;
                                break;

                            case "Sscc":
                                message += " " + operation.Sscc;
                                break;
                        }
                    }
                }
            }

            return message;
        }

        public bool GetOffsetArrowAndMessage(double x, string message, out int offsetArrow, out int offsetMessage, out int scrollEnd)
        {
            var result = false;

            if (x < 0)
            {
                x = 0;
            }
            else if (x > this.loadingUnitWidth)
            {
                x = this.loadingUnitWidth;
            }

            // set the arrow offset
            offsetArrow = (int)Math.Round(((int)this.Size) * 8 / this.loadingUnitWidth * x) - 2;     // note, sub 2 because the arrow is in the middle

            if (offsetArrow < this.ledHideOnLeftSide)
            {
                offsetArrow = this.ledHideOnLeftSide;
            }

            if (offsetArrow + LED_INTO_CHAR >= this.NumberOfLeds - this.ledHideOnRightSide) // if the message goes out to the right side of alphanumeric bar, then move toward left the message
            {
                offsetArrow = this.NumberOfLeds - this.ledHideOnRightSide - LED_INTO_CHAR;
                if (offsetArrow < this.ledHideOnLeftSide)
                {
                    offsetArrow = this.ledHideOnLeftSide;
                }
            }

            // set the message offset
            offsetMessage = offsetArrow + LED_INTO_CHAR + 1;    // standard position, message on the right side of the arrow

            if (offsetMessage + (message.Length * LED_INTO_CHAR) >= this.NumberOfLeds - this.ledHideOnRightSide) // if the message goes out to the right side of alphanumeric bar, then move toward left the message
            {
                if (offsetArrow < this.NumberOfLeds - offsetArrow + LED_INTO_CHAR - this.ledHideOnRightSide) // there is more space after the arrow
                {
                }
                else // there is more space before the arrow
                {
                    offsetMessage = offsetArrow - (message.Length * LED_INTO_CHAR);
                }
            }

            if (offsetMessage < this.ledHideOnLeftSide)
            {
                offsetMessage = this.ledHideOnLeftSide;
            }

            if (offsetMessage > offsetArrow && this.NumberOfLeds < offsetMessage + (message.Length * LED_INTO_CHAR))
            {
                scrollEnd = this.NumberOfLeds / LED_INTO_CHAR;
            }
            else if (offsetMessage < offsetArrow && offsetArrow < offsetMessage + (message.Length * LED_INTO_CHAR))
            {
                scrollEnd = offsetArrow / LED_INTO_CHAR;
            }
            else
            {
                scrollEnd = 0;
            }
            return result;
        }

        public bool GetOffsetArrowAndMessageFromCompartment(double compartmentWidth, double itemXPosition, string message, double loadingUnitWidth, WarehouseSide side, out int offsetArrow, out int offsetMessage, out int scrollEnd)
        {
            var frontPosition = (compartmentWidth / 2) + itemXPosition;
            var arrowPosition = (side == WarehouseSide.Back) ?
                    loadingUnitWidth - frontPosition :
                    frontPosition;
            return this.GetOffsetArrowAndMessage(arrowPosition, message, out offsetArrow, out offsetMessage, out scrollEnd);
        }

        public bool GetOffsetMessage(double x, string message, out int offsetMessage)
        {
            var result = false;

            if (x < 0)
            {
                x = 0;
            }
            else if (x > this.loadingUnitWidth)
            {
                x = this.loadingUnitWidth;
            }

            // set the message offset
            offsetMessage = (int)Math.Round(((int)this.Size) * 8 / this.loadingUnitWidth * x);

            if (offsetMessage + (message.Length * 6) >= this.NumberOfLeds - this.ledHideOnRightSide) // if the message goes out to the right side of alphanumeric bar, then move toward left the message
            {
                offsetMessage = this.NumberOfLeds - (message.Length * 6) - this.ledHideOnRightSide;
            }

            if (offsetMessage < this.ledHideOnLeftSide)
            {
                offsetMessage = this.ledHideOnLeftSide;
            }

            return result;
        }

        /// <summary>
        /// Send a HELP command.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HelpAsync()
        {
            //this.ClearConcurrentQueue(this.errorsQueue);
            this.EnqueueCommand(AlphaNumericBarCommands.Command.HELP);
            return true;
        }

        /// <summary>
        /// Send a LUM (luminosity) command.
        /// </summary>
        /// <param name="luminosity"></param>
        /// <returns></returns>
        public async Task<bool> LuminosityAsync(int luminosity)
        {
            //this.ClearConcurrentQueue(this.errorsQueue);
            if (luminosity > 15)
            {
                luminosity = 15;
            }
            else if (luminosity < 0)
            {
                luminosity = 0;
            }

            this.EnqueueCommand(AlphaNumericBarCommands.Command.SET_LUM, AlphaNumericBarCommands.Command.SET_LUM.ToString(), luminosity);
            return true;
        }

        public string NormalizeMessageCharacters(string str)
        {
            var result = str.Trim();

            result = result.Replace("…", "...");
            result = result.Replace("„", ",,");

            result = result.Replace("‘", "'");
            result = result.Replace("’", "'");
            result = result.Replace("“", "\"");
            result = result.Replace("”", "\"");

            result = result.Replace("–", "-");
            result = result.Replace("—", "-");
            result = result.Replace("¯", "-");

            result = result.Replace("¼", "1/4");
            result = result.Replace("½", "1/2");
            result = result.Replace("¾", "3/4");

            result = result.Replace("‰", "0/00");

            result = result.Replace("‹", "<");
            result = result.Replace("›", ">");

            result = result.Replace("«", "<<");
            result = result.Replace("»", ">>");

            result = result.Replace("à", "a'");
            result = result.Replace("á", "a'");
            result = result.Replace("À", "A'");
            result = result.Replace("Á", "A'");

            result = result.Replace("Â", "A");
            result = result.Replace("Ã", "A");
            result = result.Replace("Ä", "A");
            result = result.Replace("Å", "A");
            result = result.Replace("Æ", "AE");

            result = result.Replace("â", "a");
            result = result.Replace("ã", "a");
            result = result.Replace("ä", "a");
            result = result.Replace("å", "a");
            result = result.Replace("æ", "ae");

            result = result.Replace("¢", "c");

            result = result.Replace("Ð", "D");

            result = result.Replace("è", "e'");
            result = result.Replace("é", "e'");
            result = result.Replace("È", "E'");
            result = result.Replace("É", "E'");

            result = result.Replace("Ê", "E");
            result = result.Replace("Ë", "E");

            result = result.Replace("€", "EUR");

            result = result.Replace("ƒ", "f");

            result = result.Replace("ì", "i'");
            result = result.Replace("í", "i'");
            result = result.Replace("Ì", "I'");
            result = result.Replace("Í", "I'");

            result = result.Replace("î", "i");
            result = result.Replace("ï", "i");

            result = result.Replace("Î", "I");
            result = result.Replace("Ï", "I");

            result = result.Replace("ñ", "n");
            result = result.Replace("Ñ", "N");

            result = result.Replace("ò", "o'");
            result = result.Replace("ó", "o'");
            result = result.Replace("Ò", "O'");
            result = result.Replace("Ó", "O'");

            result = result.Replace("ô", "o");
            result = result.Replace("õ", "o");
            result = result.Replace("ö", "o");

            result = result.Replace("Ô", "O");
            result = result.Replace("Õ", "O");
            result = result.Replace("Ö", "O");

            result = result.Replace("œ", "oe");
            result = result.Replace("Œ", "OE");

            result = result.Replace("š", "s");
            result = result.Replace("Š", "S");

            result = result.Replace("ù", "u'");
            result = result.Replace("ú", "u'");
            result = result.Replace("Ù", "U'");
            result = result.Replace("Ú", "U'");

            result = result.Replace("™", "TM");

            result = result.Replace("û", "u");
            result = result.Replace("ü", "u");
            result = result.Replace("Û", "U");
            result = result.Replace("Ü", "U");

            result = result.Replace("ý", "y'");
            result = result.Replace("Ý", "Y'");
            result = result.Replace("Ÿ", "Y");

            result = result.Replace("¥", "YEN");

            result = result.Replace("ž", "z");
            result = result.Replace("Ž", "Z");

            result = result.Replace("×", "x");

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public async Task<bool> ScrollDirAsync(ScrollDirection direction)
        {
            this.EnqueueCommand(AlphaNumericBarCommands.Command.SET_SCROLL_DIR, null, (int)direction);

            return true;
        }

        /// <summary>
        /// Send a scroll speed command.
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        public async Task<bool> ScrollSpeedAsync(int speed)
        {
            this.EnqueueCommand(AlphaNumericBarCommands.Command.SET_SCROLL_SPEED, null, speed);

            return true;
        }

        /// <summary>
        /// Send a sequence of SET and WRITE command to show an arrow symbol.
        /// </summary>
        /// <param name="arrowPosition"></param>
        /// <param name="forceClear"></param>
        /// <returns></returns>
        public async Task<bool> SetAndWriteArrowAsync(int arrowPosition, bool forceClear = true)
        {
            //this.ClearConcurrentQueue(this.errorsQueue);

            if (forceClear)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.CLEAR);
            }

            this.EnqueueCommand(AlphaNumericBarCommands.Command.CSTSET, "", arrowPosition, 0);
            this.errorCount = 0;
            return true;
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
            //this.ClearConcurrentQueue(this.errorsQueue);
            ClearConcurrentQueue(this.messagesReceivedQueue);
            this.savedSetMsg = null;
            this.savedScrollMsg = null;

            //this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_OFF);    // mandatory, otherwise see duplicated message

            if (forceClear)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.CLEAR);
            }

            this.EnqueueCommand(AlphaNumericBarCommands.Command.SCROLL_OFF, message, offset);
            this.EnqueueCommand(AlphaNumericBarCommands.Command.SET, message, offset);
            this.EnqueueCommand(AlphaNumericBarCommands.Command.WRITE);
            if (this.useGet)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.GET);
                this.savedSetMsg = message;
                this.savedOffset = offset;
            }

            return true;
        }

        /// <summary>
        /// Send a sequence of SET and WRITE command with scroll.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="offset"></param>
        /// <param name="scrollEnd"></param>
        /// <param name="forceClear"></param>
        /// <returns></returns>
        public async Task<bool> SetAndWriteMessageScrollAsync(string message, int offset = 0, int scrollEnd = 0, bool forceClear = true)
        {
            //this.ClearConcurrentQueue(this.errorsQueue);
            ClearConcurrentQueue(this.messagesReceivedQueue);
            this.savedSetMsg = null;
            this.savedScrollMsg = null;
            //this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_OFF);    // mandatory, otherwise see duplicated message

            if (forceClear)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.CLEAR);
            }

            this.EnqueueCommand(AlphaNumericBarCommands.Command.SCROLL_ON, message, offset, scrollEnd);
            this.EnqueueCommand(AlphaNumericBarCommands.Command.WRITE);
            if (this.useGet)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.GET);
                this.savedScrollMsg = message;
                this.savedOffset = offset;
            }
            return true;
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
            //this.ClearConcurrentQueue(this.errorsQueue);

            if (forceClear)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.CLEAR);
            }

            this.EnqueueCommand(AlphaNumericBarCommands.Command.CSTSET, null, offset, index);
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public async Task<bool> SetScrollEnabledAsync(bool enable)
        {
            //this.ClearConcurrentQueue(this.errorsQueue);

            if (enable)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.SCROLL_ON);
            }
            else
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.SCROLL_OFF);
            }

            return true;
        }

        /// <summary>
        /// Send a TEST command.
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public async Task<bool> TestAsync(bool enable)
        {
            //this.ClearConcurrentQueue(this.errorsQueue);

            this.EnqueueCommand(AlphaNumericBarCommands.Command.ENABLE_OFF);

            if (enable)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.TEST_ON);
            }
            //else
            //{
            //    this.EnqueueCommand(AlphaNumericBarCommands.Command.TEST_OFF);
            //    this.EnqueueCommand(AlphaNumericBarCommands.Command.CLEAR);
            //}

            return true;
        }

        /// <summary>
        /// Send a TEST SCROLL command.
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public async Task<bool> TestScrollAsync(bool enable)
        {
            //this.ClearConcurrentQueue(this.errorsQueue);

            if (enable)
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.TEST_SCROLL_ON);
            }
            else
            {
                this.EnqueueCommand(AlphaNumericBarCommands.Command.TEST_SCROLL_OFF);
            }

            return true;
        }

        public async Task<bool> TryResendWriteAsync()
        {
            var ret = false;
            if (!this.useGet)
            {
                this.savedSetMsg = null;
                this.savedScrollMsg = null;
                return ret;
            }
            if (!string.IsNullOrEmpty(this.savedSetMsg))
            {
                ret = await this.ResendIfNotGet(this.savedSetMsg);
                if (ret)
                {
                    this.savedSetMsg = null;
                }
            }
            else if (!string.IsNullOrEmpty(this.savedScrollMsg))
            {
                ret = await this.ResendIfNotGet(this.savedScrollMsg);
                if (ret)
                {
                    this.savedScrollMsg = null;
                }
            }
            return ret;
        }

        private static bool ClearConcurrentQueue(ConcurrentQueue<string> concurrentQueue)
        {
            while (concurrentQueue.TryDequeue(out _))
            { }
            return true;
        }

        /// <summary>
        /// Replace the Ascii characters out side the available range width a space (char '+')
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string Encode(string str, int maxLen)
        {
            var result = "";

            int i = 0;
            foreach (var c in str)
            {
                var escapedChar = "";
                // replace more spaces with only one space
                if (c == ' '
                    && i > 0
                    && str[i - 1] == ' ')
                {
                    // do nothing
                }
                else if (//c == ' '||
                         c == '*')
                {
                    escapedChar = c.ToString();
                }
                else if (((int)c > 32 && (int)c < 126))
                {
                    escapedChar = Uri.EscapeDataString(c.ToString());
                }
                else
                {
                    escapedChar = Uri.EscapeDataString(" ");
                }

                // trim long messages
                if (maxLen <= 0 || result.Length + escapedChar.Length <= maxLen)
                {
                    result += escapedChar;
                }
                else
                {
                    break;
                }
                i++;
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
                    this.testEnabled = false;
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
                    strCommand += " " + offset + " ";
                    strCommand += this.Encode(message, this.maxMessageLength - strCommand.Length);
                    break;

                case AlphaNumericBarCommands.Command.CUSTOM:                    // CUSTOM <index> <hexval>
                    strCommand += " ";
                    strCommand += this.Encode(message, this.maxMessageLength - strCommand.Length);
                    break;

                case AlphaNumericBarCommands.Command.CSTSET:                    // CSTSET <offset> <index>
                    strCommand += " " + offset + " " + scrollEnd;
                    break;

                case AlphaNumericBarCommands.Command.SCROLL_ON:                 // TODO: must be cheked
                    strCommand = "SCROLL ON " + offset + " " + scrollEnd + " ";
                    strCommand += this.Encode(message, this.maxMessageLength - strCommand.Length);
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

                case AlphaNumericBarCommands.Command.GET:
                    strCommand = "GET";
                    break;
            }

            strCommand += NEW_LINE;

            this.messagesToBeSendQueue.Enqueue(strCommand);
            result = true;

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
                    msgReceive == "TEST OFF OK" ||
                    msgReceive.StartsWith("OK", StringComparison.Ordinal)
                    )
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

            if (message.StartsWith("TEST OFF", StringComparison.Ordinal))
            {
                result = false;
            }

            return result;
        }

        private async Task<bool> ResendIfNotGet(string message)
        {
            var ret = false;
            if (!this.messagesReceivedQueue.IsEmpty)
            {
                var receivedArray = this.messagesReceivedQueue.ToArray();
                var encoded = this.Encode(message, this.maxMessageLength - message.Length);
                var isOk = receivedArray.Any(r => r.Contains(encoded.Substring(0, Math.Min(encoded.Length, 100))));
                if (isOk)
                {
                    this.logger.Debug($"Received GET message OK");
                    ret = true;
                }
                else if (receivedArray.Any(r => r.StartsWith("GET", StringComparison.Ordinal)))
                {
                    this.logger.Debug($"Received GET message Error");
                    if (!this.IsTestLoop)
                    {
                        this.logger.Debug($"Retry sending WRITE");
                        await this.SetAndWriteMessageAsync(message, this.savedOffset, false);
                        // only one retry
                        ret = true;
                    }
                    else if (++this.errorCount > 1)
                    {
                        // testLoop: stop sending messages
                        this.HasGetErrors = true;
                        ret = true;
                    }
                    else
                    {
                        // testLoop: retry
                        this.logger.Debug($"Retry sending WRITE");
                        await this.SetAndWriteMessageAsync(message, this.savedOffset, false);
                    }
                }
                else
                {
                    this.logger.Debug($"waiting GET message");
                }
            }

            return ret;
        }

        #endregion
    }
}

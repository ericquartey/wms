using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Ferretto.VW.Utils;
using NLog;

namespace Ferretto.VW.InverterDriver
{
    /// <summary>
    /// Inverter driver Manager class.
    /// This class manages a socket to communicate with the inverter via TCP/IP protocol.
    /// This class has an internal thread to manage the basic automation for the inverter.
    /// (see System.Net.Sockets.Socket class for the implementation details).
    /// </summary>
    public class InverterDriver : IDriverBase, IDriver, IDisposable
    {
        #region Fields

        public const string IP_ADDR_INVERTER_DEFAULT = "169.254.231.248";
        public const int LONG_TIME_OUT = 250;
        public const int PORT_ADDR_INVERTER_DEFAULT = 17221;

        public const int SHORT_TIME_OUT = 50;
        public const int TIME_OUT = 150;

        private static readonly object lockObj = new object();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly InverterDriverState state;
        private AutoResetEvent ackTerminateEvent;
        private Request currentRequest;

        private bool errorReceivedTelegram; // flag to identify if received telegram is correct
        private bool executeRequestOnRunning;

        private HardwareInverterStatus hwInverterState;

        private Thread mainAutomationThread;

        private AutoResetEvent makeRequestEvent;

        private long perfFrequency;
        private long perfTimeOnReceivingTelegram;
        private long perfTimeOnSendingTelegram;
        private object retParameterValue; // it is the returned parameter value

        private Socket sckClient;

        private Int16 statusWordValue;  // it represents a shared memory where status information lived
        private AutoResetEvent terminateEvent;
        private int timeOut;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default c-tor.
        /// </summary>
        public InverterDriver()
        {
            this.LastError = InverterDriverErrors.NoError;
            this.state = InverterDriverState.Idle;
            this.IPAddressToConnect = IP_ADDR_INVERTER_DEFAULT;
            this.PortAddressToConnect = PORT_ADDR_INVERTER_DEFAULT;
            this.hwInverterState = HardwareInverterStatus.NotOperative;

            logger.Log(LogLevel.Debug, String.Format("InverterDriver in a new incarnation..."));
        }

        #endregion Constructors

        #region Events

        public event ConnectedEventHandler Connected;

        public event EnquiryTelegramDoneEventHandler EnquiryTelegramDone;

        public event ErrorEventHandler Error;

        public event LastRequestDoneEventHandler LastRequestDone;

        public event SelectTelegramDoneEventHandler SelectTelegramDone;

        #endregion Events

        #region Properties

        /// <summary>
        /// <c>True</c> if last request has been done.
        /// </summary>
        public bool GetLastRequestDone { get; private set; }

        /// <summary>
        /// Get the main state of inverter driver.
        /// See the InverterDriverState enum
        /// </summary>
        public InverterDriverState GetMainState => this.state;

        /// <summary>
        /// Set/Get IP address to connect.
        /// Specify the IPv4 address family.
        /// </summary>
        public string IPAddressToConnect { get; set; }

        /// <summary>
        /// Get the last error.
        /// </summary>
        public InverterDriverErrors LastError { get; private set; }

        /// <summary>
        /// Set/Get port address to connect.
        /// Specify the IPv4 address family.
        /// </summary>
        public int PortAddressToConnect { set; get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Release resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Get value of given parameter.
        /// </summary>
        public InverterDriverExitStatus EnquiryTelegram(ParameterID paramID, out object value)
        {
            value = null;
            if (this.errorReceivedTelegram)
            {
                return InverterDriverExitStatus.InvalidOperation;
            }
            if (this.currentRequest == null)
            {
                return InverterDriverExitStatus.InvalidOperation;
            }
            if (paramID != this.currentRequest.ParameterID)
            {
                return InverterDriverExitStatus.InvalidArgument;
            }
            if (this.executeRequestOnRunning)
            {
                return InverterDriverExitStatus.Failure;
            }

            value = this.retParameterValue;
            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Get the drawer weight
        /// </summary>
        /// <returns></returns>
        public InverterDriverExitStatus GetDrawerWeight(float ic)
        {
            logger.Log(LogLevel.Debug, String.Format("> Execute GetDrawerWeight operation."));

            // Add your implementation code here

            return InverterDriverExitStatus.Success;
        }

        public InverterDriverExitStatus GetIOEmergencyState()
        {
            logger.Log(LogLevel.Debug, String.Format("> Execute GetIOEmergencyState operation."));

            // Add your implementation code here

            return InverterDriverExitStatus.Success;
        }

        public InverterDriverExitStatus GetIOState(int index, out bool retValue)
        {
            logger.Log(LogLevel.Debug, String.Format("> Execute GetIOState operation."));

            const int N_BITS_16 = 16;
            const int N_BITS_8 = 8;

            retValue = false;
            if (index < 0)
            {
                return InverterDriverExitStatus.InvalidArgument;
            }
            if (index >= N_BITS_16 - 1)
            {
                return InverterDriverExitStatus.InvalidArgument;
            }

            byte[] ibytes = null;
            lock (lockObj)
            {
                ibytes = BitConverter.GetBytes(this.statusWordValue);
            }
            var t = new BitArray(new byte[] { ibytes[0] });  // convert more than one byte, but for simplicity I'm doing one at a time
            var bits = new bool[N_BITS_16];
            t.CopyTo(bits, 0);
            var t1 = new BitArray(new byte[] { ibytes[1] });
            t1.CopyTo(bits, N_BITS_8);

            retValue = bits[index];
            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Initialize the driver.
        /// </summary>
        public bool Initialize()
        {
            logger.Log(LogLevel.Debug, String.Format("InverterDriver initialization"));

            QueryPerformanceFrequency(out this.perfFrequency);
            this.executeRequestOnRunning = false;

            var bResult = this.connect_to_inverter();
            return bResult;
        }

        /// <summary>
        /// Call back function which will be invoked when the socket detects the incoming data on the stream.
        /// </summary>
        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                QueryPerformanceCounter(out this.perfTimeOnReceivingTelegram);
                var offsetTime_ms = (int)(((double)(this.perfTimeOnReceivingTelegram - this.perfTimeOnSendingTelegram) * 1000) / this.perfFrequency);

                // Socket' stuff
                var theSockId = (SocketPacket)asyn.AsyncState;
                var iRx = theSockId.thisSocket.EndReceive(asyn);
                var nBytes = theSockId.dataBuffer[0];

                // ------
                // The class Telegram performs the parsing of incoming data buffer and extract the information from it
                // ------

                var telegramRead = new byte[iRx];
                Array.Copy(theSockId.dataBuffer, 0, telegramRead, 0, iRx);

                // Parse the received telegram
                this.errorReceivedTelegram = !this.received_telegram(telegramRead, offsetTime_ms, out var paramID, out this.retParameterValue);
                if (!this.errorReceivedTelegram)
                {
                    if (this.currentRequest.Type == TypeOfRequest.SendRequest)
                    {
                        // Notify the <EnquiryTelegram> via Event firing
                        EnquiryTelegramDone?.Invoke(this, new EnquiryTelegramDoneEventArgs(this.currentRequest.ParameterID, this.retParameterValue, this.currentRequest.DataType));
                    }
                    else
                    {
                        // Notify the <SelectTelegram> via Event firing
                        SelectTelegramDone?.Invoke(this, new SelectTelegramDoneEventArgs(this.currentRequest.ParameterID, this.retParameterValue, this.currentRequest.DataType));
                    }

                    // cache value of status word (in cache memory shared)
                    if (this.currentRequest.ParameterID == ParameterID.STATUS_WORD_PARAM)
                    {
                        lock (lockObj)
                        {
                            this.statusWordValue = Convert.ToInt16(this.retParameterValue);
                        }
                    }
                }
                else
                {
                    // if we are here, an error occurs
                    // and so we have to manage it
                }

                lock (lockObj)
                {
                    this.executeRequestOnRunning = false;
                }

                // Prompt to receive a new message
                this.waitForData();
            }
            catch (ObjectDisposedException)
            {
                logger.Log(LogLevel.Debug, String.Format("On Data Received: the Socket has been closed"));
            }
            catch (SocketException)
            {
                logger.Log(LogLevel.Debug, String.Format("On Data Received: Socket critical failure"));
            }
        }

        /// <summary>
        /// Get value of given parameter. It is a echo for SettingRequest.
        /// </summary>
        public InverterDriverExitStatus SelectTelegram(ParameterID paramID, out object value)
        {
            value = null;
            if (this.errorReceivedTelegram)
            {
                return InverterDriverExitStatus.InvalidOperation;
            }
            if (this.currentRequest == null)
            {
                return InverterDriverExitStatus.InvalidOperation;
            }
            if (paramID != this.currentRequest.ParameterID)
            {
                return InverterDriverExitStatus.InvalidArgument;
            }
            if (this.executeRequestOnRunning)
            {
                return InverterDriverExitStatus.Failure;
            }

            value = this.retParameterValue;
            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Send a request to inverter to read parameter value.
        /// </summary>
        /// <returns></returns>
        public InverterDriverExitStatus SendRequest(ParameterID paramID, byte systemIndex, byte dataSetIndex)
        {
            if (this.executeRequestOnRunning)
            {
                return InverterDriverExitStatus.InvalidOperation;
            }

            var valueType = ParameterIDClass.Instance.GetDataValueType(paramID);

            this.currentRequest = new Request(TypeOfRequest.SendRequest, paramID, systemIndex, dataSetIndex, valueType, null);
            this.errorReceivedTelegram = false;
            this.makeRequestEvent.Set();

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Send a request to inverter to read a parameter value.
        /// </summary>
        public InverterDriverExitStatus SettingRequest(ParameterID paramID, byte systemIndex, byte dataSetIndex, object value)
        {
            if (this.executeRequestOnRunning)
            {
                return InverterDriverExitStatus.InvalidOperation;
            }

            this.currentRequest = new Request(TypeOfRequest.SettingRequest, paramID, systemIndex, dataSetIndex, ValueDataType.Int16, value);
            this.errorReceivedTelegram = false;
            this.makeRequestEvent.Set();

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Terminate and release driver' resources.
        /// </summary>
        public void Terminate()
        {
            this.terminateEvent.Set();
            this.destroyThread();
            this.disconnect_from_inverter();

            this.hwInverterState = HardwareInverterStatus.NotOperative;
            logger.Log(LogLevel.Debug, String.Format("Release InverterDriver object."));
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO: Add here methods to tear down unmanaged resources...
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceFrequency(out long lpPerformanceFrequency);

        private bool connect_to_inverter()
        {
            this.LastError = InverterDriverErrors.NoError;
            var bSuccess = true;

            if (this.IPAddressToConnect == "" || this.PortAddressToConnect <= 0)
            {
                logger.Log(LogLevel.Debug, String.Format("Invalid IP address [IP:{0}, port:{1}]", this.IPAddressToConnect, this.PortAddressToConnect));
                this.LastError = InverterDriverErrors.IOError;
                Error?.Invoke(this, new ErrorEventArgs(this.LastError));
                return false;
            }

            try
            {
                var permission = new SocketPermission(
                    NetworkAccess.Connect,
                    TransportType.Tcp,
                    "",
                    SocketPermission.AllPorts
                );

                permission.Demand();

                var ipHost = Dns.GetHostEntry("");
                var ipAddr = IPAddress.Parse(this.IPAddressToConnect);
                var iPortNumber = this.PortAddressToConnect;
                this.sckClient = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                var ipEnd = new IPEndPoint(ipAddr, iPortNumber);
                this.sckClient.Connect(ipEnd);
                if (this.sckClient.Connected)
                {
                    logger.Log(LogLevel.Debug, String.Format("Connection to inverter [IP:{0}] established", this.IPAddressToConnect));
                    Connected?.Invoke(this, new ConnectedEventArgs(true));

                    this.createThread();

                    this.waitForData();
                }
                else
                {
                    logger.Log(LogLevel.Debug, String.Format("Unable to connect to inverter [IP:{0}]", this.IPAddressToConnect));
                    Connected?.Invoke(this, new ConnectedEventArgs(false));
                }
            }
            catch (SocketException exc)
            {
                logger.Log(LogLevel.Debug, String.Format("Connection to inverter failed [error message: {0}]", exc.Message));
                this.LastError = InverterDriverErrors.GenericError;
                Error?.Invoke(this, new ErrorEventArgs(this.LastError));
                bSuccess = false;
            }

            return bSuccess;
        }

        /// <summary>
        /// Create working thread.
        /// </summary>
        private void createThread()
        {
            logger.Log(LogLevel.Debug, String.Format("Create main Working thread."));

            this.terminateEvent = new AutoResetEvent(false);
            this.makeRequestEvent = new AutoResetEvent(false);
            this.ackTerminateEvent = new AutoResetEvent(false);
            this.mainAutomationThread = new Thread(this.mainWorkingThread);
            this.mainAutomationThread.Name = "workingInverterThread";
            this.mainAutomationThread.Start();
        }

        private object decode_ParameterValue<T>(T value, ValueDataType type)
        {
            // ADD your implementation code here
            return value;
        }

        /// <summary>
        /// Release resource of working thread.
        /// </summary>
        private void destroyThread()
        {
            // Wait for the release of main working thread
            var handles = new WaitHandle[1];
            handles[0] = this.ackTerminateEvent;
            WaitHandle.WaitAny(handles, -1);
            this.mainAutomationThread = null;

            this.terminateEvent?.Close();
            this.terminateEvent = null;

            this.makeRequestEvent?.Close();
            this.makeRequestEvent = null;

            this.ackTerminateEvent?.Close();
            this.ackTerminateEvent = null;

            logger.Log(LogLevel.Debug, String.Format("Release main Working thread."));
        }

        private void disconnect_from_inverter()
        {
            this.sckClient?.Close();
            this.sckClient = null;

            Connected?.Invoke(this, new ConnectedEventArgs(false));
        }

        private Int32 encode_ParameterValue(object value, ValueDataType type)
        {
            // ADD your implementation code here
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Get IP address of local machine.
        /// </summary>
        private string getLocalIPAddress()
        {
            var strHostName = Dns.GetHostName();
            var iphostentry = Dns.GetHostEntry(strHostName);

            var IP = "";
            foreach (var ipaddress in iphostentry.AddressList)
            {
                if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    // IPv4 network type
                    IP = ipaddress.ToString();
                    return IP;
                }

                if (ipaddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    // IPv6 network type - NOT implemented
                }
            }

            return IP;
        }

        /// <summary>
        /// Main working thread.
        /// The thread monitors the [Terminate] event and the [MakeRequest] event.
        ///   The [Terminate] event get signalled when the [this] class is disposed (release)
        ///   The [MakeRequest] event get signalled when a new request shall be executed.
        private void mainWorkingThread()
        {
            const int N_EVENTS = 2;
            const int TERMINATE = 0;
            const int MAKEREQUEST = 1;

            var handles = new WaitHandle[N_EVENTS];
            handles[0] = this.terminateEvent;
            handles[1] = this.makeRequestEvent;

            this.timeOut = LONG_TIME_OUT;
            var bExit = false;

            while (!bExit)
            {
                var waitResult = WaitHandle.WaitAny(handles, this.timeOut);
                switch (waitResult)
                {
                    case TERMINATE:
                        {
                            // event for terminate the thread has been signalled (it is fired when Core class is disposed)
                            bExit = true;
                            break;
                        }

                    case MAKEREQUEST:
                        {
                            logger.Log(LogLevel.Debug, String.Format("Execute request => TypeOf:{0}, ParamID:{1}", this.currentRequest.Type.ToString(), this.currentRequest.ParameterID.ToString()));

                            // build the telegram according to data of request to send to inverter
                            this.send_request_to_inverter();

                            //this.timeOut = SHORT_TIME_OUT;
                            break;
                        }

                    case WaitHandle.WaitTimeout:
                        {
                            // Check if ACK from inverter is catched... and it handle the error if no response was not happened
                            if (this.executeRequestOnRunning)
                            {
                                // A request was made, but no response has been collected
                                // Handle it!

                                this.LastError = InverterDriverErrors.IOError;
                                Error?.Invoke(this, new ErrorEventArgs(this.LastError));
                            }
                            else
                            {
                                // Send a request to inverter about the status
                                this.send_request_to_get_status_inverter();
                            }
                            break;
                        }
                }
            }

            this.ackTerminateEvent.Set();
            logger.Log(LogLevel.Debug, String.Format("Exit from main Working thread."));
            return;
        }

        private bool received_telegram(byte[] telegram, int offsetTime_ms, out ParameterID paramID, out object retValue)
        {
            // Parsing and check the information of telegram

            // ----------------------------
            // ----------------------------
            paramID = ParameterID.STATUS_WORD_PARAM;
            retValue = null;

            const byte BIT_MASK_6 = 0x40;
            const byte N_RESERVED_BYTES = 4;

            if (telegram.Length == 0)
            {
                return false;
            }

            // header
            var header = telegram[0];
            var typeOfOperation = (header == 0x00) ? TypeOfRequest.SendRequest : TypeOfRequest.SettingRequest;

            // check error condition on header
            var bError = ((header & BIT_MASK_6) == 0x40);
            if (bError)
            {
                logger.Log(LogLevel.Debug, String.Format(" < Response => Error"));
                return false;
            }

            // Number of bytes
            var nBytes = telegram[1];

            // parameter No
            var parameterNo = new byte[2];
            Array.Copy(telegram, 4, parameterNo, 0, 2);
            parameterNo.Reverse();
            paramID = ParameterIDClass.Instance.ValueToParameterIDCode(BitConverter.ToInt16(parameterNo, 0));

            var valueType = ParameterIDClass.Instance.GetDataValueType(paramID);

            // parameter value
            var nBytesForValue = nBytes - (N_RESERVED_BYTES);
            var parameterValue = new byte[nBytesForValue];
            Array.Copy(telegram, 6, parameterValue, 0, nBytesForValue);
            parameterValue.Reverse();

            var checkNumberOfBytes = false;
            switch (valueType)
            {
                case ValueDataType.Byte: checkNumberOfBytes = nBytesForValue == 1; break;
                case ValueDataType.Float: checkNumberOfBytes = nBytesForValue == 4; break;
                case ValueDataType.Double: checkNumberOfBytes = nBytesForValue == 8; break;
                case ValueDataType.Int16: checkNumberOfBytes = nBytesForValue == 2; break;
                case ValueDataType.Int32: checkNumberOfBytes = nBytesForValue == 4; break;
                case ValueDataType.String: checkNumberOfBytes = true; break;
            }

            if (checkNumberOfBytes)
            {
                switch (valueType)
                {
                    case ValueDataType.Byte:
                        {
                            var value = parameterValue[0];
                            retValue = this.decode_ParameterValue(value, ValueDataType.Byte);
                            break;
                        }
                    case ValueDataType.Float:
                        {
                            var value = BitConverter.ToSingle(parameterValue, 0);
                            retValue = this.decode_ParameterValue(value, ValueDataType.Float);
                            break;
                        }
                    case ValueDataType.Double:
                        {
                            var value = BitConverter.ToDouble(parameterValue, 0);
                            retValue = this.decode_ParameterValue(value, ValueDataType.Double);
                            break;
                        }
                    case ValueDataType.Int16:
                        {
                            var value = BitConverter.ToInt16(parameterValue, 0);
                            retValue = this.decode_ParameterValue(value, ValueDataType.Int16);
                            break;
                        }
                    case ValueDataType.Int32:
                        {
                            var value = BitConverter.ToInt32(parameterValue, 0);
                            retValue = this.decode_ParameterValue(value, ValueDataType.Int32);
                            break;
                        }
                    case ValueDataType.String:
                        {
                            var value = BitConverter.ToString(parameterValue);
                            retValue = this.decode_ParameterValue(value, ValueDataType.String);
                            break;
                        }
                }
            }
            else
            {
                retValue = null;
            }

            logger.Log(LogLevel.Debug, String.Format(" < Response => TypeOf:{0}, ParamID:{1}, return Value:{2}    OffsetTime: {3} ms", typeOfOperation, paramID.ToString(), retValue.ToString(), offsetTime_ms));
            // ----------------------------
            // ----------------------------

            return true;
        }

        private void send_request_to_get_IOEmergency_state()
        {
            // TODO Add your implementation code here
        }

        /// <summary>
        /// Send a request to inverter to get the status.
        /// </summary>
        private void send_request_to_get_status_inverter()
        {
            if (this.executeRequestOnRunning)
            {
                return;
            }

            var valueType = ParameterIDClass.Instance.GetDataValueType(ParameterID.STATUS_WORD_PARAM);

            this.currentRequest = new Request(TypeOfRequest.SendRequest, ParameterID.STATUS_WORD_PARAM, 0x00, 0x06, valueType, null);
            this.errorReceivedTelegram = false;
            this.makeRequestEvent.Set();
        }

        private void send_request_to_inverter()
        {
            // the currentRequest object contains the definition of request need to send to inverter

            // Build the telegram with the methods of Telegram class
            byte[] telegramToSend = null;
            switch (this.currentRequest.Type)
            {
                case TypeOfRequest.SendRequest:
                    {
                        // --------
                        //telegramToSend = Telegram.BuildReadPacket(currentRequest.ParameterID, currentRequest.SysIndex, currentRequest.DsIndex);
                        // --------

                        // ---------------------------------
                        // ---------------------------------
                        var nBytesOfTelegram = 6;
                        telegramToSend = new byte[nBytesOfTelegram];
                        Array.Clear(telegramToSend, 0, nBytesOfTelegram);

                        // header : Bit7 ==> 0: Read
                        telegramToSend[0] = 0x00;
                        telegramToSend[0] |= 0 << 7;

                        // No. bytes
                        telegramToSend[1] = 4;

                        // Sys
                        telegramToSend[2] = this.currentRequest.SystemIndex;

                        // Ds
                        telegramToSend[3] = this.currentRequest.DataSetIndex;

                        // Parameter No.
                        var ans = new byte[2];
                        var parameterNo = new byte[sizeof(short)];
                        parameterNo = BitConverter.GetBytes(Convert.ToInt16(this.currentRequest.ParameterID));

                        parameterNo.CopyTo(ans, 0);

                        Array.Copy(ans, 0, telegramToSend, 4, 2);

                        logger.Log(LogLevel.Debug, String.Format(" > Request => TypeOf:{0}, ParamID:{1}", this.currentRequest.Type.ToString(), this.currentRequest.ParameterID.ToString()));

                        // -----------------------------------
                        // -----------------------------------

                        break;
                    }

                case TypeOfRequest.SettingRequest:
                    {
                        // --------
                        //telegramToSend = Telegram.BuildWritePacket(currentRequest.ParameterID, currentRequest.SysIndex, currentRequest.DsIndex, currentRequest.ParameterValueInt32);
                        // --------

                        // ---------------------------------
                        // ---------------------------------
                        var size = 0;
                        var type = ParameterIDClass.Instance.GetDataValueType(this.currentRequest.ParameterID);
                        switch (type)
                        {
                            case ValueDataType.Byte: size = 1; break;
                            case ValueDataType.Int16: size = 2; break;
                            case ValueDataType.Int32: size = 4; break;
                            case ValueDataType.Float: size = 4; break;
                        }

                        var nBytesOfTelegram = 6 + size;
                        telegramToSend = new byte[nBytesOfTelegram];
                        Array.Clear(telegramToSend, 0, nBytesOfTelegram);

                        // header : Bit7 ==> 1: Write
                        telegramToSend[0] = 0x00;
                        telegramToSend[0] |= 1 << 7;

                        // No. bytes
                        telegramToSend[1] = 6;

                        // Sys
                        telegramToSend[2] = this.currentRequest.SystemIndex;

                        // Ds
                        telegramToSend[3] = this.currentRequest.DataSetIndex;

                        // Parameter No.
                        var ans = new byte[2];
                        var parameterNo = new byte[sizeof(short)];
                        parameterNo = BitConverter.GetBytes(Convert.ToInt16(this.currentRequest.ParameterID));
                        parameterNo.CopyTo(ans, 0);

                        Array.Copy(ans, 0, telegramToSend, 4, 2);

                        // Value parameter No
                        switch (type)
                        {
                            case ValueDataType.Byte:
                                {
                                    var value = Convert.ToByte(this.currentRequest.Value);

                                    ans = new byte[size];
                                    var valueBytes = new byte[size];
                                    valueBytes = BitConverter.GetBytes(value);
                                    valueBytes.CopyTo(ans, 0);
                                    Array.Copy(ans, 0, telegramToSend, 6, size);

                                    logger.Log(LogLevel.Debug, String.Format(" > Request => TypeOf:{0}, ParamID:{1} Value:{2}", this.currentRequest.Type.ToString(), this.currentRequest.ParameterID.ToString(), value));
                                    break;
                                }
                            case ValueDataType.Float:
                                {
                                    float value = this.encode_ParameterValue(this.currentRequest.Value, type);

                                    ans = new byte[size];
                                    var valueBytes = new byte[size];
                                    valueBytes = BitConverter.GetBytes(value);
                                    valueBytes.CopyTo(ans, 0);
                                    Array.Copy(ans, 0, telegramToSend, 6, size);

                                    logger.Log(LogLevel.Debug, String.Format(" > Request => TypeOf:{0}, ParamID:{1} Value:{2}", this.currentRequest.Type.ToString(), this.currentRequest.ParameterID.ToString(), value));
                                    break;
                                }
                            case ValueDataType.Int16:
                                {
                                    var value = Convert.ToInt16(this.encode_ParameterValue(this.currentRequest.Value, type));

                                    ans = new byte[size];
                                    var valueBytes = new byte[size];
                                    valueBytes = BitConverter.GetBytes(value);
                                    valueBytes.CopyTo(ans, 0);
                                    Array.Copy(ans, 0, telegramToSend, 6, size);

                                    logger.Log(LogLevel.Debug, String.Format(" > Request => TypeOf:{0}, ParamID:{1} Value:{2}", this.currentRequest.Type.ToString(), this.currentRequest.ParameterID.ToString(), value));
                                    break;
                                }
                            case ValueDataType.Int32:
                                {
                                    var value = this.encode_ParameterValue(this.currentRequest.Value, type);

                                    ans = new byte[size];
                                    var valueBytes = new byte[size];
                                    valueBytes = BitConverter.GetBytes(value);
                                    valueBytes.CopyTo(ans, 0);
                                    Array.Copy(ans, 0, telegramToSend, 6, size);

                                    logger.Log(LogLevel.Debug, String.Format(" > Request => TypeOf:{0}, ParamID:{1} Value:{2}", this.currentRequest.Type.ToString(), this.currentRequest.ParameterID.ToString(), value));
                                    break;
                                }
                        }

                        // -----------------------------------
                        // -----------------------------------

                        break;
                    }
            }

            // Send the telegram related to the current request to inverter
            this.sendDataToInverter(telegramToSend);

            // Update the flag
            lock (lockObj)
            {
                this.executeRequestOnRunning = true;
            }
        }

        /// <summary>
        /// Send a given telegram data to inverter.
        /// </summary>
        private void sendDataToInverter(byte[] byTelegramToSend)
        {
            try
            {
                if (null != byTelegramToSend)
                {
                    QueryPerformanceCounter(out this.perfTimeOnSendingTelegram);
                    this.sckClient?.Send(byTelegramToSend);
                }
            }
            catch (SocketException exc)
            {
                logger.Log(LogLevel.Debug, String.Format("Send telegram to inverter failed [error Message: {0}]", exc.Message));
                // TODO: Warning? Handle the exception?
            }
        }

        /// <summary>
        /// Start waiting data from the inverter (via socket).
        /// </summary>
        private void waitForData()
        {
            try
            {
                var theSocPkt = new SocketPacket();
                theSocPkt.thisSocket = this.sckClient;
                var result = this.sckClient.BeginReceive(
                    theSocPkt.dataBuffer,
                    0,
                    theSocPkt.dataBuffer.Length,
                    SocketFlags.None,
                    new AsyncCallback(this.OnDataReceived),
                    theSocPkt
                );
            }
            catch (SocketException exc)
            {
                logger.Log(LogLevel.Debug, String.Format("Asyncronously receive message invoke failed [error Message: {0}]", exc.Message));
                // TODO: Warning? Handle the exception?
            }
        }

        #endregion Methods
    }
}

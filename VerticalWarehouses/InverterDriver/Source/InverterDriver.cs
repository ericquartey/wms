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
        public const int PORT_ADDR_INVERTER_DEFAULT = 17221;

        private static readonly object lockObj = new object();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly InverterDriverState state;
        private Request currentRequest;

        private bool errorReceivedTelegram; // flag to identify if received telegram is correct
        private bool executeRequestOnRunning;

        private HardwareInverterStatus hwInverterState;
        
        private AutoResetEvent makeRequestEvent;

        private long perfFrequency;
        private long perfTimeOnReceivingTelegram;
        private long perfTimeOnSendingTelegram;
        private object retParameterValue; // it is the returned parameter value

        private Socket sckClient;

        private Int16 statusWordValue;  // it represents a shared memory where status information lived
        private AutoResetEvent eventToSendPacket;
        private RegisteredWaitHandle regWaitForMainThread;
        private long TimeSendingPacket;

        #region Sensors Digital Signals
        private bool BrakeResistanceOvertemperature;
        private bool EmergencyStop;
        private bool PawlSensorZero;
        private bool UdcPresenceCradleOperator;
        private bool UdcPresenceCradleMachine;

        #endregion Sensors Digital Signals

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

        /// <summary>
        /// Get brake resistance overtemperature-Digital value
        /// </summary>
        public bool Brake_Resistance_Overtemperature
        {
            get
            {
                lock (lockObj)
                {
                    return this.BrakeResistanceOvertemperature;
                }
            }
        }

        /// <summary>
        /// Get Emergency Stop-Digital value
        /// </summary>
        public bool Emergency_Stop
        {
            get
            {
                lock (lockObj)
                {
                    return this.EmergencyStop;
                }
            }
        }

        /// <summary>
        /// Get Pawl Sensor Zero-Digital value
        /// </summary>
        public bool Pawl_Sensor_Zero
        {
            get
            {
                lock (lockObj)
                {
                    return this.PawlSensorZero;
                }
            }
        }

        /// <summary>
        /// Get Udc Presence Cradle Operator-Digital value
        /// </summary>
        public bool Udc_Presence_Cradle_Operator
        {
            get
            {
                lock (lockObj)
                {
                    return this.UdcPresenceCradleOperator;
                }
            }
        }

        /// <summary>
        /// Get Udc Presence Cradle Machine-Digital value
        /// </summary>
        public  bool Udc_Presence_Cradle_Machine
        {
            get
            {
                lock (lockObj)
                {
                    return this.UdcPresenceCradleMachine;
                }
            }
        }

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
            if(bResult == true)
            {
                //Start the main thread
                this.eventToSendPacket?.Set();
            }
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
                this.errorReceivedTelegram = this.received_telegram(telegramRead, out var paramID, out this.retParameterValue);
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

                    switch (this.currentRequest.ParameterID)
                    {
                        case ParameterID.STATUS_WORD_PARAM:
                            {
                                lock (lockObj)
                                {
                                    this.statusWordValue = Convert.ToInt16(this.retParameterValue);
                                }
                                break;
                            }
                        case ParameterID.STATUS_DIGITAL_SIGNALS:
                            {

                                lock (lockObj)
                                {
                                    var retValueShort = Convert.ToInt16(this.retParameterValue);
                                
                                    var arraybytes = BitConverter.GetBytes(retValueShort);
                                    BitArray bit_array = new BitArray(arraybytes);
                                    this.BrakeResistanceOvertemperature = bit_array.Get(2);
                                    this.EmergencyStop = bit_array.Get(4);
                                    this.PawlSensorZero = bit_array.Get(5);
                                    this.UdcPresenceCradleOperator = bit_array.Get(10);
                                    this.UdcPresenceCradleMachine = bit_array.Get(11);
                                }

                                break;
                            }
                        default:
                            {
                                break;
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

                this.eventToSendPacket?.Set();

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
            this.eventToSendPacket = new AutoResetEvent(false);
            this.regWaitForMainThread = ThreadPool.RegisterWaitForSingleObject(this.eventToSendPacket, this.onMainWorkingThread, null, -1, false);
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
            this.regWaitForMainThread?.Unregister(this.eventToSendPacket);
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
        /// The thread monitors the [eventSendToPacket] event.
        /// The [eventSendToPacket] event get signalled when a new request shall be executed.
        private void onMainWorkingThread(object data, bool bTimeOut)
        {
            lock (lockObj)
            {
                long t = 0;
                QueryPerformanceCounter(out t);
                var offsetTime_ms = (int)(((double)(t - this.TimeSendingPacket) * 1000) / this.perfFrequency);
                this.TimeSendingPacket = t;

                // Send a request
                this.currentRequest = new Request(TypeOfRequest.SendRequest, ParameterID.STATUS_DIGITAL_SIGNALS, 0x00, 0x05, ValueDataType.Int16, null);

                // execute the request
                this.send_request_to_inverter();
                logger.Log(LogLevel.Debug, String.Format("Send Read Request. Time elapsed: {0}", offsetTime_ms));

            }
            
        }

        private bool received_telegram(byte[] telegram, out ParameterID paramID, out object retValue)
        {
            // Parsing and check the information of telegram

            var t = new Telegram();
            t.ParseDataBuffer(telegram, telegram.Length, out bool error);

            paramID = t.ParameterIDFromParse;
            retValue = t.RetValueFromParse;

            return error;
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
                        var telegram = new Telegram(); 
                        telegramToSend = telegram.BuildReadPacket(Convert.ToByte(currentRequest.SystemIndex), Convert.ToByte(currentRequest.DataSetIndex), Convert.ToInt16(currentRequest.ParameterID));
                        break;
                        
                    }

                case TypeOfRequest.SettingRequest:
                    {
                        var telegram = new Telegram();
                        telegramToSend = telegram.BuildWritePacket(Convert.ToByte(currentRequest.SystemIndex), Convert.ToByte(currentRequest.DataSetIndex), Convert.ToInt16(currentRequest.ParameterID), currentRequest.DataType, currentRequest.Value);
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

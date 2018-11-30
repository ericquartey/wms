using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public const int HEARTBEAT_TIMEOUT = 300;
        public const int BITS_16 = 16;
        public const int HEARTBIT = 14;

        private static readonly object lockObj = new object();
        private static readonly object lockFlags = new object();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly InverterDriverState state;
        private Request currentRequest;
        private List<Request> RequestList;

        private bool errorReceivedTelegram; // flag to identify if received telegram is correct
        private bool executeRequestOnRunning;
        private bool getStatusWordValue;
        private bool getActualPositionShaftValue;
        private int ActualPositionShaft;
        
        private HardwareInverterStatus hwInverterState;

        private long perfFrequency;
        private long perfTimeOnReceivingTelegram;
        private long perfTimeOnSendingTelegram;
        private object retParameterValue; 

        private Socket sckClient;

        #region Motion Control

        private BitArray StatusWord;           // it represents a shared memory where status information lived
        private BitArray CtrlWord;

        #endregion
        
        private AutoResetEvent eventToSendPacket;
        private RegisteredWaitHandle regWaitForMainThread;
        private long TimeSendingPacket;
        private long TimeSendingHeartBeatPacket;
        
        private bool HeartBeat;
        private Thread thrdHeartBeat;
        private AutoResetEvent Terminate_HeartBeat;

        private Request[] BaseRequestArray;
        private int IndexOfBaseRequest;

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
            this.CtrlWord = new BitArray(BITS_16);
            this.StatusWord = new BitArray(BITS_16);
            
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

        public BitArray Status_Word => this.StatusWord;

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

        public int Actual_Position_Shaft => this.ActualPositionShaft;

        public bool Get_Status_Word_Enable
        {
            set
            {
                lock (lockFlags)
                {
                    this.getStatusWordValue = value;
                }
            }

            get
            {
                lock (lockFlags)
                {
                    return this.getStatusWordValue;
                }
            }
        }

        public bool Get_Actual_Position_Shaft_Enable
        {
            set
            {
                lock (lockFlags)
                {
                    this.getActualPositionShaftValue = value;
                }
            }

            get
            {
                lock (lockFlags)
                {
                    return this.getActualPositionShaftValue;
                }
            }
        }

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
            //logger.Log(LogLevel.Debug, String.Format("> Execute GetIOState operation."));

            //const int N_BITS_16 = 16;
            //const int N_BITS_8 = 8;

            //retValue = false;
            //if (index < 0)
            //{
            //    return InverterDriverExitStatus.InvalidArgument;
            //}
            //if (index >= N_BITS_16 - 1)
            //{
            //    return InverterDriverExitStatus.InvalidArgument;
            //}

            //byte[] ibytes = null;
            //lock (lockObj)
            //{
            //    ibytes = BitConverter.GetBytes(this.statusWordValue);
            //}
            //var t = new BitArray(new byte[] { ibytes[0] });  // convert more than one byte, but for simplicity I'm doing one at a time
            //var bits = new bool[N_BITS_16];
            //t.CopyTo(bits, 0);
            //var t1 = new BitArray(new byte[] { ibytes[1] });
            //t1.CopyTo(bits, N_BITS_8);

            //retValue = bits[index];

            retValue = true;
            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Initialize the driver.
        /// </summary>
        public bool Initialize()
        {
            logger.Log(LogLevel.Debug, String.Format("InverterDriver initializing..."));

            QueryPerformanceFrequency(out this.perfFrequency);
            this.executeRequestOnRunning = false;

            // Create the base requests array (for internal requests)
            this.BaseRequestArray = new Request[3];
            this.BaseRequestArray[0] = new Request(TypeOfRequest.SendRequest, ParameterID.STATUS_DIGITAL_SIGNALS, RequestSource.Internal, 0x00, 0x05, ValueDataType.Int16, null);
            this.BaseRequestArray[1] = new Request(TypeOfRequest.SendRequest, ParameterID.STATUS_WORD_PARAM, RequestSource.Internal, 0x00, 0x05, ValueDataType.Int16, null);
            this.BaseRequestArray[2] = new Request(TypeOfRequest.SendRequest, ParameterID.ACTUAL_POSITION_SHAFT, RequestSource.Internal, 0x00, 0x05, ValueDataType.Int16, null);

            this.getStatusWordValue = false;
            this.getActualPositionShaftValue = false;
            this.IndexOfBaseRequest = -1;

            // Create the requests list (for external requests)
            this.RequestList = new List<Request>();

            // Connect to inverter
            var bResult = this.connect_to_inverter();
            if(bResult)
            {
                // Start the main thread
                this.eventToSendPacket?.Set();
            }

            return bResult;
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
            var valueType = ParameterIDClass.Instance.GetDataValueType(paramID);

            //Store the request into the list.
            var Rq = new Request(TypeOfRequest.SendRequest, paramID, RequestSource.External, systemIndex, dataSetIndex, valueType, null);
            lock (lockObj)
            {
                this.RequestList.Add(Rq);
            }

            this.errorReceivedTelegram = false;
    
            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Send a request to inverter to read a parameter value.
        /// </summary>
        public InverterDriverExitStatus SettingRequest(ParameterID paramID, byte systemIndex, byte dataSetIndex, object value)
        {   
            //Store the request into the list.
            var Rq = new Request(TypeOfRequest.SettingRequest, paramID, RequestSource.External, systemIndex, dataSetIndex, ValueDataType.Int16, value);
            lock (lockObj)
            {
                this.RequestList.Add(Rq);
            }

            this.errorReceivedTelegram = false;

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Terminate and release driver' resources.
        /// </summary>
        public void Terminate()
        {
            this.Terminate_HeartBeat.Set();  //Terminate the heartbeat thread
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

        #region Win32

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceFrequency(out long lpPerformanceFrequency);

        #endregion

        /// <summary>
        /// Connect to inverter device.
        /// </summary>
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

                    this.createThreads();
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
        /// Create main working thread and heartbeat thread.
        /// </summary>
        private void createThreads()
        {
            logger.Log(LogLevel.Debug, String.Format("Create main Working thread."));
            this.eventToSendPacket = new AutoResetEvent(false);
            this.regWaitForMainThread = ThreadPool.RegisterWaitForSingleObject(this.eventToSendPacket, this.onMainWorkingThread, null, -1, false);

            this.Terminate_HeartBeat = new AutoResetEvent(false);
            this.thrdHeartBeat = new Thread(this.HeartBeat_Thread);
            this.thrdHeartBeat.Name = "HeartBeat_Thread";
            this.thrdHeartBeat.Priority = ThreadPriority.Highest;
            this.thrdHeartBeat.Start();
        }

        /// <summary>
        /// Release resource of working thread.
        /// </summary>
        private void destroyThread()
        {
            this.regWaitForMainThread?.Unregister(this.eventToSendPacket);
            logger.Log(LogLevel.Debug, String.Format("Release main Working thread."));
        }

        /// <summary>
        /// Disconnect from inverter device.
        /// </summary>
        private void disconnect_from_inverter()
        {
            this.sckClient?.Close();
            this.sckClient = null;

            Connected?.Invoke(this, new ConnectedEventArgs(false));          
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
        /// Convert BitArray structure to Bytes array.
        /// </summary>
        internal static byte[] BitArrayToByteArray(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        /// <summary>
        /// Main working thread.
        /// The thread monitors the [eventSendToPacket] event.
        /// The [eventSendToPacket] event get signalled when a new request shall be executed.
        private void onMainWorkingThread(object data, bool bTimeOut)
        {
            long t = 0;
            QueryPerformanceCounter(out t);
            var offsetTime_ms = (int)(((double)(t - this.TimeSendingPacket) * 1000) / this.perfFrequency);
            this.TimeSendingPacket = t;

            var isHeartBeat = false;
            lock(lockObj)
            {
                isHeartBeat = this.HeartBeat;
            }

            // Send a request        
            if (isHeartBeat)
            {
                var offsetTime_HeartBeat = (int)(((double)(t - this.TimeSendingHeartBeatPacket) * 1000) / this.perfFrequency);
                this.TimeSendingHeartBeatPacket = t;

                var bytes = BitArrayToByteArray(this.CtrlWord);
                var value = BitConverter.ToInt16(bytes, 0);
                this.currentRequest = new Request(TypeOfRequest.SettingRequest, ParameterID.CONTROL_WORD_PARAM, RequestSource.Internal, 0x00, 0x05, ValueDataType.Int16, value);
                this.CtrlWord.Set(HEARTBIT, !this.CtrlWord.Get(HEARTBIT));
                //logger.Log(LogLevel.Debug, String.Format("Send HeartBeat. Time elapsed: {0}", offsetTime_HeartBeat));
            }
            else
            {
                if (this.RequestList.Count > 0)
                {
                    // Select the first item in the list.
                    this.currentRequest = this.RequestList[0];
                    // Remove the first item in the list.
                    lock (lockObj)
                    {
                        this.RequestList.RemoveAt(0);
                    }

                    //logger.Log(LogLevel.Debug, String.Format("Send External Request Size of List: {0} Time elapsed: {1}", this.RequestList.Count, offsetTime_ms));
                }
                else
                {
                    // Select the next index of request for base request array
                    this.IndexOfBaseRequest = getNextIndexOfRequest(this.IndexOfBaseRequest + 1);
                    this.currentRequest = this.BaseRequestArray[this.IndexOfBaseRequest];

                    //logger.Log(LogLevel.Debug, String.Format("Send Base Request Indext of Request: {0} Time elapsed: {1}", this.IndexOfBaseRequest, offsetTime_ms));
                }

            }

            // execute the request
            this.send_request_to_inverter();
        }

        /// <summary>
        /// Retrieve the index of next request belonging to the base requests array. 
        /// </summary>
        private int getNextIndexOfRequest(int nextIndexCandidate)
        {
            int k = 0;
            bool exit = false;
            int nextIndex = 0;

            while (k < this.BaseRequestArray.Length && !exit)
            {
                nextIndex = (nextIndexCandidate + k) % this.BaseRequestArray.Length;
                switch (nextIndex)
                {
                  case 0:
                        {
                            exit = true;
                            break;
                        }
                  case 1:
                        {
                            lock(lockFlags)
                            { 
                              exit = this.getStatusWordValue;
                            }

                            break;
                        
                        }

                  case 2:
                        {
                            lock (lockFlags)
                            {
                                exit = this.getActualPositionShaftValue;
                            }

                            break;
                        }

                  default:
                        break;
                }

                k++;
            }

            return nextIndex;
        }

        /// <summary>
        /// HeartBeat Thread. This thread prompts the driver
        /// to send a defined packet to inverter for heartbeat routine.
        /// </summary>
        private void HeartBeat_Thread()
        {
            const int TERMINATE = 0;
            var exit = false;
            var handles = new WaitHandle[1];
            handles[0] = this.Terminate_HeartBeat;
            while (!exit)
            {

                var WaitResult = WaitHandle.WaitAny(handles, HEARTBEAT_TIMEOUT);
                switch (WaitResult)
                {
                    case TERMINATE:
                        {
                            exit = true;
                            break;
                        }

                    case WaitHandle.WaitTimeout:
                        {
                            lock (lockObj)
                            {
                                this.HeartBeat = true;
                            }
                            break;
                        }

                    default:
                        break;
                }

            }
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

                // Socket' stuf
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
                    if (this.currentRequest.Type == TypeOfRequest.SendRequest && this.currentRequest.Source == RequestSource.External)
                    {
                        // Notify the <EnquiryTelegram> via Event firing
                        EnquiryTelegramDone?.Invoke(this, new EnquiryTelegramDoneEventArgs(this.currentRequest.ParameterID, this.retParameterValue, this.currentRequest.DataType));
                    }

                    if (this.currentRequest.Type == TypeOfRequest.SettingRequest && this.currentRequest.Source == RequestSource.External)
                    {
                        // Notify the <SelectTelegram> via Event firing
                        SelectTelegramDone?.Invoke(this, new SelectTelegramDoneEventArgs(this.currentRequest.ParameterID, this.retParameterValue, this.currentRequest.DataType));
                    }

                    // Update internal class members
                    switch (this.currentRequest.ParameterID)
                    {
                        case ParameterID.STATUS_WORD_PARAM:
                            {
                                lock (lockObj)
                                {
                                    var retValueShort = Convert.ToInt16(this.retParameterValue);
                                    var arraybytes = BitConverter.GetBytes(retValueShort);
                                    this.StatusWord = new BitArray(arraybytes);
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

                        case ParameterID.CONTROL_WORD_PARAM:
                            {
                                if (this.currentRequest.Source == RequestSource.Internal)
                                {
                                    lock (lockObj)
                                    {
                                        this.HeartBeat = false;
                                    }
                                }
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
        /// Parse the receiving telegram from inverter device.
        /// </summary>
        private bool received_telegram(byte[] telegram, out ParameterID paramID, out object retValue)
        {
            // Parsing and check the information of telegram

            var t = new Telegram();
            t.ParseDataBuffer(telegram, telegram.Length, out bool error);

            paramID = t.ParameterIDFromParse;
            retValue = t.RetValueFromParse;

            return error;
        }

        /// <summary>
        /// Send a chosen request to inverter.
        /// </summary>
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

        ///<summary>
        /// Start waiting data from the inverter(via socket).
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

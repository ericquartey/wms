﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using NLog;

namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Inverter driver Manager class.
    /// This class manages a socket to communicate with the inverter via TCP/IP protocol.
    /// This class has an internal thread to manage the basic automation for the inverter.
    /// (see System.Net.Sockets.Socket class for the implementation details).
    /// </summary>
    public class InverterDriver : IDriver, IDisposable, IInverterDriver
    {
        #region Fields

        public const int BITS_16 = 16;

        public const int HEARTBEAT_TIMEOUT = 300;

        public const int HEARTBIT = 14;

        public const string IP_ADDR_INVERTER_DEFAULT = "169.254.231.248";

        public const int PORT_ADDR_INVERTER_DEFAULT = 17221;

        private static readonly object lockFlags = new object();

        private static readonly object lockObj = new object();

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly BitArray CtrlWord;

        private readonly InverterDriverState state;

        private int ActualPositionHorizontalShaft;

        private int ActualPositionVerticalShaft;

        private Request[] BaseRequestArray;

        private bool BrakeResistanceOvertemperature;

        private Request currentRequest;

        private bool EmergencyStop;

        private bool enableUpdateCurrentPositionHorizontalShaftMode;

        private bool enableUpdateCurrentPositionVerticalShaftMode;

        private bool errorReceivedTelegram;

        private AutoResetEvent eventToSendPacket;

        private bool getStatusWordValue;

        private bool HeartBeat;

        private int IndexOfBaseRequest;

        private bool PawlSensorZero;

        private long perfFrequency;

        private long perfHeartBeatTime;

        private long perfTimeGetActualPosition;

        private long perfTimeOnReceivingTelegram;

        private long perfTimeOnSendingTelegram;

        private RegisteredWaitHandle regWaitForMainThread;

        private List<Request> RequestList;

        private object retParameterValue;

        private Socket sckClient;

        private BitArray StatusWord;

        private AutoResetEvent Terminate_HeartBeat;

        private Thread thrdHeartBeat;

        private long TimeSendingHeartBeatPacket;

        private long TimeSendingPacket;

        private bool UdcPresenceCradleMachine;

        private bool UdcPresenceCradleOperator;

        #endregion

        #region Constructors

        public InverterDriver()
        {
            this.LastError = InverterDriverErrors.NoError;
            this.state = InverterDriverState.Idle;
            this.IPAddressToConnect = IP_ADDR_INVERTER_DEFAULT;
            this.PortAddressToConnect = PORT_ADDR_INVERTER_DEFAULT;
            this.CtrlWord = new BitArray(BITS_16);
            this.StatusWord = new BitArray(BITS_16);
        }

        #endregion

        #region Events

        public event EventHandler<ConnectedEventArgs> Connected;

        public event EventHandler<EnquiryTelegramDoneEventArgs> EnquiryTelegramDone_CalibrateVerticalAxis;

        public event EventHandler<EnquiryTelegramDoneEventArgs> EnquiryTelegramDone_PositioningDrawer;

        public event EventHandler<ErrorEventArgs> Error;

        public event EventHandler<SelectTelegramDoneEventArgs> SelectTelegramDone_CalibrateVerticalAxis;

        public event EventHandler<SelectTelegramDoneEventArgs> SelectTelegramDone_PositioningDrawer;

        #endregion

        #region Properties

        /// <summary>
        /// Get brake resistance overtemperature-Digital value.
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
        /// Get the current position of controlled Shaft (horizontal axis).
        /// </summary>
        public int Current_Position_Horizontal_Shaft
        {
            get
            {
                lock (lockObj)
                {
                    return this.ActualPositionHorizontalShaft;
                }
            }
        }

        /// <summary>
        /// Get the current position of controlled Shaft (vertical axis).
        /// </summary>
        public int Current_Position_Vertical_Shaft
        {
            get
            {
                lock (lockObj)
                {
                    return this.ActualPositionVerticalShaft;
                }
            }
        }

        public ActionType CurrentActionType { get; set; }

        /// <summary>
        /// Get Emergency Stop-Digital value.
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
        /// Enable the automatic updating of current position shaft parameter (vertical axis).
        /// </summary>
        public bool Enable_Update_Current_Position_Horizontal_Shaft_Mode
        {
            get
            {
                lock (lockFlags)
                {
                    return this.enableUpdateCurrentPositionHorizontalShaftMode;
                }
            }
            set
            {
                lock (lockFlags)
                {
                    this.enableUpdateCurrentPositionHorizontalShaftMode = value;
                }
            }
        }

        /// <summary>
        /// Enable the automatic updating of current position shaft parameter (vertical axis).
        /// </summary>
        public bool Enable_Update_Current_Position_Vertical_Shaft_Mode
        {
            get
            {
                lock (lockFlags)
                {
                    return this.enableUpdateCurrentPositionVerticalShaftMode;
                }
            }
            set
            {
                lock (lockFlags)
                {
                    this.enableUpdateCurrentPositionVerticalShaftMode = value;
                    // logger.Log(LogLevel.Debug, "Set enableUpdateCurrentPositionVerticalShaftMode = {0}", value.ToString());
                }
            }
        }

        /// <summary>
        /// Enable the retrieve of StatusWord parameter value.
        /// </summary>
        public bool Get_Status_Word_Enable
        {
            get
            {
                lock (lockFlags)
                {
                    return this.getStatusWordValue;
                }
            }
            set
            {
                lock (lockFlags)
                {
                    this.getStatusWordValue = value;
                }
            }
        }

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
        /// Get Pawl Sensor Zero-Digital value.
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
        /// Set/Get port address to connect.
        /// Specify the IPv4 address family.
        /// </summary>
        public int PortAddressToConnect { set; get; }

        /// <summary>
        /// Get the StatusWord parameter value.
        /// </summary>
        public BitArray Status_Word => this.StatusWord;

        /// <summary>
        /// Get Udc Presence Cradle Machine-Digital value.
        /// </summary>
        public bool Udc_Presence_Cradle_Machine
        {
            get
            {
                lock (lockObj)
                {
                    return this.UdcPresenceCradleMachine;
                }
            }
        }

        /// <summary>
        /// Get Udc Presence Cradle Operator-Digital value.
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

        #endregion

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
        /// Get the drawer weight.
        /// </summary>
        /// <returns></returns>
        public InverterDriverExitStatus GetDrawerWeight(float ic)
        {
            // logger.Log(LogLevel.Debug, String.Format("> Execute GetDrawerWeight operation."));

            // Add your implementation code here

            return InverterDriverExitStatus.Success;
        }

        public InverterDriverExitStatus GetIOEmergencyState()
        {
            // logger.Log(LogLevel.Debug, String.Format("> Execute GetIOEmergencyState operation."));

            // Add your implementation code here

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Initialize the driver.
        /// </summary>
        public bool Initialize()
        {
            // logger.Log(LogLevel.Debug, String.Format("InverterDriver initializing..."));

            QueryPerformanceFrequency(out this.perfFrequency);

            // Create the base requests array (for internal requests)
            this.BaseRequestArray = new Request[3];
            this.BaseRequestArray[0] = new Request(TypeOfRequest.SendRequest, ParameterId.STATUS_DIGITAL_SIGNALS, RequestSource.Internal, 0x00, 0x05, ValueDataType.UInt16, null);
            this.BaseRequestArray[1] = new Request(TypeOfRequest.SendRequest, ParameterId.STATUS_WORD_PARAM, RequestSource.Internal, 0x00, 0x05, ValueDataType.UInt16, null);
            this.BaseRequestArray[2] = new Request(TypeOfRequest.SendRequest, ParameterId.ACTUAL_POSITION_SHAFT, RequestSource.Internal, 0x00, 0x05, ValueDataType.Int32, null);

            this.getStatusWordValue = false;
            this.enableUpdateCurrentPositionVerticalShaftMode = false;
            this.enableUpdateCurrentPositionHorizontalShaftMode = false;
            this.IndexOfBaseRequest = -1;

            // Create the requests list (for external requests)
            this.RequestList = new List<Request>();

            // Connect to inverter
            var bResult = this.ConnectToInverter();
            if (bResult)
            {
                // Start the main thread
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

                // Socket' stuf
                var theSockId = (SocketPacket)asyn.AsyncState;
                var iRx = theSockId.thisSocket.EndReceive(asyn);
                var nBytes = theSockId.dataBuffer[0];

                var telegramRead = new byte[iRx];
                Array.Copy(theSockId.dataBuffer, 0, telegramRead, 0, iRx);

                // Parse the received telegram
                this.errorReceivedTelegram = ParseTelegram(telegramRead, out var paramID, out this.retParameterValue);
                if (!this.errorReceivedTelegram)
                {
                    // Update internal class members
                    switch (this.currentRequest.ParameterId)
                    {
                        case ParameterId.STATUS_WORD_PARAM:
                            {
                                lock (lockObj)
                                {
                                    var retValueShort = Convert.ToUInt16(this.retParameterValue);
                                    var arraybytes = BitConverter.GetBytes(retValueShort);
                                    this.StatusWord = new BitArray(arraybytes);
                                }

                                break;
                            }

                        case ParameterId.STATUS_DIGITAL_SIGNALS:
                            {
                                lock (lockObj)
                                {
                                    var retValueShort = Convert.ToUInt16(this.retParameterValue);

                                    var arraybytes = BitConverter.GetBytes(retValueShort);
                                    var bit_array = new BitArray(arraybytes);
                                    this.BrakeResistanceOvertemperature = bit_array.Get(2);
                                    this.EmergencyStop = bit_array.Get(4);
                                    this.PawlSensorZero = bit_array.Get(5);
                                    this.UdcPresenceCradleOperator = bit_array.Get(10);
                                    this.UdcPresenceCradleMachine = bit_array.Get(11);
                                }

                                break;
                            }

                        case ParameterId.ACTUAL_POSITION_SHAFT:
                            {
                                var dTime_ms = (int)(((double)(this.perfTimeOnReceivingTelegram - this.perfTimeGetActualPosition) * 1000) / this.perfFrequency);
                                this.perfTimeGetActualPosition = this.perfTimeOnReceivingTelegram;
                                //logger.Log(LogLevel.Debug, String.Format(" --> Get actual position: dTime = {0} ms", dTime_ms));

                                if (this.CurrentActionType == ActionType.PositioningDrawer)
                                {
                                    lock (lockObj)
                                    {
                                        this.ActualPositionVerticalShaft = Convert.ToInt32(this.retParameterValue);
                                    }
                                }
                                else
                                {
                                    lock (lockObj)
                                    {
                                        this.ActualPositionHorizontalShaft = Convert.ToInt32(this.retParameterValue);
                                    }
                                }

                                break;
                            }

                        case ParameterId.CONTROL_WORD_PARAM:
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

                    // Notify
                    switch (this.CurrentActionType)
                    {
                        case ActionType.CalibrateVerticalAxis:
                        case ActionType.CalibrateHorizontalAxis:
                            {
                                if (this.currentRequest.Type == TypeOfRequest.SendRequest && this.currentRequest.Source == RequestSource.External) { this.EnquiryTelegramDone_CalibrateVerticalAxis?.Invoke(this, new EnquiryTelegramDoneEventArgs(this.currentRequest.ParameterId, this.retParameterValue, this.currentRequest.DataType)); }
                                if (this.currentRequest.Type == TypeOfRequest.SettingRequest && this.currentRequest.Source == RequestSource.External)
                                {
                                    //logger.Log(LogLevel.Debug, "Invoke SelectTelegramDone for parameter: {0}, value: {1}", this.currentRequest.ParameterId, (ushort)this.retParameterValue);
                                    this.SelectTelegramDone_CalibrateVerticalAxis?.Invoke(this, new SelectTelegramDoneEventArgs(this.currentRequest.ParameterId, this.retParameterValue, this.currentRequest.DataType));
                                }

                                break;
                            }
                        case ActionType.PositioningDrawer:
                        case ActionType.HorizontalMoving:
                            {
                                if (this.currentRequest.Type == TypeOfRequest.SendRequest && this.currentRequest.Source == RequestSource.External) { this.EnquiryTelegramDone_PositioningDrawer?.Invoke(this, new EnquiryTelegramDoneEventArgs(this.currentRequest.ParameterId, this.retParameterValue, this.currentRequest.DataType)); }
                                if (this.currentRequest.Type == TypeOfRequest.SettingRequest && this.currentRequest.Source == RequestSource.External) { this.SelectTelegramDone_PositioningDrawer?.Invoke(this, new SelectTelegramDoneEventArgs(this.currentRequest.ParameterId, this.retParameterValue, this.currentRequest.DataType)); }
                                break;
                            }
                        default:
                            break;
                    }
                }
                else
                {
                    // if we are here, an error occurs
                    // and so we have to manage it
                }

                this.eventToSendPacket?.Set();

                // Prompt to receive a new message
                this.WaitForData();
            }
            catch (ObjectDisposedException)
            {
                // logger.Log(LogLevel.Debug, String.Format("On Data Received: the Socket has been closed"));
            }
            catch (SocketException)
            {
                // logger.Log(LogLevel.Debug, String.Format("On Data Received: Socket critical failure"));
            }
        }

        /// <summary>
        /// Send a request to inverter to read parameter value.
        /// </summary>
        /// <returns></returns>
        public InverterDriverExitStatus SendRequest(ParameterId paramID, byte systemIndex, byte dataSetIndex)
        {
            var valueType = ParameterIdClass.GetDataValueType(paramID);

            //Store the request into the list.
            var request = new Request(TypeOfRequest.SendRequest, paramID, RequestSource.External, systemIndex, dataSetIndex, valueType, null);
            lock (lockObj)
            {
                this.RequestList.Add(request);
            }

            this.errorReceivedTelegram = false;

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Send a request to inverter to set a value for a given parameter.
        /// </summary>
        public InverterDriverExitStatus SettingRequest(
            ParameterId parameterId,
            byte systemIndex,
            byte dataSetIndex,
            object value)
        {
            var valueType = ParameterIdClass.GetDataValueType(parameterId);

            var request = new Request(
                TypeOfRequest.SettingRequest,
                parameterId,
                RequestSource.External,
                systemIndex,
                dataSetIndex,
                valueType,
                value);

            BitArray bitArrayCtrlTmp = null;
            if (parameterId == ParameterId.CONTROL_WORD_PARAM)
            {
                var retValueShort = Convert.ToUInt16(value);
                var arraybytes = BitConverter.GetBytes(retValueShort);
                bitArrayCtrlTmp = new BitArray(arraybytes);
            }

            lock (lockObj)
            {
                // Store the request into the list.
                this.RequestList.Add(request);

                // Update the ControlWord parameter value, if required
                if (bitArrayCtrlTmp != null)
                {
                    for (var j = 0; j < BITS_16; j++)
                    {
                        if (j != HEARTBIT)
                        {
                            this.CtrlWord.Set(j, bitArrayCtrlTmp.Get(j));
                        }
                    }
                }
            }

            this.errorReceivedTelegram = false;

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Terminate and release driver' resources.
        /// </summary>
        public void Terminate()
        {
            this.Terminate_HeartBeat?.Set();  // Terminate the heartbeat thread
            this.DestroyThread();
            this.DisconnectFromInverter();

            // logger.Log(LogLevel.Debug, String.Format("Release InverterDriver object."));
        }

        /// <summary>
        /// Convert BitArray structure to Bytes array.
        /// Internal method.
        /// </summary>
        internal static byte[] BitArrayToByteArray(BitArray bits)
        {
            var ret = new byte[((bits.Length - 1) / 8) + 1];
            bits.CopyTo(ret, 0);
            return ret;
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

        /// <summary>
        /// Parse the receiving telegram from inverter device.
        /// </summary>
        private static bool ParseTelegram(byte[] telegramBytes, out ParameterId paramID, out object retValue)
        {
            // Parsing and check the information of telegram
            var telegram = new Telegram();
            telegram.ParseDataBuffer(telegramBytes, telegramBytes.Length, out var error);

            paramID = telegram.ParameterIdFromParse;
            retValue = telegram.RetValueFromParse;

            return error;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceFrequency(out long lpPerformanceFrequency);

        /// <summary>
        /// Connect to inverter device.
        /// </summary>
        private bool ConnectToInverter()
        {
            this.LastError = InverterDriverErrors.NoError;
            var bSuccess = true;

            if (this.IPAddressToConnect == string.Empty || this.PortAddressToConnect <= 0)
            {
                // logger.Log(LogLevel.Debug, String.Format("Invalid IP address [IP:{0}, port:{1}]", this.IPAddressToConnect, this.PortAddressToConnect));
                this.LastError = InverterDriverErrors.IOError;
                this.Error?.Invoke(this, new ErrorEventArgs(this.LastError));
                return false;
            }

            try
            {
                var ipHost = Dns.GetHostEntry(string.Empty);
                var ipAddr = IPAddress.Parse(this.IPAddressToConnect);
                var iPortNumber = this.PortAddressToConnect;
                this.sckClient = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                var ipEnd = new IPEndPoint(ipAddr, iPortNumber);
                this.sckClient.Connect(ipEnd);
                if (this.sckClient.Connected)
                {
                    // logger.Log(LogLevel.Debug, String.Format("Connection to inverter [IP:{0}] established", this.IPAddressToConnect));
                    this.Connected?.Invoke(this, new ConnectedEventArgs(true));

                    this.CreateThreads();
                    this.WaitForData();
                }
                else
                {
                    // logger.Log(LogLevel.Debug, String.Format("Unable to connect to inverter [IP:{0}]", this.IPAddressToConnect));
                    this.Connected?.Invoke(this, new ConnectedEventArgs(false));
                }
            }
            catch (SocketException)
            {
                // logger.Log(LogLevel.Debug, String.Format("Connection to inverter failed [error message: {0}]", exc.Message));
                this.LastError = InverterDriverErrors.GenericError;
                this.Error?.Invoke(this, new ErrorEventArgs(this.LastError));
                bSuccess = false;
            }

            return bSuccess;
        }

        /// <summary>
        /// Create main working thread and heartbeat thread.
        /// </summary>
        private void CreateThreads()
        {
            // logger.Log(LogLevel.Debug, String.Format("Create main Working thread."));
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
        private void DestroyThread()
        {
            this.regWaitForMainThread?.Unregister(this.eventToSendPacket);
            // logger.Log(LogLevel.Debug, String.Format("Release main Working thread."));
        }

        /// <summary>
        /// Disconnect from inverter device.
        /// </summary>
        private void DisconnectFromInverter()
        {
            this.sckClient?.Close();
            this.sckClient = null;

            this.Connected?.Invoke(this, new ConnectedEventArgs(false));
        }

        /// <summary>
        /// Get IP address of local machine.
        /// </summary>
        private string getLocalIPAddress()
        {
            var strHostName = Dns.GetHostName();
            var iphostentry = Dns.GetHostEntry(strHostName);

            var IP = string.Empty;
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
        /// Retrieve the index of next request belonging to the base requests array.
        /// </summary>
        private int getNextIndexOfRequest(int nextIndexCandidate)
        {
            var k = 0;
            var exit = false;
            var nextIndex = 0;

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
                            lock (lockFlags)
                            {
                                exit = this.getStatusWordValue;
                            }

                            break;
                        }

                    case 2:
                        {
                            lock (lockFlags)
                            {
                                exit = this.enableUpdateCurrentPositionVerticalShaftMode;
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
                            QueryPerformanceCounter(out var t);
                            var offsetTime_ms = (int)(((double)(t - this.perfHeartBeatTime) * 1000) / this.perfFrequency);
                            this.perfHeartBeatTime = t;

                            //logger.Log(LogLevel.Debug, String.Format("Heart Beat dwTime = {0} ms", offsetTime_ms));

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
        /// Main working thread.
        /// The thread monitors the [eventSendToPacket] event.
        /// The [eventSendToPacket] event get signalled when a new request shall be executed.
        private void onMainWorkingThread(object data, bool bTimeOut)
        {
            QueryPerformanceCounter(out var t);
            var offsetTime_ms = (int)(((double)(t - this.TimeSendingPacket) * 1000) / this.perfFrequency);
            this.TimeSendingPacket = t;

            var isHeartBeat = false;
            //lock (lockObj)
            //{
            //    isHeartBeat = this.HeartBeat;
            //}

            // Send a request
            if (isHeartBeat)
            {
                var offsetTime_HeartBeat = (int)(((double)(t - this.TimeSendingHeartBeatPacket) * 1000) / this.perfFrequency);

                // Just wait in order to ensure HEARTBEAT_TIMEOUT time elapsed from the previous HeartBeat operation (inverter heart beat feature)
                if (offsetTime_HeartBeat < HEARTBEAT_TIMEOUT)
                {
                    Thread.Sleep(HEARTBEAT_TIMEOUT - offsetTime_HeartBeat);
                    QueryPerformanceCounter(out t);
                    offsetTime_HeartBeat = (int)(((double)(t - this.TimeSendingHeartBeatPacket) * 1000) / this.perfFrequency);
                }
                this.TimeSendingHeartBeatPacket = t;

                var ctrlWTmp = new BitArray(BITS_16);
                lock (lockObj)
                {
                    ctrlWTmp = this.CtrlWord;
                }

                var bytes = BitArrayToByteArray(ctrlWTmp);
                var value = BitConverter.ToUInt16(bytes, 0);
                this.currentRequest = new Request(TypeOfRequest.SettingRequest, ParameterId.CONTROL_WORD_PARAM, RequestSource.Internal, 0x00, 0x05, ValueDataType.UInt16, value);

                lock (lockObj)
                {
                    this.CtrlWord.Set(HEARTBIT, !ctrlWTmp.Get(HEARTBIT));
                }

                // logger.Log(LogLevel.Debug, String.Format("Send HeartBeat. Time elapsed: {0}", offsetTime_HeartBeat));
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
                    this.IndexOfBaseRequest = this.getNextIndexOfRequest(this.IndexOfBaseRequest + 1);
                    this.currentRequest = this.BaseRequestArray[this.IndexOfBaseRequest];

                    //logger.Log(LogLevel.Debug, String.Format("Send Base Request Indext of Request: {0} Time elapsed: {1}", this.IndexOfBaseRequest, offsetTime_ms));
                }
            }

            // execute the request
            this.SendRequestToInverter();
        }

        /// <summary>
        /// Send a given telegram data to inverter.
        /// </summary>
        private void SendDataToInverter(byte[] byTelegramToSend)
        {
            try
            {
                if (byTelegramToSend != null)
                {
                    QueryPerformanceCounter(out this.perfTimeOnSendingTelegram);
                    this.sckClient?.Send(byTelegramToSend);
                }
            }
            catch (SocketException)
            {
                // logger.Log(LogLevel.Debug, String.Format("Send telegram to inverter failed [error Message: {0}]", exc.Message));
                // TODO: Warning? Handle the exception?
            }
        }

        /// <summary>
        /// Send a chosen request to inverter.
        /// </summary>
        private void SendRequestToInverter()
        {
            // the currentRequest object contains the definition of request need to send to inverter

            // Build the telegram with the methods of Telegram class
            byte[] telegramToSend = null;
            switch (this.currentRequest.Type)
            {
                case TypeOfRequest.SendRequest:
                    {
                        var telegram = new Telegram();
                        telegramToSend = telegram.BuildReadPacket(
                            Convert.ToByte(this.currentRequest.SystemIndex),
                            Convert.ToByte(this.currentRequest.DataSetIndex),
                            Convert.ToInt16(this.currentRequest.ParameterId));
                        break;
                    }

                case TypeOfRequest.SettingRequest:
                    {
                        var telegram = new Telegram();
                        telegramToSend = telegram.BuildWritePacket(
                            Convert.ToByte(this.currentRequest.SystemIndex),
                            Convert.ToByte(this.currentRequest.DataSetIndex),
                            Convert.ToInt16(this.currentRequest.ParameterId),
                            this.currentRequest.DataType,
                            this.currentRequest.Value);
                        break;
                    }
            }

            // Send the telegram related to the current request to inverter
            this.SendDataToInverter(telegramToSend);
        }

        ///<summary>
        /// Start waiting data from the inverter(via socket).
        /// </summary>
        private void WaitForData()
        {
            try
            {
                var packet = new SocketPacket { thisSocket = this.sckClient };

                var result = this.sckClient.BeginReceive(
                    packet.dataBuffer,
                    0,
                    packet.dataBuffer.Length,
                    SocketFlags.None,
                    new AsyncCallback(this.OnDataReceived),
                    packet
                );
            }
            catch (SocketException)
            {
                // logger.Log(LogLevel.Debug, String.Format("Asyncronously receive message invoke failed [error Message: {0}]", exc.Message));
                // TODO: Warning? Handle the exception?
            }
        }

        #endregion
    }
}

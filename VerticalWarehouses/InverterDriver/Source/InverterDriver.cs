using System;
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
        public const int LONG_TIME_OUT = 5000;
        public const int PORT_ADDR_INVERTER_DEFAULT = 17221;
        public const int SHORT_TIME_OUT = 50;
        public const int SIZEMAX_DATABUFFER = 1024;
        public const int TIME_OUT = 150;

        private static readonly object Lock = new object();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly InverterDriverState state;
        private AutoResetEvent ackTerminateEvent;
        private Request currentRequest;
        private InverterDriverErrors error;
        private bool executeRequestOnRunning;
        private HardwareInverterStatus hwInverterState;
        private string ipAddressToConnect;
        private Thread mainAutomationThread;
        private AutoResetEvent makeRequestEvent;
        private int portAddressToConnect;
        private RequestList requestList;
        private Socket sckClient;
        private AutoResetEvent terminateEvent;
        private int timeOut;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default c-tor.
        /// </summary>
        public InverterDriver()
        {
            this.error = InverterDriverErrors.NoError;
            this.state = InverterDriverState.Idle;
            this.ipAddressToConnect = IP_ADDR_INVERTER_DEFAULT;
            this.portAddressToConnect = PORT_ADDR_INVERTER_DEFAULT;
            this.hwInverterState = HardwareInverterStatus.NotOperative;

            logger.Log(LogLevel.Debug, String.Format("InverterDriver in a new incarnation..."));
        }

        #endregion Constructors

        #region Events

        public event ConnectedEventHandler Connected;

        public event ErrorEventHandler Error;

        // note: Does it remove?
        public event GetMessageFromServerEventHandler GetMessageFromServer;

        public event OperationDoneEventHandler OperationDone;

        #endregion Events

        #region Properties

        /// <summary>
        /// Get the last error.
        /// </summary>
        public InverterDriverErrors GetLastError => this.error;

        /// <summary>
        /// Get the main state of inverter driver.
        /// See the InverterDriverState enum
        /// </summary>
        public InverterDriverState GetMainState => this.state;

        /// <summary>
        /// Set/Get IP address to connect.
        /// Specify the IPv4 address family.
        /// </summary>
        public string IPAddressToConnect
        {
            get => this.ipAddressToConnect;
            set => this.ipAddressToConnect = value;
        }

        /// <summary>
        /// Set/Get port address to connect.
        /// Specify the IPv4 address family.
        /// </summary>
        public int PortAddressToConnect
        {
            set => this.portAddressToConnect = value;
            get => this.portAddressToConnect;
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

        public InverterDriverExitStatus GetIOState()
        {
            logger.Log(LogLevel.Debug, String.Format("> Execute GetIOState operation."));

            // Add your implementation code here

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Initialize the driver.
        /// </summary>
        public bool Initialize()
        {
            logger.Log(LogLevel.Debug, String.Format("InverterDriver initialization"));

            this.requestList = new RequestList();
            this.executeRequestOnRunning = false;

            this.createThread();

            var bResult = this.connect_to_inverter();
            return bResult;
        }

        /// <summary>
        /// Move along horizontal axis with given profile.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="a"></param>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="v2"></param>
        /// <param name="a1"></param>
        /// <param name="s3"></param>
        /// <param name="s4"></param>
        /// <param name="v3"></param>
        /// <param name="a2"></param>
        /// <param name="s5"></param>
        /// <param name="s6"></param>
        /// <param name="a3"></param>
        /// <param name="s7"></param>
        /// <returns></returns>
        public InverterDriverExitStatus MoveAlongHorizontalAxisWithProfile(float v1, float a, short s1, short s2,
            float v2, float a1, short s3, short s4, float v3, float a2, short s5, short s6, float a3, short s7)
        {
            logger.Log(LogLevel.Debug, String.Format("> Execute MoveAlongHorizontalAxisWithProfile operation."));

            // Add your implementation code here

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Move along vertical axis to given point.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="vMax"></param>
        /// <param name="acc"></param>
        /// <param name="dec"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public InverterDriverExitStatus MoveAlongVerticalAxisToPoint(short x, float vMax, float acc, float dec, float w)
        {
            logger.Log(LogLevel.Debug, String.Format("> Execute MoveAlongVerticalAxisToPoint operation (Parameter: x={0}, vMax={1}, acc={2}, dec={3}, w={4})", x, vMax, acc, dec, w));

            // Build the request list to perform the [MoveAlongVerticalAxisToPoint] operation
            this.requestList.build_For_MoveAlongVerticalAxisToPoint_Operation(x, vMax, acc, dec, w);

            // Start the operation
            this.makeRequestEvent.Set();

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Call back function which will be invoked when the socket detects the incoming data on the stream.
        /// </summary>
        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                var theSockId = (SocketPacket)asyn.AsyncState;
                var iRx = theSockId.thisSocket.EndReceive(asyn);
                var nBytes = theSockId.dataBuffer[0];

                // The class Telegram performs the parsing of incoming data buffer and extract the information from it

                var telegramRead = new byte[iRx];
                Array.Copy(theSockId.dataBuffer, 0, telegramRead, 0, iRx);

                //
                if (this.received_telegram(telegramRead, out var parameterID))
                {
                    if (parameterID == Request.STATUS_WORD_PARAM)
                    {
                        // a request of status has been performed to inverter

                        logger.Log(LogLevel.Debug, String.Format("< Response => get status"));
                    }
                    else
                    {
                        // delete the current request from the list
                        this.requestList.RemoveAt(0);

                        if (this.requestList.Count > 0)
                        {
                            // there are still requests to be executed, so
                            // fire the makeRequest event to notify the new request from the list
                            this.makeRequestEvent.Set();
                        }
                        else
                        {
                            // No other requests in the list
                            // Probably, we have to wait the execution of the last request (for example, we have to wait the elevator goes to the new position and it can required many seconds)

                            this.executeRequestOnRunning = false;

                            this.timeOut = LONG_TIME_OUT;
                        }
                    }

                    lock (Lock)
                    {
                        this.executeRequestOnRunning = false;
                    }
                }
                else
                {
                    // if we are here, an error occurs
                    // and so we have to manage it
                }

                this.waitForData();
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException)
            {
                // Warning?
            }
        }

        /// <summary>
        /// Run routine for detect the weight of current drawer.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="w"></param>
        /// <param name="a"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public InverterDriverExitStatus RunDrawerWeightRoutine(short d, float w, float a, byte e)
        {
            logger.Log(LogLevel.Debug, String.Format("> Execute RunDrawerWeightRoutine operation."));

            // Add your implementation code here

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Run shutter on opening movement or closing movement.
        /// </summary>
        /// <returns></returns>
        public InverterDriverExitStatus RunShutter(byte m)
        {
            logger.Log(LogLevel.Debug, String.Format("> Execute RunShutter operation."));

            // Add your implementation code here

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Set ON/OFF value to the given line.
        /// </summary>
        /// <returns></returns>
        public InverterDriverExitStatus Set(int i, byte value)
        {
            logger.Log(LogLevel.Debug, String.Format("> Execute Set operation."));

            // Add your implementation code here

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Select type of motor movement: vertical or horizontal movement.
        /// </summary>
        /// <returns></returns>
        public InverterDriverExitStatus SetTypeOfMotorMovement(byte m)
        {
            logger.Log(LogLevel.Debug, String.Format("> Execute SetTypeOfMotorMovement operation."));

            // Add your implementation code here

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Set vertical axis origin routine.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="vSearch"></param>
        /// <param name="vCam0"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public InverterDriverExitStatus SetVerticalAxisOrigin(byte mode, float vSearch, float vCam0, float offset)
        {
            logger.Log(LogLevel.Debug, String.Format("> Execute SetVerticalAxisOrigin operation (Parameter: mode={0}, offset={1}, vSearch={2}, vCam0={3})", mode, offset, vSearch, vCam0));

            // Build the request list to perform the [SetVerticalAxisOrigin] operation
            this.requestList.build_For_SetVerticalAxisOrigin_Operation(mode, vSearch, vCam0, offset);

            // Start the operation
            this.makeRequestEvent.Set();

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Stop.
        /// </summary>
        public InverterDriverExitStatus Stop()
        {
            logger.Log(LogLevel.Debug, String.Format("> Execute Stop operation."));

            // Add your implementation code here

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
            this.error = InverterDriverErrors.NoError;
            var bSuccess = true;

            if (this.ipAddressToConnect == "" || this.portAddressToConnect <= 0)
            {
                logger.Log(LogLevel.Debug, String.Format("Invalid IP address [IP:{0}, port:{1}]", this.ipAddressToConnect, this.portAddressToConnect));
                this.error = InverterDriverErrors.IOError;
                Error?.Invoke(this, new ErrorEventArgs(this.error));
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
                var ipAddr = IPAddress.Parse(this.ipAddressToConnect);
                var iPortNumber = this.portAddressToConnect;
                this.sckClient = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                var ipEnd = new IPEndPoint(ipAddr, iPortNumber);
                this.sckClient.Connect(ipEnd);
                if (this.sckClient.Connected)
                {
                    logger.Log(LogLevel.Debug, String.Format("Connection to inverter [IP:{0}] established", this.ipAddressToConnect));
                    Connected?.Invoke(this, new ConnectedEventArgs(true));
                    this.waitForData();
                }
                else
                {
                    logger.Log(LogLevel.Debug, String.Format("Unable to connect to inverter [IP:{0}]", this.ipAddressToConnect));
                    Connected?.Invoke(this, new ConnectedEventArgs(false));
                }
            }
            catch (SocketException exc)
            {
                logger.Log(LogLevel.Debug, String.Format("Connection to inverter failed [error message: {0}]", exc.Message));
                this.error = InverterDriverErrors.GenericError;
                Error?.Invoke(this, new ErrorEventArgs(this.error));
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

        /// <summary>
        /// Release resource of working thread.
        /// </summary>
        private void destroyThread()
        {
            // Wait for the release of main working thread
            var handles = new WaitHandle[1];
            handles[0] = this.ackTerminateEvent;
            WaitHandle.WaitAny(handles, -1);

            if (null != this.mainAutomationThread)
            {
                this.mainAutomationThread.Abort();
            }

            if (null != this.terminateEvent)
            {
                this.terminateEvent.Close();
            }
            this.terminateEvent = null;

            if (null != this.makeRequestEvent)
            {
                this.makeRequestEvent.Close();
            }
            this.makeRequestEvent = null;

            if (null != this.ackTerminateEvent)
            {
                this.ackTerminateEvent.Close();
            }
            this.ackTerminateEvent = null;

            logger.Log(LogLevel.Debug, String.Format("Release main Working thread."));
        }

        private void disconnect_from_inverter()
        {
            if (this.sckClient != null)
            {
                this.sckClient.Close();
                this.sckClient = null;
            }

            Connected?.Invoke(this, new ConnectedEventArgs(false));
        }

        private void get_inverter_status()
        {
            // -----------------------------------------------------
            // -----------------------------------------------------
            // Write the telegram to get state and send it
            // Build the telegram with the methods of Telegram class
            byte[] telegramToSend = null;

            var nBytesOfTelegram = 6;
            telegramToSend = new byte[nBytesOfTelegram];
            Array.Clear(telegramToSend, 0, nBytesOfTelegram);

            // header : Bit7 ==> 0: Read
            telegramToSend[0] = 0x00;
            telegramToSend[0] |= 0 << 7;

            // No. bytes
            telegramToSend[1] = 4;

            // Sys
            telegramToSend[2] = 0x00;

            // Ds
            telegramToSend[3] = 0x00;

            // Parameter No.
            var ans = new byte[2];
            var parameterNo = new byte[sizeof(short)];
            parameterNo = BitConverter.GetBytes(Request.STATUS_WORD_PARAM);
            parameterNo.CopyTo(ans, 0);

            Array.Copy(ans, 0, telegramToSend, 4, 2);

            logger.Log(LogLevel.Debug, String.Format(" > Request => TypeOf:{0}, ParamID:{1}", TypeOfRequest.Read.ToString(), Request.STATUS_WORD_PARAM));

            this.sendDataToInverter(telegramToSend);

            lock (Lock)
            {
                this.executeRequestOnRunning = true;
            }
            // -----------------------------------------------------
            // -----------------------------------------------------
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
                }
            }

            return IP;
        }

        /// <summary>
        /// Main working thread.
        /// The thread monitors the Terminate event and the Make Request event.
        ///   The terminateEvent get signalled when the [this] class is disposed (release)
        ///   The makeRequestEvent get signalled when a new request is executed. The request belongs to the request list for the current operation.
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
                            // the event, for execute a new request belonging to the list, has been signalled (it is fired at the start of execution operation
                            // and when telegram messages are received from the inverter via socket)

                            // 1. read current request from the list (select the first) and
                            // 2. update the internal variable related to the <current_request_to_perform>
                            this.currentRequest = this.requestList[0];

                            logger.Log(LogLevel.Debug, String.Format("Execute request => TypeOf:{0}, ParamID:{1}", this.currentRequest.Type.ToString(), this.currentRequest.ParameterID.ToString()));

                            // 3. build the telegram according to data of request to send to inverter
                            this.send_request_to_inverter();

                            this.timeOut = SHORT_TIME_OUT;
                            break;
                        }

                    case WaitHandle.WaitTimeout:
                        {
                            // Check if ACK from inverter is catched... and it handle the error for the not execution
                            if (this.executeRequestOnRunning)
                            {
                                // A request was made, but no response has been collected
                                // Handle it!

                                this.error = InverterDriverErrors.IOError;
                                Error?.Invoke(this, new ErrorEventArgs(this.error));
                            }
                            else
                            {
                                // Send a request to inverter about the status
                                this.get_inverter_status();
                            }
                            break;
                        }
                }
            }

            this.ackTerminateEvent.Set();
            logger.Log(LogLevel.Debug, String.Format("Exit from main Working thread."));
            return;
        }

        private bool received_telegram(byte[] telegram, out short parameterID)
        {
            // Parsing and check the information of telegram

            // ----------------------------
            // ----------------------------
            parameterID = 0;
            const byte BIT_MASK_6 = 0x40;

            if (telegram.Length == 0)
            {
                return false;
            }

            // header
            var header = telegram[0];
            // check error condition on header
            var bError = ((header & BIT_MASK_6) == 0x40);
            if (bError)
            {
                logger.Log(LogLevel.Debug, String.Format(" < Response => Error"));
                return false;
            }

            // parameter No
            var parameterNo = new byte[2];
            Array.Copy(telegram, 4, parameterNo, 0, 2);
            parameterNo.Reverse();
            parameterID = BitConverter.ToInt16(parameterNo, 0);

            // parameter value
            var parameterValue = new byte[2];
            Array.Copy(telegram, 6, parameterValue, 0, 2);
            parameterValue.Reverse();
            var value = BitConverter.ToInt16(parameterValue, 0);

            var typeOfOperation = (header == 0x00) ? "Read" : "Write";
            logger.Log(LogLevel.Debug, String.Format(" < Response => TypeOf:{0}, ParamID:{1}, Value:{2}", typeOfOperation, parameterID, value));
            // ----------------------------
            // ----------------------------

            return true;
        }

        private void send_request_to_inverter()
        {
            // the currentRequest object contains the definition of request need to send to inverter

            // Build the telegram with the methods of Telegram class
            byte[] telegramToSend = null;
            switch (this.currentRequest.Type)
            {
                case TypeOfRequest.Read:
                    {
                        //telegramToSend = Telegram.BuildReadPacket(currentRequest.ParameterID, currentRequest.SysIndex, currentRequest.DsIndex);

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

                case TypeOfRequest.Write:
                    {
                        //telegramToSend = Telegram.BuildWritePacket(currentRequest.ParameterID, currentRequest.SysIndex, currentRequest.DsIndex, currentRequest.ParameterValueInt32);

                        // ---------------------------------
                        // ---------------------------------
                        var nBytesOfTelegram = 8;
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
                        var value = 0;
                        switch (this.currentRequest.DataType)
                        {
                            case ValueDataType.Byte: value = this.currentRequest.ParameterValueByte; break;
                            case ValueDataType.Float: value = (int)this.currentRequest.ParameterValueFloat; break;
                            case ValueDataType.Int16: value = this.currentRequest.ParameterValueInt16; break;
                            case ValueDataType.Int32: value = this.currentRequest.ParameterValueInt32; break;
                        }
                        ans = new byte[2];
                        var valueBytes = new byte[sizeof(short)];
                        valueBytes = BitConverter.GetBytes(Convert.ToInt16(value));
                        valueBytes.CopyTo(ans, 0);

                        Array.Copy(ans, 0, telegramToSend, 6, 2);

                        logger.Log(LogLevel.Debug, String.Format(" > Request => TypeOf:{0}, ParamID:{1} Value:{2}", this.currentRequest.Type.ToString(), this.currentRequest.ParameterID.ToString(), value));
                        // -----------------------------------
                        // -----------------------------------

                        break;
                    }
            }

            // Send the telegram related to the current request to inverter
            this.sendDataToInverter(telegramToSend);

            // Update the flag
            lock (Lock)
            {
                this.executeRequestOnRunning = true;
            }
        }

        /// <summary>
        /// Send a given telegram data to inverter.
        /// </summary>
        /// <param name="byTelegramToSend"></param>
        private void sendDataToInverter(byte[] byTelegramToSend)
        {
            try
            {
                if (null != byTelegramToSend)
                {
                    if (this.sckClient != null)
                    {
                        this.sckClient.Send(byTelegramToSend);
                    }
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

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Ferretto.VW.Utils;

namespace Ferretto.VW.InverterDriver
{
    /// <summary>
    /// Inverter manager class.
    /// This class manages a socket to comunicate with a device via TCP/IP protocol. The socket in the class acts as a client in an client/server architecture
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
        private readonly HardwareInverterStatus hwInverterState;
        private readonly InverterDriverState state;
        private AutoResetEvent ackTerminateEvent;
        private Request currentRequest;
        private byte[] DataBufferCommand;
        private InverterDriverErrors error;
        private bool executeRequestOnRunning;
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
            this.DataBufferCommand = new byte[SIZEMAX_DATABUFFER];
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
            this.DataBufferCommand[2] = Convert.ToByte(CommandId.GetDrawerWeight);

            var convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(ic);
            convertionBuffer.CopyTo(this.DataBufferCommand, 3);
            /// <summary>
            ///  set the byte of command to be executed
            /// </summary>
            lock (Lock)
            {
                this.DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }

        public InverterDriverExitStatus GetIOEmergencyState()
        {
            this.DataBufferCommand[2] = Convert.ToByte(CommandId.GetIOEmergencyState);

            lock (Lock)
            {
                this.DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }

        public InverterDriverExitStatus GetIOState()
        {
            this.DataBufferCommand[2] = Convert.ToByte(CommandId.GetIOState);

            lock (Lock)
            {
                this.DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Get IP address of local machine.
        /// </summary>
        public string getLocalIPAddress()
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
        /// Initialize the driver.
        /// </summary>
        public bool Initialize()
        {
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

                if (this.received_telegram(telegramRead))
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
            // Add your implementation code here

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Run shutter on opening movement or closing movement.
        /// </summary>
        /// <returns></returns>
        public InverterDriverExitStatus RunShutter(byte m)
        {
            // Add your implementation code here

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Set ON/OFF value to the given line.
        /// </summary>
        /// <returns></returns>
        public InverterDriverExitStatus Set(int i, byte value)
        {
            // Add your implementation code here

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Select type of motor movement: vertical or horizontal movement.
        /// </summary>
        /// <returns></returns>
        public InverterDriverExitStatus SetTypeOfMotorMovement(byte m)
        {
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
            this.DataBufferCommand = null;
            this.disconnect_from_inverter();
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
                    Connected?.Invoke(this, new ConnectedEventArgs(true));
                    this.waitForData();
                }
                else
                {
                    Connected?.Invoke(this, new ConnectedEventArgs(false));
                }
            }
            catch (SocketException)
            {
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
            this.terminateEvent = new AutoResetEvent(false);
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
                            // event for execute a new request belonging to the list, has been signalled (it is fired at the start of execution operation
                            // and when telegram messages are received from the inverter via socket)

                            // 1. read current request from the list (select the first) and
                            // 2. update the internal variable related to the <current_request_to_perform>
                            this.currentRequest = this.requestList[0];

                            // 3. build the telegram according to data of request to send to inverter
                            this.send_request_to_inverter();

                            this.timeOut = SHORT_TIME_OUT;
                            break;
                        }

                    case WaitHandle.WaitTimeout:
                        {
                            // Check if ACK from inverter is catched... and it handle the error for the not execution
                            break;
                        }
                }
            }

            this.ackTerminateEvent.Set();
            return;
        }

        private bool received_telegram(byte[] telegram)
        {
            // Parsing and check the information of telegram
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
                        break;
                    }

                case TypeOfRequest.Write:
                    {
                        //telegramToSend = Telegram.BuildWritePacket(currentRequest.ParameterID, currentRequest.SysIndex, currentRequest.DsIndex, currentRequest.ParameterValueInt32);
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
            catch (SocketException)
            {
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
            catch (SocketException)
            {
                // TODO: Warning? Handle the exception?
            }
        }

        #endregion Methods
    }
}

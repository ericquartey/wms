using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Ferretto.VW.Utils;

namespace Ferretto.VW.InverterDriver
{
    /// <summary>
    ///Delegate for the Connected event.
    /// </summary>
    public delegate void ConnectedEventHandler(object sender, ConnectedEventArgs eventArgs);

    /// <summary>
    /// Delegate for Getting Messages From the Server
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    public delegate void GetMessageFromServerEventHandler(object sender, GetMessageFromServerEventArgs eventArgs);

    /// <summary>
    /// Status of inverter machine.
    /// </summary>
    public enum HardwareInverterStatus
    {
        NotOperative = 0x0,
        Operative = 0x1
    }

    /// <summary>
    /// Inverter Driver errors.
    /// </summary>
    public enum InverterDriverErrors
    {
        /// <summary>
        /// No error: no error encountered
        /// </summary>
        NoError = 0x00,

        /// <summary>
        /// Hardware error: not recovery condition
        /// </summary>
        HardwareError,

        /// <summary>
        /// IO error: communication error
        /// </summary>
        IOError,

        /// <summary>
        /// Internal error: software errors
        /// </summary>
        InternalError,

        /// <summary>
        /// Generic error: generic error
        /// </summary>
        GenericError = 0xFF
    }

    /// <summary>
    /// Inverter Driver exist status.
    /// </summary>
    public enum InverterDriverExitStatus
    {
        /// <summary>
        /// Successful operation
        /// </summary>
        Success = 0x0,

        /// <summary>
        /// Invalid argument
        /// </summary>
        InvalidArgument,

        /// <summary>
        /// Invalid operation
        /// </summary>
        InvalidOperation,

        /// <summary>
        /// Generic failure: see Errors enum
        /// </summary>
        Failure = 0xFF
    }

    /// <summary>
    /// Inverter states.
    /// </summary>
    public enum InverterDriverState
    {
        /// <summary>
        /// Idle: not connected
        /// </summary>
        Idle,

        /// <summary>
        /// Ready: initialized and ready to operate
        /// </summary>
        Ready,

        /// <summary>
        /// Working: perform an operation
        /// </summary>
        Working,

        /// <summary>
        /// Error: the Inverter occurs in an irreversible error state
        /// </summary>
        Error
    }

    /// <summary>
    /// Connected interface IConnectedEvent
    /// </summary>

    [
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    ]
    public interface IConnectedEvent
    {
        #region Methods

        void Connected(object sender, ConnectedEventArgs eventArgs);

        #endregion Methods
    }

    /// <summary>
    /// Connected event arguments interface.
    /// Use IDispatch.
    /// </summary>
    [
        InterfaceType(ComInterfaceType.InterfaceIsIDispatch),
    ]
    public interface IConnectedEventArgs
    {
        #region Properties

        bool State { get; }

        #endregion Properties
    }

    /// <summary>
    /// GetMessageFromServer event interface.
    /// </summary>
    [
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    ]
    public interface IGetMessageFromServerEvent
    {
        #region Methods

        /// <summary>
        /// GetMessageFromServer Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        void GetMessageFromServer(object sender, GetMessageFromServerEventArgs eventArgs);

        #endregion Methods
    }

    [
            InterfaceType(ComInterfaceType.InterfaceIsIDispatch),
        ]
    public interface IGetMessageFromServerEventArgs
    {
        #region Properties

        /// <summary>
        /// Command Id.
        /// </summary>
        CommandId CmdId { get; }

        /// <summary>
        /// Message.
        /// </summary>
        string Message { get; }

        #endregion Properties
    }

    /// <summary>
    /// Inverter manager class.
    /// This class manages a socket to comunicate with a device via TCP/IP protocol. The socket in the class acts as a client in an client/server architecture
    /// (see System.Net.Sockets.Socket class for the implementation details).
    /// </summary>
    public class CInverterDriver : IDriverBase, IDriver, IDisposable
    {
        #region Fields

        // Consts
        public const string IP_ADDR_INVERTER_DEFAULT = "172.16.199.200";

        public const int PORT_ADDR_INVERTER_DEFAULT = 8000;

        public const int SIZEMAX_DATABUFFER = 1024;

        // Maximum size of data buffer for command
        public const int TIME_OUT = 150; //50;

        private static readonly object g_lock = new object();

        // Time out for main thread (ms)
        private readonly InverterDriverState m_state;

        // Port address of inverter (manifactured port address)
        private byte[] m_DataBufferCommand;

        // IP address of inverter (manifactured IP address)
        private InverterDriverErrors m_error;

        //!< Data buffer containing the operation arguments
        private AutoResetEvent m_hevAckTerminate;

        //!< State
        //!< Error flag
        private AutoResetEvent m_hevTerminate;

        private HardwareInverterStatus m_hwInverterState;

        //!< Stop event for automated operation
        //!< Acknowledge for terminate
        private Thread m_opThread;

        private int m_portAddressToConnect;

        //!< IP address to connect
        private string m_szIPAddressToConnect;

        private int msgCounter;

        //!< Address port to connect
        //!< Reference to a callback for connecting to server socket
        private IAsyncResult result;

        private Socket sckClient;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default c-tor.
        /// </summary>
        public CInverterDriver()
        {
            this.m_error = InverterDriverErrors.NoError;
            this.m_state = InverterDriverState.Idle;

            this.m_hevTerminate = null;
            this.m_hevAckTerminate = null;
            this.m_opThread = null;

            this.m_szIPAddressToConnect = IP_ADDR_INVERTER_DEFAULT;
            this.m_portAddressToConnect = PORT_ADDR_INVERTER_DEFAULT;

            this.m_DataBufferCommand = new byte[SIZEMAX_DATABUFFER];
            Array.Clear(this.m_DataBufferCommand, 0, SIZEMAX_DATABUFFER);
        }

        #endregion Constructors

        #region Events

        public event ConnectedEventHandler Connected;

        public event GetMessageFromServerEventHandler GetMessageFromServer;

        #endregion Events

        #region Properties

        /// <summary>
        /// Set/Get IP address to connect.
        /// Specify the IPv4 address family.
        /// </summary>
        public string IPAddressToConnect
        {
            get => this.m_szIPAddressToConnect;
            set => this.m_szIPAddressToConnect = value;
        }

        //!< Asynchronuos operation status
        //!< Socket client
        //!< Messages from server counter
        /// <summary>
        /// Set/Get port address to connect.
        /// Specify the IPv4 address family.
        /// </summary>
        public int PortAddressToConnect
        {
            set => this.m_portAddressToConnect = value;
            get => this.m_portAddressToConnect;
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

        //! Get the drawer weight
        public InverterDriverExitStatus GetDrawerWeight(float ic)
        {
            this.m_DataBufferCommand[2] = Convert.ToByte(CommandId.GetDrawerWeight);

            // ic
            var convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(ic);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 3);
            // set the byte of command to be executed
            lock (g_lock)
            {
                this.m_DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }

        // -------
        // Events
        //! Get IO emergency sensors state.
        public InverterDriverExitStatus GetIOEmergencyState()
        {
            this.m_DataBufferCommand[2] = Convert.ToByte(CommandId.GetIOEmergencyState);

            // set the byte of command to be executed
            lock (g_lock)
            {
                this.m_DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }

        //!< Data buffer for the command
        //! Get IO sensor state.
        public InverterDriverExitStatus GetIOState()
        {
            this.m_DataBufferCommand[2] = Convert.ToByte(CommandId.GetIOState);

            // set the byte of command to be executed
            lock (g_lock)
            {
                this.m_DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Get IP address of local machine.
        /// </summary>
        /// <returns>The IP address.</returns>
        public string GetIP()
        {
            var strHostName = Dns.GetHostName();

            // Find host by name
            var iphostentry = Dns.GetHostEntry(strHostName);

            // Get the first IP addresses
            var szIP = "";
            foreach (var ipaddress in iphostentry.AddressList)
            {
                // IPv4
                if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    szIP = ipaddress.ToString();
                    return szIP;
                }

                // IPv6
                if (ipaddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    // TODO: Add your implementation code here
                }
            }

            return szIP;
        }

        //! Get main status of inverter.
        public InverterDriverExitStatus GetMainState()
        {
            this.m_DataBufferCommand[2] = Convert.ToByte(CommandId.GetMainState);

            // set the byte of command to be executed
            lock (g_lock)
            {
                this.m_DataBufferCommand[1] = 0x01;
            }

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Initialize the driver.
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            // create the thread
            this.createThread();

            // create the socket and connect to inverter
            var bResult = this.connect_to_inverter();

            return bResult;
        }

        //! Move along horizontal axis with given profile.
        public InverterDriverExitStatus MoveAlongHorizontalAxisWithProfile(float v1, float a, short s1, short s2,
            float v2, float a1, short s3, short s4, float v3, float a2, short s5, short s6, float a3, short s7)
        {
            this.m_DataBufferCommand[2] = Convert.ToByte(CommandId.MoveAlongHorizontalAxisWithProfile);

            // v1
            var convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(v1);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 3);
            // a
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 7);

            // s1
            convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(s1);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 11);
            // s2
            convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(s2);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 13);
            //v2
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(v2);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 15);
            // a1
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a1);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 19);

            // s3
            convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(s3);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 23);
            // s4
            convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(s4);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 25);
            //v3
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(v3);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 27);
            // a2
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a2);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 31);

            // s5
            convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(s5);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 35);
            // s6
            convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(s6);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 37);
            // a3
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a3);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 39);

            // s7
            convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(s7);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 43);

            // set the byte of command to be executed
            lock (g_lock)
            {
                this.m_DataBufferCommand[1] = 0x01;
            }

            return InverterDriverExitStatus.Success;
        }

        //! Move along vertical axis to given point.
        public InverterDriverExitStatus MoveAlongVerticalAxisToPoint(short x, float vMax, float a, float a1, float w)
        {
            this.m_DataBufferCommand[2] = Convert.ToByte(CommandId.MoveAlongVerticalAxisToPoint);

            // x
            var convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(x);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 3);
            // vMax
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(vMax);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 5);

            // a
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 9);
            // a1
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a1);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 13);
            //w
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(w);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 17);

            // set the byte of command to be executed
            lock (g_lock)
            {
                this.m_DataBufferCommand[1] = 0x01;
            }

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Call back function which will be invoked when the socket detects the incoming data on the stream.
        /// </summary>
        /// <param name="asyn"></param>
        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                var theSockId = (SocketPacket)asyn.AsyncState;
                var iRx = theSockId.thisSocket.EndReceive(asyn);

                // theSockId.dataBuffer is the data buffer containing the response send by the inverter

                // Translate this data buffer according to the Bonfiglioli protocol inverter
                var nBytes = theSockId.dataBuffer[0];
                var cmdId = theSockId.dataBuffer[1];
                var state = theSockId.dataBuffer[2];
                var szMessage = (state == 0x00) ? "Failed" : "Success";
                var id = CommandId.None;
                switch (cmdId)
                {
                    case 0x00:
                        id = CommandId.SetVerticalAxisOrigin;
                        break;

                    case 0x01:
                        id = CommandId.MoveAlongVerticalAxisToPoint;
                        break;

                    case 0x02:
                        id = CommandId.SelectMovement;
                        break;

                    case 0x03:
                        id = CommandId.MoveAlongHorizontalAxisWithProfile;
                        break;

                    case 0x05:
                        id = CommandId.RunShutter;
                        break;

                    case 0x06:
                        id = CommandId.RunDrawerWeightRoutine;
                        break;

                    case 0x07:
                        id = CommandId.GetDrawerWeight;
                        break;

                    case 0x08:
                        id = CommandId.Stop;
                        break;

                    case 0x09:
                        {
                            id = CommandId.GetMainState;
                            this.m_hwInverterState = (0x01 == theSockId.dataBuffer[2])
                                ? HardwareInverterStatus.Operative
                                : HardwareInverterStatus.NotOperative;

                            szMessage = this.m_hwInverterState.ToString();
                            break;
                        }
                    case 0x0A:
                        id = CommandId.GetIOState;
                        break;

                    case 0x0B:
                        id = CommandId.GetIOEmergencyState;
                        break;

                    case 0x0C:
                        id = CommandId.Set;
                        break;

                    case 0xFF:
                        id = CommandId.None;
                        break;
                }

                var result = theSockId.dataBuffer[2];

                this.msgCounter++;

                if (null != GetMessageFromServer)
                {
                    GetMessageFromServer(this, new GetMessageFromServerEventArgs(szMessage, id));
                }

                this.waitForData();
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException)
            {
                // TODO: Log a warning?
            }
        }

        //! Run routine for detect the weight of current drawer.
        public InverterDriverExitStatus RunDrawerWeightRoutine(short d, float w, float a, byte e)
        {
            this.m_DataBufferCommand[2] = Convert.ToByte(CommandId.RunDrawerWeightRoutine);

            // d
            var convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(d);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 3);
            // w
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(w);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 5);
            // a
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 9);
            // e
            this.m_DataBufferCommand[13] = e;
            // set the byte of command to be executed
            lock (g_lock)
            {
                this.m_DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }

        //! Run shutter on opening movement or closing movement.
        public InverterDriverExitStatus RunShutter(byte m)
        {
            // write the command id in the data buffer
            this.m_DataBufferCommand[2] = Convert.ToByte(CommandId.RunShutter);

            // set the byte of command to be executed
            lock (g_lock)
            {
                this.m_DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }

        //! Select movement among vertical movement and horizontal movement.
        public InverterDriverExitStatus SelectMovement(byte m)
        {
            //
            this.m_DataBufferCommand[3] = m;

            // write the command id in the data buffer
            this.m_DataBufferCommand[2] = Convert.ToByte(CommandId.SelectMovement);

            // set the byte of command to be executed
            lock (g_lock)
            {
                this.m_DataBufferCommand[1] = 0x01;
            }

            return InverterDriverExitStatus.Success;
        }

        //! Set ON/OFF value to the given line.
        public InverterDriverExitStatus Set(int i, byte value)
        {
            this.m_DataBufferCommand[2] = Convert.ToByte(CommandId.Set);

            //i
            var convertionBuffer = new byte[sizeof(int)];
            convertionBuffer = BitConverter.GetBytes(i);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 3);

            // value
            this.m_DataBufferCommand[7] = value;

            // set the byte of command to be executed
            lock (g_lock)
            {
                this.m_DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }

        //! Set vertical axis origin routine.
        public InverterDriverExitStatus SetVerticalAxisOrigin(byte direction, float vSearch, float vCam0, float a,
            float a1, float a2)
        {
            this.m_DataBufferCommand[2] = Convert.ToByte(CommandId.SetVerticalAxisOrigin);

            // direction
            this.m_DataBufferCommand[3] = direction;

            // vSearch
            var convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(vSearch);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 4);

            // vCam0
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(vCam0);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 8);
            // a
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 12);
            // a1
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a1);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 16);
            //a2
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a2);
            convertionBuffer.CopyTo(this.m_DataBufferCommand, 20);

            // set the byte of command to be executed
            lock (g_lock)
            {
                this.m_DataBufferCommand[1] = 0x01;
            }

            return InverterDriverExitStatus.Success;
        }

        // -----------------------------------------------------------------
        // IDriver interface
        //! Stop.
        public InverterDriverExitStatus Stop()
        {
            this.m_DataBufferCommand[2] = Convert.ToByte(CommandId.Stop);

            // set the byte of command to be executed
            lock (g_lock)
            {
                this.m_DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Terminate and release driver' resources.
        /// </summary>
        public void Terminate()
        {
            this.m_hevTerminate.Set();

            // destroy thread resources
            this.destroyThread();

            this.m_DataBufferCommand = null;

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
                // release unmanaged resources
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceFrequency(out long lpPerformanceFrequency);

        private bool build_telegram_and_send()
        {
            var cmdId = this.byte2CommandId(this.m_DataBufferCommand[2]);
            byte nBytes = 0;
            switch (cmdId)
            {
                case CommandId.SetVerticalAxisOrigin:
                    nBytes = 22;
                    break;

                case CommandId.MoveAlongVerticalAxisToPoint:
                    nBytes = 19;
                    break;

                case CommandId.SelectMovement:
                    nBytes = 2;
                    break;

                case CommandId.MoveAlongHorizontalAxisWithProfile:
                    nBytes = 43;
                    break;

                case CommandId.GetMainState:
                    nBytes = 1;
                    break;

                case CommandId.RunShutter:
                    nBytes = 2;
                    break;

                case CommandId.RunDrawerWeightRoutine:
                    nBytes = 12;
                    break;

                case CommandId.GetDrawerWeight:
                    nBytes = 5;
                    break;

                case CommandId.Stop:
                    nBytes = 1;
                    break;

                case CommandId.GetIOState:
                    nBytes = 1;
                    break;

                case CommandId.GetIOEmergencyState:
                    nBytes = 1;
                    break;

                case CommandId.Set:
                    nBytes = 6;
                    break;
            }

            // read arguments from data buffer

            var byTelegramToSend = new byte[nBytes + 1];
            byTelegramToSend[0] = nBytes;
            Array.Copy(this.m_DataBufferCommand, 2, byTelegramToSend, 1, nBytes);

            // build the telegram according the arguments and the protocol provided by Bonfiglioli inverter

            this.sendDataToServer(byTelegramToSend);

            lock (g_lock)
            {
                this.m_DataBufferCommand[1] = 0x00;
            }

            return true;
        }

        private CommandId byte2CommandId(byte value)
        {
            var cmdId = CommandId.None;
            switch (value)
            {
                case 0x00:
                    cmdId = CommandId.SetVerticalAxisOrigin;
                    break;

                case 0x01:
                    cmdId = CommandId.MoveAlongVerticalAxisToPoint;
                    break;

                case 0x02:
                    cmdId = CommandId.SelectMovement;
                    break;

                case 0x03:
                    cmdId = CommandId.MoveAlongHorizontalAxisWithProfile;
                    break;

                case 0x09:
                    cmdId = CommandId.GetMainState;
                    break;

                case 0x05:
                    cmdId = CommandId.RunShutter;
                    break;

                case 0x06:
                    cmdId = CommandId.RunDrawerWeightRoutine;
                    break;

                case 0x07:
                    cmdId = CommandId.GetDrawerWeight;
                    break;

                case 0x08:
                    cmdId = CommandId.Stop;
                    break;

                case 0x0A:
                    cmdId = CommandId.GetIOState;
                    break;

                case 0x0B:
                    cmdId = CommandId.GetIOEmergencyState;
                    break;

                case 0x0C:
                    cmdId = CommandId.Set;
                    break;
            }

            return cmdId;
        }

        private bool connect_to_inverter()
        {
            this.m_error = InverterDriverErrors.NoError;

            // See if we have text on the IP and Port text fields
            if (this.m_szIPAddressToConnect == "" || this.m_portAddressToConnect <= 0)
            {
                this.m_error = InverterDriverErrors.IOError;
                return false;
            }

            try
            {
                // Create one SocketPermission for socket access restrictions
                var permission = new SocketPermission(
                    NetworkAccess.Connect, // Connection permission
                    TransportType.Tcp, // Defines transport types
                    "", // Gets the IP addresses
                    SocketPermission.AllPorts // All ports
                );

                // Ensures the code to have permission to access a Socket
                permission.Demand();

                // Resolves a host name to an IPHostEntry instance
                var ipHost = Dns.GetHostEntry("");

                // Gets first IP address associated with a localhost
                //IPAddress ipAddr = ipHost.AddressList[0];

                // Get the given IP address
                var ipAddr = IPAddress.Parse(this.m_szIPAddressToConnect);
                // Assign the address port
                var iPortNumber = this.m_portAddressToConnect;

                // Create the socket instance
                this.sckClient = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                //m_sckClient.NoDelay = false;   // Using the Nagle algorithm

                // Create the end point
                var ipEnd = new IPEndPoint(ipAddr, iPortNumber);
                // Connect to the remote host
                this.sckClient.Connect(ipEnd);
                if (this.sckClient.Connected)
                {
                    Connected?.Invoke(this, new ConnectedEventArgs(true));

                    // Wait for data asynchronously
                    this.waitForData();
                }
                else
                {
                    Connected?.Invoke(this, new ConnectedEventArgs(false));
                }
            }
            catch (SocketException)
            {
                // TODO: Log a warning?
                this.m_error = InverterDriverErrors.GenericError;
            }

            return true;
        }

        /// <summary>
        /// Create working thread.
        /// </summary>
        private void createThread()
        {
            this.m_hevTerminate = new AutoResetEvent(false);
            this.m_hevAckTerminate = new AutoResetEvent(false);

            // Run an internal thread to perform operation
            this.m_opThread = new Thread(this.mainThread);
            this.m_opThread.Name = "workingInverterThread";
            this.m_opThread.Start();
        }

        /// <summary>
        /// Release resource of working thread.
        /// </summary>
        private void destroyThread()
        {
            var handles = new WaitHandle[1];
            handles[0] = this.m_hevAckTerminate;
            WaitHandle.WaitAny(handles, -1);

            if (null != this.m_opThread)
            {
                this.m_opThread.Abort();
            }

            if (null != this.m_hevTerminate)
            {
                this.m_hevTerminate.Close();
            }

            this.m_hevTerminate = null;

            if (null != this.m_hevAckTerminate)
            {
                this.m_hevAckTerminate.Close();
            }

            this.m_hevAckTerminate = null;
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
        /// Check the existence of pending operation and send the command to inverter.
        /// </summary>
        /// <returns></returns>
        private bool existPendingOperation()
        {
            var bExistsPendingOperation = false;
            lock (g_lock)
            {
                bExistsPendingOperation = (this.m_DataBufferCommand[1] == 0x01);
            }

            return bExistsPendingOperation;
        }

        private bool get_main_state_of_inverter()
        {
            //
            var exitCode = this.GetMainState();

            return true;
        }

        /// <summary>
        /// Working thread.
        /// </summary>
        private void mainThread()
        {
            const int EV_TERMINATE = 0;

            var handles = new WaitHandle[1];
            handles[0] = this.m_hevTerminate;

            var bExit = false;
            while (!bExit)
            {
                var code = WaitHandle.WaitAny(handles, TIME_OUT);
                switch (code)
                {
                    case EV_TERMINATE:
                        {
                            // Exit from thread
                            bExit = true;
                            break;
                        }
                    case WaitHandle.WaitTimeout:
                        {
                            if (this.existPendingOperation())
                            {
                                // an operation must be executed, so I send it to the inverter
                                this.build_telegram_and_send();
                            }
                            else
                            {
                                // if no operation, then check the status of inverter (send a command to inverter to check the state)
                                //this.get_main_state_of_inverter();
                            }

                            break;
                        }
                }
            }

            this.m_hevAckTerminate.Set();
        }

        /// <summary>
        /// Send a given telegram data to server.
        /// </summary>
        /// <param name="byTelegramToSend"></param>
        private void sendDataToServer(byte[] byTelegramToSend)
        {
            try
            {
                if (null != byTelegramToSend)
                {
                    if (this.sckClient != null)
                    {
                        // Send data
                        this.sckClient.Send(byTelegramToSend);
                    }
                }
            }
            catch (SocketException)
            {
                // TODO: Log a warning?
            }
        }

        /// <summary>
        /// Start waiting data from the server.
        /// </summary>
        private void waitForData()
        {
            try
            {
                var theSocPkt = new SocketPacket();
                theSocPkt.thisSocket = this.sckClient;
                // Start listening to the data asynchronously
                this.result = this.sckClient.BeginReceive(
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
                // TODO: Log a warning
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// Connected event arguments.
    /// </summary>
    [
        ClassInterface(ClassInterfaceType.None),
    ]
    public class ConnectedEventArgs : EventArgs, IConnectedEventArgs
    {
        #region Fields

        private readonly bool _state;

        #endregion Fields

        #region Constructors

        public ConnectedEventArgs(bool State)
        {
            this._state = State;
        }

        #endregion Constructors

        #region Properties

        public bool State => this._state;

        #endregion Properties
    }

    /// <summary>
    /// GetMessageFromServer Event Arguments
    /// </summary>

    [
        ClassInterface(ClassInterfaceType.None),
    ]
    public class GetMessageFromServerEventArgs : EventArgs, IGetMessageFromServerEventArgs
    {
        #region Constructors

        public GetMessageFromServerEventArgs(string szMsg, CommandId cmdId)
        {
            this.Message = szMsg;
            this.CmdId = cmdId;
        }

        #endregion Constructors

        #region Properties

        public CommandId CmdId { get; }
        public string Message { get; }

        #endregion Properties
    }
}

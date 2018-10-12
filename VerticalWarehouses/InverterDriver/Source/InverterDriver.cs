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

        /// <summary>
        /// Consts
        /// </summary>
        public const string IP_ADDR_INVERTER_DEFAULT = "172.16.199.200";

        public const int PORT_ADDR_INVERTER_DEFAULT = 8000;

        public const int SIZEMAX_DATABUFFER = 1024;

        /// <summary>
        /// Maximum size of data buffer for command
        /// </summary>
        public const int TIME_OUT = 150;

        private static readonly object g_lock = new object();

        /// <summary>
        /// Time out for main thread (ms)
        /// </summary>
        private readonly InverterDriverState state;

        /// <summary>
        /// Port address of inverter (manifactured port address)
        /// </summary>
        private byte[] DataBufferCommand;

        /// <summary>
        /// IP address of inverter (manifactured IP address)
        /// </summary>
        private InverterDriverErrors error;

        /// <summary>
        ///  Data buffer containing the operation arguments
        /// </summary>
        private AutoResetEvent hevAckTerminate;
        private AutoResetEvent hevTerminate;
        private HardwareInverterStatus hwInverterState;

        /// <summary>
        /// Stop event for automated operation
        /// Acknowledge for terminate
        /// </summary>
        private Thread opThread;

        private int portAddressToConnect;

        /// <summary>
        /// IP address to connect
        /// </summary>
        private string ipAddressToConnect;

        private int msgCounter;

        /// <summary>
        /// Address port to connect
        /// </summary>
        private IAsyncResult result;

        private Socket sckClient;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default c-tor.
        /// </summary>
        public CInverterDriver()
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

        public event GetMessageFromServerEventHandler GetMessageFromServer;

        #endregion Events

        #region Properties

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
        /// <param name="ic"></param>
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
            lock (g_lock)
            {
                this.DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }

        public InverterDriverExitStatus GetIOEmergencyState()
        {
            this.DataBufferCommand[2] = Convert.ToByte(CommandId.GetIOEmergencyState);

            lock (g_lock)
            {
                this.DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }
        public InverterDriverExitStatus GetIOState()
        {
            this.DataBufferCommand[2] = Convert.ToByte(CommandId.GetIOState);

            lock (g_lock)
            {
                this.DataBufferCommand[1] = 0x01;
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
            var iphostentry = Dns.GetHostEntry(strHostName);

            var IP = "";
            foreach (var ipaddress in iphostentry.AddressList)
            {
                
                if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
                {
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
        /// Get main status of inverter.
        /// </summary>
        /// <returns></returns>
        public InverterDriverExitStatus GetMainState()
        {
            this.DataBufferCommand[2] = Convert.ToByte(CommandId.GetMainState);

            lock (g_lock)
            {
                this.DataBufferCommand[1] = 0x01;
            }

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Initialize the driver.
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
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
            this.DataBufferCommand[2] = Convert.ToByte(CommandId.MoveAlongHorizontalAxisWithProfile);

            var convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(v1);
            convertionBuffer.CopyTo(this.DataBufferCommand, 3);

            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a);
            convertionBuffer.CopyTo(this.DataBufferCommand, 7);

            convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(s1);
            convertionBuffer.CopyTo(this.DataBufferCommand, 11);

            convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(s2);
            convertionBuffer.CopyTo(this.DataBufferCommand, 13);

            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(v2);
            convertionBuffer.CopyTo(this.DataBufferCommand, 15);

            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a1);
            convertionBuffer.CopyTo(this.DataBufferCommand, 19);

            convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(s3);
            convertionBuffer.CopyTo(this.DataBufferCommand, 23);
 
            convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(s4);
            convertionBuffer.CopyTo(this.DataBufferCommand, 25);
  
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(v3);
            convertionBuffer.CopyTo(this.DataBufferCommand, 27);
   
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a2);
            convertionBuffer.CopyTo(this.DataBufferCommand, 31);

            convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(s5);
            convertionBuffer.CopyTo(this.DataBufferCommand, 35);

            convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(s6);
            convertionBuffer.CopyTo(this.DataBufferCommand, 37);
  
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a3);
            convertionBuffer.CopyTo(this.DataBufferCommand, 39);

            convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(s7);
            convertionBuffer.CopyTo(this.DataBufferCommand, 43);

            lock (g_lock)
            {
                this.DataBufferCommand[1] = 0x01;
            }

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Move along vertical axis to given point.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="vMax"></param>
        /// <param name="a"></param>
        /// <param name="a1"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public InverterDriverExitStatus MoveAlongVerticalAxisToPoint(short x, float vMax, float a, float a1, float w)
        {
            this.DataBufferCommand[2] = Convert.ToByte(CommandId.MoveAlongVerticalAxisToPoint);

            var convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(x);
            convertionBuffer.CopyTo(this.DataBufferCommand, 3);

            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(vMax);
            convertionBuffer.CopyTo(this.DataBufferCommand, 5);

            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a);
            convertionBuffer.CopyTo(this.DataBufferCommand, 9);

            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a1);
            convertionBuffer.CopyTo(this.DataBufferCommand, 13);

            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(w);
            convertionBuffer.CopyTo(this.DataBufferCommand, 17);

            lock (g_lock)
            {
                this.DataBufferCommand[1] = 0x01;
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
                var nBytes = theSockId.dataBuffer[0];
                var cmdId = theSockId.dataBuffer[1];
                var state = theSockId.dataBuffer[2];
                var Message = (state == 0x00) ? "Failed" : "Success";
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
                            this.hwInverterState = (0x01 == theSockId.dataBuffer[2])
                                ? HardwareInverterStatus.Operative
                                : HardwareInverterStatus.NotOperative;

                            Message = this.hwInverterState.ToString();
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

                    default:
                        break;
                }

                var result = theSockId.dataBuffer[2];

                this.msgCounter++;

                if (null != GetMessageFromServer)
                {
                    GetMessageFromServer(this, new GetMessageFromServerEventArgs(Message, id));
                }

                this.waitForData();
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException)
            {
               
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
            this.DataBufferCommand[2] = Convert.ToByte(CommandId.RunDrawerWeightRoutine);

            var convertionBuffer = new byte[sizeof(short)];
            convertionBuffer = BitConverter.GetBytes(d);
            convertionBuffer.CopyTo(this.DataBufferCommand, 3);
  
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(w);
            convertionBuffer.CopyTo(this.DataBufferCommand, 5);

            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a);
            convertionBuffer.CopyTo(this.DataBufferCommand, 9);

            this.DataBufferCommand[13] = e;

            lock (g_lock)
            {
                this.DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Run shutter on opening movement or closing movement.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public InverterDriverExitStatus RunShutter(byte m)
        {

            this.DataBufferCommand[2] = Convert.ToByte(CommandId.RunShutter);

            lock (g_lock)
            {
                this.DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Select movement among vertical movement and horizontal movement.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public InverterDriverExitStatus SelectMovement(byte m)
        {
            this.DataBufferCommand[3] = m;

            this.DataBufferCommand[2] = Convert.ToByte(CommandId.SelectMovement);

            lock (g_lock)
            {
                this.DataBufferCommand[1] = 0x01;
            }

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Set ON/OFF value to the given line.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public InverterDriverExitStatus Set(int i, byte value)
        {
            this.DataBufferCommand[2] = Convert.ToByte(CommandId.Set);

            var convertionBuffer = new byte[sizeof(int)];
            convertionBuffer = BitConverter.GetBytes(i);
            convertionBuffer.CopyTo(this.DataBufferCommand, 3);

            this.DataBufferCommand[7] = value;

            lock (g_lock)
            {
                this.DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Set vertical axis origin routine.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="vSearch"></param>
        /// <param name="vCam0"></param>
        /// <param name="a"></param>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        public InverterDriverExitStatus SetVerticalAxisOrigin(byte direction, float vSearch, float vCam0, float a,
            float a1, float a2)
        {
            this.DataBufferCommand[2] = Convert.ToByte(CommandId.SetVerticalAxisOrigin);

            this.DataBufferCommand[3] = direction;

            var convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(vSearch);
            convertionBuffer.CopyTo(this.DataBufferCommand, 4);

            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(vCam0);
            convertionBuffer.CopyTo(this.DataBufferCommand, 8);

            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a);
            convertionBuffer.CopyTo(this.DataBufferCommand, 12);
  
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a1);
            convertionBuffer.CopyTo(this.DataBufferCommand, 16);
  
            convertionBuffer = new byte[sizeof(float)];
            convertionBuffer = BitConverter.GetBytes(a2);
            convertionBuffer.CopyTo(this.DataBufferCommand, 20);

            lock (g_lock)
            {
                this.DataBufferCommand[1] = 0x01;
            }

            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Stop.
        /// </summary>
        /// <returns></returns>
        public InverterDriverExitStatus Stop()
        {
            this.DataBufferCommand[2] = Convert.ToByte(CommandId.Stop);

            lock (g_lock)
            {
                this.DataBufferCommand[1] = 0x01;
            }
            return InverterDriverExitStatus.Success;
        }

        /// <summary>
        /// Terminate and release driver' resources.
        /// </summary>
        public void Terminate()
        {
            this.hevTerminate.Set();
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
 
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceFrequency(out long lpPerformanceFrequency);

        private bool build_telegram_and_send()
        {
            var cmdId = this.byte2CommandId(this.DataBufferCommand[2]);
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

                default:
                    break;
            }


            var byTelegramToSend = new byte[nBytes + 1];
            byTelegramToSend[0] = nBytes;
            Array.Copy(this.DataBufferCommand, 2, byTelegramToSend, 1, nBytes);

            this.sendDataToServer(byTelegramToSend);

            lock (g_lock)
            {
                this.DataBufferCommand[1] = 0x00;
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

                default:
                    break;
            }

            return cmdId;
        }

        private bool connect_to_inverter()
        {
            this.error = InverterDriverErrors.NoError;

            if (this.ipAddressToConnect == "" || this.portAddressToConnect <= 0)
            {
                this.error = InverterDriverErrors.IOError;
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
            }

            return true;
        }

        /// <summary>
        /// Create working thread.
        /// </summary>
        private void createThread()
        {
            this.hevTerminate = new AutoResetEvent(false);
            this.hevAckTerminate = new AutoResetEvent(false);
            this.opThread = new Thread(this.mainThread);
            this.opThread.Name = "workingInverterThread";
            this.opThread.Start();
        }

        /// <summary>
        /// Release resource of working thread.
        /// </summary>
        private void destroyThread()
        {
            var handles = new WaitHandle[1];
            handles[0] = this.hevAckTerminate;
            WaitHandle.WaitAny(handles, -1);

            if (null != this.opThread)
            {
                this.opThread.Abort();
            }

            if (null != this.hevTerminate)
            {
                this.hevTerminate.Close();
            }

            this.hevTerminate = null;

            if (null != this.hevAckTerminate)
            {
                this.hevAckTerminate.Close();
            }

            this.hevAckTerminate = null;
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
            var ExistsPendingOperation = false;
            lock (g_lock)
            {
                ExistsPendingOperation = (this.DataBufferCommand[1] == 0x01);
            }

            return ExistsPendingOperation;
        }

        private bool get_main_state_of_inverter()
        {
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
            handles[0] = this.hevTerminate;

            var bExit = false;
            while (!bExit)
            {
                var code = WaitHandle.WaitAny(handles, TIME_OUT);
                switch (code)
                {
                    case EV_TERMINATE:
                        {
                            bExit = true;
                            break;
                        }
                    case WaitHandle.WaitTimeout:
                        {
                            if (this.existPendingOperation())
                            {
                                this.build_telegram_and_send();
                            }
                            else
                            {
                        
                                this.get_main_state_of_inverter();
                            }

                            break;
                        }
                    default:
                        break;
                }
            }

            this.hevAckTerminate.Set();
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
                        this.sckClient.Send(byTelegramToSend);
                    }
                }
            }
            catch (SocketException)
            {
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

        public GetMessageFromServerEventArgs(string Msg, CommandId cmdId)
        {
            this.Message = Msg;
            this.CmdId = cmdId;
        }

        #endregion Constructors

        #region Properties

        public CommandId CmdId { get; }
        public string Message { get; }

        #endregion Properties
    }
}

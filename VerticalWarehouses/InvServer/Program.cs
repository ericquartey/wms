using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using NLog;

namespace Ferretto.VW.InvServer
{
    /// <summary>
    /// Parameter ID codes. The parameter are used in the Inverter driver.
    /// Constants enumerative
    /// </summary>
    public enum ParameterID
    {
        CONTROL_WORD_PARAM = 410,

        HOMING_CREEP_SPEED_PARAM = 1133,

        HOMING_FAST_SPEED_PARAM = 1132,

        HOMING_MODE_PARAM = 1130,

        HOMING_OFFSET_PARAM = 1131,

        POSITION_ACCELERATION_PARAM = 1457,

        POSITION_DECELERATION_PARAM = 1458,

        POSITION_TARGET_POSITION_PARAM = 1455,

        POSITION_TARGET_SPEED_PARAM = 1456,

        SET_OPERATING_MODE_PARAM = 1454,

        STATUS_WORD_PARAM = 411
    }

    public class Program
    {
        #region Fields

        public const int DEFAULT_PORT = 17221;              // Default port address

        public const int N_BITS_16 = 16;                    // Number of lines for status word

        public const int N_BITS_8 = 8;                      // Number of bits for a byte

        public const int NBYTES_ERROR_ENQUIRY_TELEGRAM = 6; // Size of enquiry telegram for error

        public const int NMAX_CLIENTS = 1;                  // Number of concurrent clients in the TCP/IP architecture

        // Resource synchronization
        private static readonly object lockObj = new object();

        // Logger
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private int controlWord;

        private int diffTime = 0;

        private bool errorCondition;

        private bool operativeStatus;

        private IPEndPoint remoteIpEndPoint;

        //!< Server socket
        private Socket sckMain;

        private Socket sckWorker;

        // Status word (internal)
        private BitArray statusWord;

        #endregion Fields

        #region Constructors

        /// </summary>
        public Program()
        {
            this.sckMain = null;
            this.sckWorker = null;
            this.statusWord = new BitArray(N_BITS_16);
            this.operativeStatus = false;

            // Status e Control Word initialization
            this.controlWord = 0;
        }

        #endregion Constructors

        #region Delegates

        // Client connection delegate
        public delegate void ConnClientEventHandler();

        // Hardware cable unplug delegate
        public delegate void DisconnectedClientEventHandler(string StopListen, string StartLitener);

        // Client disconnection delegate
        public delegate void DisconnectedSocketsEventHandler(string StopListen);

        // Message To Send to client delegate
        public delegate void MessageEventHandler(ParameterID param);

        #endregion Delegates

        #region Events

        public event DisconnectedClientEventHandler DiscClient;

        public event DisconnectedSocketsEventHandler DiscSockets;

        public event ConnClientEventHandler SendClientEvent;

        public event MessageEventHandler ThrowEvent;

        #endregion Events

        #region Properties

        /// <summary>
        /// Get/set an error condition.
        /// </summary>
        public bool SetErrorCondition
        {
            get => this.errorCondition;
            set
            {
                this.errorCondition = value;
                this.sendErrorToInverter();
            }
        }

        /// <summary>
        /// Gets/sets the operative status of SW emulator inverter.
        /// </summary>
        public bool StateOperativeInverter
        {
            get => this.operativeStatus;
            set => this.operativeStatus = value;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the IP address.
        /// </summary>
        public void ClientConn(out string IPAddressClient, out string PortClient)
        {
            logger.Log(LogLevel.Debug, string.Format("Client connection"));

            IPAddressClient = this.remoteIpEndPoint.Address.ToString();
            PortClient = this.remoteIpEndPoint.Port.ToString();
        }

        /// <summary>
        /// Function to retrieve the IP address (IPv4 network type) of local host.
        /// </summary>
        public string GetIPAddress()
        {
            var szHostName = Dns.GetHostName();

            // Find host by name
            var iphostentry = Dns.GetHostEntry(szHostName);

            // Grab the first IP addresses
            var szIP = "";
            foreach (var ipaddress in iphostentry.AddressList)
            {
                // address IPv4
                if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    szIP = ipaddress.ToString();
                    return szIP;
                }
            }

            return szIP;
        }

        /// <summary>
        /// Send a given byte array to client.
        /// </summary>
        public void sendDataToClient(byte[] Answer)
        {
            logger.Log(LogLevel.Debug, string.Format(" > sendDataToClient"));

            if (this.sckWorker != null)
            {
                try
                {
                    if (this.sckWorker.Connected)
                    {
                        lock (lockObj)
                        {
                            this.sckWorker.Send(Answer);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    logger.Log(LogLevel.Debug, string.Format("Socket Exception Message: {0}", ex.Message));
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Debug, string.Format("Exception Message: {0}", ex.Message));
                }
            }
        }

        /// <summary>
        /// Set a given bit of status word.
        /// </summary>
        public void SetStateLineInverter(int index, bool value)
        {
            if (index < 0 || index >= N_BITS_16)
                return;
            this.statusWord[index] = value;
        }

        public string StartListen()
        {
            var sListening = "ATTIVO";
            logger.Log(LogLevel.Debug, string.Format("Start listening..."));

            try
            {
                var port = DEFAULT_PORT;
#if NET4
                // Creates one SocketPermission object for access restrictions
                var permission = new SocketPermission(
                    NetworkAccess.Accept,     // Allowed to accept connections
                    TransportType.Tcp,        // Defines transport types
                    "",                       // The IP addresses of local host
                    SocketPermission.AllPorts // Specifies all ports
                );

                // Ensures the code to have permission to access a Socket
                permission.Demand();
#endif
                // Resolves a host name to an IPHostEntry instance
                var ipHost = Dns.GetHostEntry("");
                logger.Log(LogLevel.Debug, string.Format("ipHost = {0}", ipHost.ToString()));

                // Gets first IP address associated with a localhost (IPv6)
                IPAddress ipAddr = null;
                foreach (var ipaddress in ipHost.AddressList)
                {
                    // IPv4
                    if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddr = ipaddress;
                        logger.Log(LogLevel.Debug, string.Format(" ipAddr = {0}", ipaddress.ToString()));
                    }
                }

                // Creates a network endpoint
                var ipLocal = new IPEndPoint(ipAddr, port);

                logger.Log(LogLevel.Debug, string.Format("ipLocal = {0}", ipLocal.ToString()));

                // Create the listening socket (main)
                this.sckMain = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Bind to local IP Address
                this.sckMain.Bind(ipLocal);
                // Start listening
                this.sckMain.Listen(NMAX_CLIENTS);
                // Create the call back for any client connections...
                this.sckMain.BeginAccept(new AsyncCallback(this.onClientConnect), null);

                logger.Log(LogLevel.Debug, string.Format("Socket started successfully"));
            }
            catch (SocketException ex)
            {
                sListening = "ERROR";
                logger.Log(LogLevel.Debug, string.Format("Socket Exception Message: {0}", ex.Message));
            }
            catch (IOException ex)
            {
                sListening = "ERROR";
                logger.Log(LogLevel.Debug, string.Format("IO Exception Message: {0}", ex.Message));
            }
            catch (Exception ex)
            {
                sListening = "ERROR";
                logger.Log(LogLevel.Debug, string.Format("Exception Message: {0}", ex.Message));
            }

            return sListening;
        }

        /// <summary>
        /// Stop listen.
        /// Close the connection to all sockets.
        /// </summary>
        public string StopListen()
        {
            logger.Log(LogLevel.Debug, string.Format("Stop listening..."));

            var cSocket = this.closeSockets();
            return cSocket;
        }

        private static byte[] BitArrayToByteArray(BitArray bits)
        {
            var ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        /// <summary>
        /// Close sockets.
        /// </summary>
        private string closeSockets()
        {
            logger.Log(LogLevel.Debug, string.Format("Closing socket..."));
            var cSocket = "STOP";

            try
            {
                this.sckMain?.Close();
                this.sckWorker?.Close();
                this.sckWorker = null;
            }
            catch (SocketException ex)
            {
                cSocket = "ERRORE";
                logger.Log(LogLevel.Debug, string.Format("Socket Exception Message: {0}", ex.Message));
            }
            catch (Exception ex)
            {
                cSocket = "ERRORE";
                logger.Log(LogLevel.Debug, string.Format("Exception Message: {0}", ex.Message));
            }

            return cSocket;
        }

        /// <summary>
        /// On socket client connected callback.
        /// </summary>
        private void onClientConnect(IAsyncResult asyn)
        {
            logger.Log(LogLevel.Debug, string.Format(" --> onClientConnect"));

            try
            {
                // Here we complete/end the BeginAccept() asynchronous call by calling EndAccept()
                // - which returns the reference to a new Socket object
                this.sckWorker = this.sckMain.EndAccept(asyn);

                // -------------------------------------
                var size = sizeof(uint);
                uint on = 1;
                uint keepAliveInterval = 10000;
                uint retryInterval = 1000;
                var inArray = new byte[3 * size];
                Array.Copy(BitConverter.GetBytes(on), 0, inArray, 0, size);
                Array.Copy(BitConverter.GetBytes(keepAliveInterval), 0, inArray, size, size);
                Array.Copy(BitConverter.GetBytes(retryInterval), 0, inArray, size * 2, size);
                this.sckWorker.IOControl(IOControlCode.KeepAliveValues, inArray, null);
                // -------------------------------------

                this.waitForData(this.sckWorker);

                // Write the client connection as a status message on the Log
                this.remoteIpEndPoint = this.sckWorker.RemoteEndPoint as IPEndPoint;
                logger.Log(LogLevel.Debug, string.Format("Client connected [{0}, {1}]", this.remoteIpEndPoint.Address.ToString(), this.remoteIpEndPoint.Port.ToString()));

                // Fire event to notify Connection is occurred
                SendClientEvent();
            }
            catch (SocketException ex)
            {
                logger.Log(LogLevel.Debug, string.Format("Socket Exception Message: {0}", ex.Message));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Debug, string.Format("Exception Message: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Call back function which will be invoked when the socket detects any client writing of data on the stream.
        /// </summary>
        /// <param name="asyn">The status of asynchronous operation.</param>
        private void onDataReceived(IAsyncResult asyn)
        {
            try
            {
                lock (lockObj)
                {
                    var startTime = DateTime.Now.Millisecond;
                    var socketData = (SocketPacket)asyn.AsyncState;

                    // Retrieve the number of bytes for incoming message
                    var iRx = socketData.m_currentSocket.EndReceive(asyn);

                    // Cache the incoming data in main data buffer stream for message
                    var msgToParse = new byte[1024];
                    Array.Copy(socketData.dataBuffer, 0, msgToParse, 0, socketData.dataBuffer.Length);

                    // Parse the incoming telegram and send a response to Inverter driver
                    this.parseTelegramAndSendResponse(msgToParse, iRx);

                    this.diffTime = startTime - DateTime.Now.Millisecond;

                    // Continue the waiting for data on the Socket
                    this.waitForData(this.sckWorker);
                }
            }
            catch (SocketException ex)
            {
                // Il codice di errore 10053 è quello sollevato con la disconnessione del cavo
                if (ex.ErrorCode == 10053)
                {
                    var StopListen = this.StopListen();
                    logger.Log(LogLevel.Debug, string.Format("Connection lost"));
                    DiscSockets?.Invoke(StopListen);
                }

                logger.Log(LogLevel.Debug, string.Format("Socket Exception Message: {0}", ex.Message));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Debug, string.Format("Exception Message: {0}", ex.Message));
            }
        }

        private void parseTelegramAndSendResponse(byte[] telegramReceived, int nBytes)
        {
            // extract header
            var header = telegramReceived[0];

            // check bit
            var t = new BitArray(new byte[] { header });
            var bits = new bool[N_BITS_8];
            t.CopyTo(bits, 0);
            var bIsSettingRequest = bits[7];

            // extract number of relevant bytes (see the Bonfiglioli documentation)
            var noRelevantBytes = telegramReceived[1];

            var systemIndex = telegramReceived[2];

            var dataSetIndex = telegramReceived[3];

            // parameter No
            var parameterNo = new byte[2];
            Array.Copy(telegramReceived, 4, parameterNo, 0, 2);
            parameterNo.Reverse();
            var paramId = BitConverter.ToInt16(parameterNo, 0);

            byte[] telegramToSend = null;
            if (bIsSettingRequest)
            {
                // create an echo of received telegram
                telegramToSend = new byte[nBytes];
                Array.Copy(telegramReceived, 0, telegramToSend, 0, nBytes);

                if ((ParameterID)paramId == ParameterID.CONTROL_WORD_PARAM)
                {
                    this.controlWord = telegramReceived[6];
                }
            }
            else
            {
                var nBytesPayload = 0;
                switch ((ParameterID)paramId)
                {
                    case ParameterID.CONTROL_WORD_PARAM: nBytesPayload = 2; break;
                    case ParameterID.HOMING_CREEP_SPEED_PARAM: nBytesPayload = 4; break;
                    case ParameterID.HOMING_FAST_SPEED_PARAM: nBytesPayload = 4; break;
                    case ParameterID.HOMING_MODE_PARAM: nBytesPayload = 2; break;
                    case ParameterID.HOMING_OFFSET_PARAM: nBytesPayload = 2; break;
                    case ParameterID.POSITION_ACCELERATION_PARAM: nBytesPayload = 4; break;
                    case ParameterID.POSITION_DECELERATION_PARAM: nBytesPayload = 4; break;
                    case ParameterID.POSITION_TARGET_POSITION_PARAM: nBytesPayload = /*2*/4; break;
                    case ParameterID.POSITION_TARGET_SPEED_PARAM: nBytesPayload = /*2*/4; break;
                    case ParameterID.SET_OPERATING_MODE_PARAM: nBytesPayload = 2; break;
                    case ParameterID.STATUS_WORD_PARAM: nBytesPayload = 2; break;
                    default: nBytesPayload = 2; break;
                }

                // create a response for received telegram
                telegramToSend = new byte[nBytes + nBytesPayload];

                telegramToSend[0] = header;
                telegramToSend[1] = Convert.ToByte(nBytes + nBytesPayload - 2); // see documentation for telegram's Bonfiglioli
                telegramToSend[2] = telegramReceived[2];
                telegramToSend[3] = telegramReceived[3];

                // parameter No
                Array.Copy(parameterNo, 0, telegramToSend, 4, 2);

                // parameter Value
                switch ((ParameterID)paramId)
                {
                    case ParameterID.CONTROL_WORD_PARAM:
                        {
                            var ans = new byte[2];
                            var valueBytes = new byte[sizeof(short)];
                            valueBytes = BitConverter.GetBytes(this.controlWord); //Convert.ToInt16(value));
                            valueBytes.CopyTo(ans, 0);
                            Array.Copy(ans, 0, telegramToSend, 6, 2);

                            break;
                        }
                    case ParameterID.HOMING_CREEP_SPEED_PARAM:
                        {
                            var value = 0x0EE01001;
                            var ans = new byte[4];
                            var valueBytes = new byte[sizeof(int)];
                            valueBytes = BitConverter.GetBytes(Convert.ToInt32(value));
                            valueBytes.CopyTo(ans, 0);
                            Array.Copy(ans, 0, telegramToSend, 6, 4);
                            break;
                        }
                    case ParameterID.HOMING_FAST_SPEED_PARAM:
                        {
                            var value = 0x00001222;
                            var ans = new byte[4];
                            var valueBytes = new byte[sizeof(int)];
                            valueBytes = BitConverter.GetBytes(Convert.ToInt32(value));
                            valueBytes.CopyTo(ans, 0);
                            Array.Copy(ans, 0, telegramToSend, 6, 4);
                            break;
                        }
                    case ParameterID.HOMING_MODE_PARAM:
                        {
                            var value = 0x0006;
                            var ans = new byte[2];
                            var valueBytes = new byte[sizeof(short)];
                            valueBytes = BitConverter.GetBytes(Convert.ToInt16(value));
                            valueBytes.CopyTo(ans, 0);
                            Array.Copy(ans, 0, telegramToSend, 6, 2);
                            break;
                        }
                    case ParameterID.HOMING_OFFSET_PARAM:
                        {
                            var value = 0x0011;
                            var ans = new byte[2];
                            var valueBytes = new byte[sizeof(short)];
                            valueBytes = BitConverter.GetBytes(Convert.ToInt16(value));
                            valueBytes.CopyTo(ans, 0);
                            Array.Copy(ans, 0, telegramToSend, 6, 2);
                            break;
                        }
                    case ParameterID.POSITION_ACCELERATION_PARAM:
                        {
                            var value = 0x00008500;
                            var ans = new byte[4];
                            var valueBytes = new byte[sizeof(int)];
                            valueBytes = BitConverter.GetBytes(Convert.ToInt32(value));
                            valueBytes.CopyTo(ans, 0);
                            Array.Copy(ans, 0, telegramToSend, 6, 4);
                            break;
                        }
                    case ParameterID.POSITION_DECELERATION_PARAM:
                        {
                            var value = 0x00000033;
                            var ans = new byte[4];
                            var valueBytes = new byte[sizeof(int)];
                            valueBytes = BitConverter.GetBytes(Convert.ToInt32(value));
                            valueBytes.CopyTo(ans, 0);
                            Array.Copy(ans, 0, telegramToSend, 6, 4);
                            break;
                        }
                    case ParameterID.POSITION_TARGET_POSITION_PARAM:
                        {
                            var value = 0x12340000;
                            var ans = new byte[/*2*/4];
                            var valueBytes = new byte[sizeof(/*short*/int)];
                            valueBytes = BitConverter.GetBytes(/*Convert.ToInt16(value)*/Convert.ToInt32(value));
                            valueBytes.CopyTo(ans, 0);
                            Array.Copy(ans, 0, telegramToSend, 6, /*2*/4);
                            break;
                        }
                    case ParameterID.POSITION_TARGET_SPEED_PARAM:
                        {
                            var value = 0x03450011;
                            var ans = new byte[/*2*/4];
                            var valueBytes = new byte[sizeof(/*short*/int)];
                            valueBytes = BitConverter.GetBytes(/*Convert.ToInt16(value)*/Convert.ToInt32(value));
                            valueBytes.CopyTo(ans, 0);
                            Array.Copy(ans, 0, telegramToSend, 6, /*2*/4);
                            break;
                        }
                    case ParameterID.SET_OPERATING_MODE_PARAM:
                        {
                            var value = 0x0005;
                            var ans = new byte[2];
                            var valueBytes = new byte[sizeof(short)];
                            valueBytes = BitConverter.GetBytes(Convert.ToInt16(value));
                            valueBytes.CopyTo(ans, 0);
                            Array.Copy(ans, 0, telegramToSend, 6, 2);
                            break;
                        }
                    case ParameterID.STATUS_WORD_PARAM:
                        {
                            this.statusWord.SetAll(false);

                            switch (this.controlWord)
                            {
                                // 3.1
                                case (4):// Control Word: 00000000 00000100
                                    this.statusWord.Set(4, true);

                                    break;
                                // 3.2
                                case (6):// Control Word: 00000000 00000110
                                    this.statusWord.Set(5, true);
                                    this.statusWord.Set(4, true);
                                    this.statusWord.Set(0, true);

                                    break;
                                // 3.3
                                case (7): // Control Word: 00000000 00000111
                                    this.statusWord.Set(5, true);
                                    this.statusWord.Set(4, true);
                                    this.statusWord.Set(1, true);

                                    break;
                                // 3.4
                                case (15): // Control Word: 00000000 00001111
                                    this.statusWord.Set(5, true);
                                    this.statusWord.Set(4, true);
                                    this.statusWord.Set(2, true);
                                    this.statusWord.Set(1, true);

                                    break;
                                // 3.5
                                case (31): // Control Word: 00000000 00011111
                                    this.statusWord.Set(12, true);
                                    this.statusWord.Set(5, true);
                                    this.statusWord.Set(4, true);
                                    this.statusWord.Set(2, true);
                                    this.statusWord.Set(1, true);

                                    break;

                                default:

                                    break;
                            }

                            var ans = BitArrayToByteArray(this.statusWord);
                            Array.Copy(ans, 0, telegramToSend, 6, 2);
                            break;
                        }
                    default: nBytesPayload = 2; break;
                }
            }

            // Fire event to notify the send response
            ThrowEvent?.Invoke(((ParameterID)paramId));

            // emulate the inverter response timing
            System.Threading.Thread.Sleep(18);

            // send telegram to client
            this.sendDataToClient(telegramToSend);
        }

        private void sendErrorToInverter()
        {
            // send an enquiry telegram for the parameter Status_Word
            var telegramToSend = new byte[NBYTES_ERROR_ENQUIRY_TELEGRAM];

            telegramToSend[0] = 0x00;
            telegramToSend[1] = Convert.ToByte(NBYTES_ERROR_ENQUIRY_TELEGRAM - 2); // see documentation for telegram's Bonfiglioli
            telegramToSend[2] = 0x00;
            telegramToSend[3] = 0x05;

            // parameter No
            var ans = new byte[2];
            var parameterNo = new byte[sizeof(short)];
            parameterNo = BitConverter.GetBytes(Convert.ToInt16(ParameterID.STATUS_WORD_PARAM));
            parameterNo.CopyTo(ans, 0);

            Array.Copy(ans, 0, telegramToSend, 4, 2);

            // parameter Value
            var errorBits = new BitArray(N_BITS_16);
            for (var i = 0; i < N_BITS_16; i++)
            {
                errorBits[i] = true;  // 0xFFFF
            }
            ans = BitArrayToByteArray(errorBits);
            Array.Copy(ans, 0, telegramToSend, 6, 2);

            // Send telegram to client
            this.sendDataToClient(telegramToSend);
        }

        /// <summary>
        /// Wait the incoming data from socket.
        /// </summary>
        private void waitForData(System.Net.Sockets.Socket sckt)
        {
            try
            {
                var theSocPkt = new SocketPacket();
                theSocPkt.m_currentSocket = sckt;

                // Start receiving any data written by the connected client asynchronously
                sckt.BeginReceive(
                    theSocPkt.dataBuffer,
                    0,
                    theSocPkt.dataBuffer.Length,
                    SocketFlags.None,
                    new AsyncCallback(this.onDataReceived),
                    theSocPkt
                );
            }
            catch (SocketException ex)
            {
                // Error code to notify the lost connection (hardware cable unplug) to server
                if (ex.ErrorCode == 10054)
                {
                    var StopListen = this.StopListen();
                    var StartLitener = this.StartListen();
                    logger.Log(LogLevel.Debug, string.Format("Connection to client is lost"));
                    DiscClient?.Invoke(StopListen, StartLitener);
                }

                logger.Log(LogLevel.Debug, string.Format("Socket Exception Message: {0}", ex.Message));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Debug, string.Format("Exception Message: {0}", ex.Message));
            }
        }

#endregion Methods
    }
}

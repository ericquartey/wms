using System;
using System.Collections.Generic;
using System.IO;

using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Ferretto.VW.InvServer
{
    public class Program
    {
        #region Fields

        public AsyncCallback pfnWorkerCallback;

        private FileStream f;
        public StreamWriter s;
        private const int DEFAULT_PORT = 8000;

        // Resource synchronization
        private static object g_lock = new object();

        // Rivedere dove salva il Log
        private readonly string LOG_PATH;

        private int GetMainStateCounter;

        private Socket m_sckMain;

        //!< Server socket
        private Socket m_sckWorker = null;

        int DiffTime = 0;

        private IPEndPoint remoteIpEndPoint;

        private bool m_state;

        #endregion Fields

        #region Constructors

        /// </summary>
        public Program()
        {
            this.GetMainStateCounter = 0;
            this.m_sckMain = null;

            // Log path and file creation
            this.LOG_PATH = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "\\Logs\\";

            var exists = System.IO.Directory.Exists(this.LOG_PATH);

            if (!exists)
            {
                System.IO.Directory.CreateDirectory(this.LOG_PATH);
            }
        }

        #endregion Constructors

        #region Delegates

        // Delegate per segnalare la connessione di un client
        public delegate void ConnClientHandler();

        // Delegate per segnalare la disconnessione del cavo
        public delegate void DisconnectedClientEventHandler(string StopListen, string StartLitener);
        public delegate void DisconnectedSocketsEventHandler(string StopListen);

        // Delegate per segnalare l'arrivo di un nuovo messaggio
        public delegate void MessageEventHandler(InverterCmd SingleCmd);

        #endregion Delegates

        #region Events

        public event DisconnectedClientEventHandler DiscClient;
        public event DisconnectedSocketsEventHandler DiscSockets;

        public event ConnClientHandler SendClientEvent;

        public event MessageEventHandler ThrowEvent;

        #endregion Events

        #region Enums

        /// <summary>
        /// Categories for parsed requested operation.
        /// </summary>
        private enum ParsedReqOperation
        {
            /// <summary>
            /// Get IO sensors status.
            /// </summary>
            GetIOState = 0x00,

            /// <summary>
            /// Other operation.
            /// </summary>
            Other
        }

        #endregion Enums

        #region Methods

        public void ClientConn(out string IPAddressClient, out string PortClient)
        {
            this.WriteLog("ClientConn");

            IPAddressClient = this.remoteIpEndPoint.Address.ToString();
            PortClient = this.remoteIpEndPoint.Port.ToString();
        }

        // Metodo per il recupero dell'ind. IP della macchina
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

        public bool SetStateInverter
        {
            set { this.m_state = value; }
        }

        /// <summary>
        /// Send a given string data to client.
        /// </summary>
        /// <param name="index">Index of client</param>
        /// <param name="szData">The string</param>
        public void sendDataToClient(byte[] Answer)
        {
            this.WriteLog("sendDataToClient");

            if (this.m_sckWorker != null)
            {
                try
                {
                    if (this.m_sckWorker.Connected)
                    {
                        lock (g_lock)
                        {
                            this.m_sckWorker.Send(Answer);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    this.WriteLog("Socket Exception Message: " + ex.Message);
                    this.WriteLog("Socket Exception InnerException: " + ex.InnerException);
                }
                catch (Exception ex)
                {
                    this.WriteLog("Exception Message: " + ex.Message);
                    this.WriteLog("Exception InnerException: " + ex.InnerException);
                }
            }
        }

        public string StartListen()
        {
            var sListening = "ATTIVO";

            this.WriteLog("StartListen");

            try
            {
                var port = DEFAULT_PORT;

                // Creates one SocketPermission object for access restrictions
                var permission = new SocketPermission(NetworkAccess.Accept,     // Allowed to accept connections
                                                                   TransportType.Tcp,        // Defines transport types
                                                                   "",                       // The IP addresses of local host
                                                                   SocketPermission.AllPorts // Specifies all ports
                                                                   );

                // Ensures the code to have permission to access a Socket
                permission.Demand();

                // Resolves a host name to an IPHostEntry instance
                var ipHost = Dns.GetHostEntry("");

                this.WriteLog("ipHost = " + ipHost);

                // Gets first IP address associated with a localhost (IPv6)
                IPAddress ipAddr = null;
                foreach (var ipaddress in ipHost.AddressList)
                {
                    // IPv4
                    if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddr = ipaddress;
                        this.WriteLog("ipAddr = " + ipAddr);
                    }
                }

                // Creates a network endpoint
                var ipLocal = new IPEndPoint(ipAddr, port);

                this.WriteLog("ipLocal = " + ipLocal);

                // Create the listening socket (main)
                this.m_sckMain = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Bind to local IP Address
                // Associa un Socket ad un End-Point locale
                this.m_sckMain.Bind(ipLocal);
                // Start listening
                this.m_sckMain.Listen(1);
                // Create the call back for any client connections...
                this.m_sckMain.BeginAccept(new AsyncCallback(this.onClientConnect), null);

                this.WriteLog("Socket avviato con successo");
            }
            catch (SocketException ex)
            {
                sListening = "ERROR";

                this.WriteLog("Socket Exception Message: " + ex.Message);
                this.WriteLog("Socket Exception InnerException: " + ex.InnerException);
            }
            catch (IOException ex)
            {
                sListening = "ERROR";

                this.WriteLog("IO Exception Message: " + ex.Message);
                this.WriteLog("IO Exception InnerException: " + ex.InnerException);
            }
            catch (Exception ex)
            {
                sListening = "ERROR";

                this.WriteLog("Exception Message: " + ex.Message);
                this.WriteLog("Exception InnerException: " + ex.InnerException);
            }

            return sListening;
        }

        /// <summary>
        /// Stop listen.
        /// Close the connection to all sockets.
        /// </summary>
        public string StopListen()
        {
            this.WriteLog("StopListen");

            var cSocket = this.closeSockets();

            return cSocket;
        }

        // C'è solo CloseSocket per chiudere il Socket?
        /// <summary>
        /// Close sockets.
        /// </summary>
        private string closeSockets()
        {
            this.WriteLog("closeSockets");

            var cSocket = "STOP";

            try
            {
                // main (server)
                if (this.m_sckMain != null)
                {
                    this.m_sckMain.Close();
                }
                if (this.m_sckWorker != null)
                {
                    this.m_sckWorker.Close();
                    this.m_sckWorker = null;
                }
            }
            catch (SocketException ex)
            {
                cSocket = "ERRORE";

                this.WriteLog("Socket Exception Message: " + ex.Message);
                this.WriteLog("Socket Exception InnerException: " + ex.InnerException);
            }
            catch (Exception ex)
            {
                cSocket = "ERRORE";

                this.WriteLog("Exception Message: " + ex.Message);
                this.WriteLog("Exception InnerException: " + ex.InnerException);
            }

            return cSocket;
        }

        private void onClientConnect(IAsyncResult asyn)
        {
            this.WriteLog("onClientConnect");

            try
            {
                // Here we complete/end the BeginAccept() asynchronous call by calling EndAccept()
                // - which returns the reference to a new Socket object
                this.m_sckWorker = this.m_sckMain.EndAccept(asyn);

                // -------------------------------------

                var size = sizeof(UInt32);
                UInt32 on = 1;
                UInt32 keepAliveInterval = 10000;
                UInt32 retryInterval = 1000;
                var inArray = new byte[3 * size];
                Array.Copy(BitConverter.GetBytes(on), 0, inArray, 0, size);
                Array.Copy(BitConverter.GetBytes(keepAliveInterval), 0, inArray, size, size);
                Array.Copy(BitConverter.GetBytes(retryInterval), 0, inArray, size * 2, size);
                this.m_sckWorker.IOControl(IOControlCode.KeepAliveValues, inArray, null);

                // -------------------------------------

                this.waitForData(this.m_sckWorker);

                // Write the client connection as a status message on the Log
                this.remoteIpEndPoint = this.m_sckWorker.RemoteEndPoint as IPEndPoint;
                this.WriteLog("Client Address connected: " + this.remoteIpEndPoint.Address.ToString());
                this.WriteLog("Client Port connected: " + this.remoteIpEndPoint.Port.ToString());

                SendClientEvent();
            }
            catch (SocketException ex)
            {
                this.WriteLog("Socket Exception Message: " + ex.Message);
                this.WriteLog("Socket Exception InnerException: " + ex.InnerException);
            }
            catch (Exception ex)
            {
                this.WriteLog("Exception Message: " + ex.Message);
                this.WriteLog("Exception InnerException: " + ex.InnerException);
            }
        }

        /// <summary>
        /// Call back function which will be invoked when the socket detects any client writing of data on the stream.
        /// </summary>
        /// <param name="asyn">The status of asynchronous operation.</param>
        private void onDataReceived(IAsyncResult asyn)
        {
            this.WriteLog("onDataReceived");

            try
            {
                lock (g_lock)
                {
                    var startTime = DateTime.Now.Millisecond;
                    var socketData = (SocketPacket)asyn.AsyncState;

                    // Cache the incoming data in main data buffer stream for message
                    // Va in errore, array troppo piccolo.

                    var msgToParse = new byte[1024];
                    Array.Copy(socketData.dataBuffer, 0, msgToParse, 0, socketData.dataBuffer.Length);

                    this.WriteLog("Operation Code: " + msgToParse[1].ToString());

                    // GetMainState immediately sends an answer to the client
                    if (msgToParse[1] == 0x09)
                    {
                        byte OpStatus;

                        if (this.m_state)
                        {
                            OpStatus = 0x01;
                        }
                        else
                        {
                            OpStatus = 0x00;
                        }

                        byte[] GetMainState = new byte[] {0x03, 0x09, OpStatus };
                        this.sendDataToClient(GetMainState);

                        this.GetMainStateCounter++;
                        this.WriteLog("N° richieste di tipo GetMainState: " + this.GetMainStateCounter.ToString());
                    }
                    else
                    {
                        InverterCmd SingleCmd = new InverterCmd();

                        // Message Length
                        SingleCmd.Lunghezza = (int)msgToParse[0];
                        // Operation Code
                        SingleCmd.CodeOp = msgToParse[1].ToString("X4");

                        switch (msgToParse[1])
                        {
                            case 0x00: // SetVerticalAxisOrigin
                                {
                                    // byte direction
                                    byte direction = msgToParse[2];
                                    // float vSearch
                                    byte[] vSearchByte = new byte[] { msgToParse[3], msgToParse[4], msgToParse[5], msgToParse[6] };
                                    float vSearch = BitConverter.ToSingle(vSearchByte, 0);
                                    // float vCam0
                                    byte[] vCam0Byte = new byte[] { msgToParse[7], msgToParse[8], msgToParse[9], msgToParse[10] };
                                    float vCam0 = BitConverter.ToSingle(vCam0Byte, 0);
                                    // float a
                                    byte[] aByte = new byte[] { msgToParse[11], msgToParse[12], msgToParse[13], msgToParse[14] };
                                    float a = BitConverter.ToSingle(aByte, 0);
                                    // float a1
                                    byte[] a1Byte = new byte[] { msgToParse[15], msgToParse[16], msgToParse[17], msgToParse[18] };
                                    float a1 = BitConverter.ToSingle(a1Byte, 0);
                                    // float a2
                                    byte[] a2Byte = new byte[] { msgToParse[19], msgToParse[20], msgToParse[21], msgToParse[22] };
                                    float a2 = BitConverter.ToSingle(a2Byte, 0);

                                    // Eseguo la conversione a string dei parametri
                                    SingleCmd.Param1 = Convert.ToString(direction, 2).PadLeft(8, '0');
                                    SingleCmd.Param2 = Convert.ToString(vSearch);
                                    SingleCmd.Param3 = Convert.ToString(vCam0);
                                    SingleCmd.Param4 = Convert.ToString(a);
                                    SingleCmd.Param5 = Convert.ToString(a1);
                                    SingleCmd.Param6 = Convert.ToString(a2);

                                    break;
                                }
                            case 0x01: // MoveAlongVerticalAxisToPoint
                                {
                                    // short x - ha dimensione 2 Byte
                                    short x = BitConverter.ToInt16(new byte[2] { msgToParse[2], msgToParse[3] }, 0);
                                    // float vMax
                                    byte[] vMaxByte = new byte[] { msgToParse[4], msgToParse[5], msgToParse[6], msgToParse[7] };
                                    float vMax = BitConverter.ToSingle(vMaxByte, 0);
                                    // float a
                                    byte[] aByte = new byte[] { msgToParse[8], msgToParse[9], msgToParse[10], msgToParse[11] };
                                    float a = BitConverter.ToSingle(aByte, 0);
                                    // float a1
                                    byte[] a1Byte = new byte[] { msgToParse[12], msgToParse[13], msgToParse[14], msgToParse[15] };
                                    float a1 = BitConverter.ToSingle(a1Byte, 0);
                                    // float w
                                    byte[] wByte = new byte[] { msgToParse[16], msgToParse[17], msgToParse[18], msgToParse[19] };
                                    float w = BitConverter.ToSingle(wByte, 0);

                                    // Eseguo la conversione a string dei parametri
                                    SingleCmd.Param1 = Convert.ToString(x);
                                    SingleCmd.Param2 = Convert.ToString(vMax);
                                    SingleCmd.Param3 = Convert.ToString(a);
                                    SingleCmd.Param4 = Convert.ToString(a1);
                                    SingleCmd.Param5 = Convert.ToString(w);

                                    break;
                                }
                            case 0x02: // SelectMovement
                            case 0x05: // RunShutter
                                {
                                    // byte m
                                    byte m = msgToParse[2];

                                    // Eseguo la conversione a string dei parametri
                                    SingleCmd.Param1 = Convert.ToString(m, 2).PadLeft(8, '0');

                                    break;
                                }
                            case 0x03: // MoveAlongHorizontalAxisWithProfile
                                {
                                    // float v1
                                    byte[] v1Byte = new byte[] { msgToParse[2], msgToParse[3], msgToParse[4], msgToParse[5] };
                                    float v1 = BitConverter.ToSingle(v1Byte, 0);
                                    // float a
                                    byte[] aByte = new byte[] { msgToParse[6], msgToParse[7], msgToParse[8], msgToParse[9] };
                                    float a = BitConverter.ToSingle(aByte, 0);
                                    // short s1
                                    short s1 = BitConverter.ToInt16(new byte[2] { msgToParse[10], msgToParse[11] }, 0);
                                    // short s2
                                    short s2 = BitConverter.ToInt16(new byte[2] { msgToParse[12], msgToParse[13] }, 0);
                                    // float v2
                                    byte[] v2Byte = new byte[] { msgToParse[14], msgToParse[15], msgToParse[16], msgToParse[17] };
                                    float v2 = BitConverter.ToSingle(v2Byte, 0);
                                    // float a1
                                    byte[] a1Byte = new byte[] { msgToParse[18], msgToParse[19], msgToParse[20], msgToParse[21] };
                                    float a1 = BitConverter.ToSingle(a1Byte, 0);
                                    // short s3
                                    short s3 = BitConverter.ToInt16(new byte[2] { msgToParse[22], msgToParse[23] }, 0);
                                    // short s4
                                    short s4 = BitConverter.ToInt16(new byte[2] { msgToParse[24], msgToParse[25] }, 0);
                                    // float v3
                                    byte[] v3Byte = new byte[] { msgToParse[26], msgToParse[27], msgToParse[28], msgToParse[29] };
                                    float v3 = BitConverter.ToSingle(v3Byte, 0);
                                    // float a2
                                    byte[] a2Byte = new byte[] { msgToParse[30], msgToParse[31], msgToParse[32], msgToParse[33] };
                                    float a2 = BitConverter.ToSingle(a2Byte, 0);
                                    // short s5
                                    short s5 = BitConverter.ToInt16(new byte[2] { msgToParse[34], msgToParse[35] }, 0);
                                    // short s6
                                    short s6 = BitConverter.ToInt16(new byte[2] { msgToParse[36], msgToParse[37] }, 0);
                                    // float a3
                                    byte[] a3Byte = new byte[] { msgToParse[38], msgToParse[39], msgToParse[40], msgToParse[41] };
                                    float a3 = BitConverter.ToSingle(a3Byte, 0);
                                    // short s7
                                    short s7 = BitConverter.ToInt16(new byte[2] { msgToParse[42], msgToParse[43] }, 0);

                                    // Eseguo la conversione a string dei parametri
                                    SingleCmd.Param1 = Convert.ToString(v1);
                                    SingleCmd.Param2 = Convert.ToString(a);
                                    SingleCmd.Param3 = Convert.ToString(s1);
                                    SingleCmd.Param4 = Convert.ToString(s2);
                                    SingleCmd.Param5 = Convert.ToString(v2);
                                    SingleCmd.Param6 = Convert.ToString(a1);
                                    SingleCmd.Param7 = Convert.ToString(s3);
                                    SingleCmd.Param8 = Convert.ToString(s4);
                                    SingleCmd.Param9 = Convert.ToString(v3);
                                    SingleCmd.Param10 = Convert.ToString(a2);
                                    SingleCmd.Param11 = Convert.ToString(s5);
                                    SingleCmd.Param12 = Convert.ToString(s6);
                                    SingleCmd.Param13 = Convert.ToString(a3);
                                    SingleCmd.Param14 = Convert.ToString(s7);

                                    break;
                                }
                            // 0x04
                            // 0x05: // RunShutter
                            case 0x06: // RunDrawerWeightRoutine
                                {
                                    // short d
                                    short d = BitConverter.ToInt16(new byte[2] { msgToParse[2], msgToParse[3] }, 0);
                                    // float w
                                    byte[] wByte = new byte[] { msgToParse[4], msgToParse[5], msgToParse[6], msgToParse[7] };
                                    float w = BitConverter.ToSingle(wByte, 0);
                                    // float a
                                    byte[] aByte = new byte[] { msgToParse[8], msgToParse[9], msgToParse[10], msgToParse[11] };
                                    float a = BitConverter.ToSingle(aByte, 0);
                                    // byte e
                                    byte e = msgToParse[12];

                                    // Eseguo la conversione a string dei parametri
                                    SingleCmd.Param1 = Convert.ToString(d);
                                    SingleCmd.Param2 = Convert.ToString(w);
                                    SingleCmd.Param3 = Convert.ToString(a);
                                    SingleCmd.Param4 = Convert.ToString(e, 2).PadLeft(8, '0');

                                    break;
                                }
                            case 0x07: // GetDrawerWeight
                                {
                                    // out float ic
                                    break;
                                }
                            case 0x08: // Stop
                                {
                                    // Nessuno
                                    break;
                                }
                            // case 0x09: // GetMainState: spostato in Program
                            case 0x0A: // GetIOState
                                {
                                    // Proprietà con get
                                    break;
                                }
                            case 0x0B: // GetIOEmergencyState
                                {
                                    // Proprietà con get
                                    break;
                                }
                            case 0x0C: // Set
                                {
                                    // int i
                                    int i = BitConverter.ToInt32(new byte[4] { msgToParse[2], msgToParse[3], msgToParse[4], msgToParse[5] }, 0);
                                    // byte value
                                    byte value = msgToParse[6];

                                    // Eseguo la conversione a string dei parametri
                                    SingleCmd.Param1 = Convert.ToString(i);
                                    SingleCmd.Param2 = Convert.ToString(value, 2).PadLeft(8, '0');

                                    break;
                                }
                            case 0xFF: // None
                                {
                                    // Non presente in IDriver.cs
                                    break;
                                }
                            default:
                                {
                                    this.WriteLog("Operation Code NOT recognized");

                                    break;
                                }
                        }

                        ThrowEvent.Invoke(SingleCmd); // Event to update the UI
                    }

                    msgToParse = null;

                    this.DiffTime = startTime - DateTime.Now.Millisecond;
                    this.WriteLog("Elaboration time: " + this.GetMainStateCounter.ToString());

                    // Continue the waiting for data on the Socket
                    this.waitForData(this.m_sckWorker);
                }
            }
            catch (SocketException ex)
            {
                // Il codice di errore 10053 è quello sollevato con la disconnessione del cavo
                if (ex.ErrorCode == 10053)
                {
                    string StopListen = this.StopListen();
                    this.WriteLog("La connessione tra il Client ed il Server è caduta!");
                    if (DiscClient != null)
                    {
                        DiscSockets.Invoke(StopListen);
                    }
                }

                this.WriteLog("Socket Exception Message: " + ex.Message);
                this.WriteLog("Socket Exception Error Code: " + ex.ErrorCode);
            }
            catch (Exception ex)
            {
                this.WriteLog("Exception Message: " + ex.Message);
            }
        }

        private void waitForData(System.Net.Sockets.Socket sckt)
        {
            this.WriteLog("waitForData");

            try
            {
                if (this.pfnWorkerCallback == null)
                {
                    // Specify the call back function which is to be
                    // invoked when there is any write activity by the
                    // connected client
                    this.pfnWorkerCallback = new AsyncCallback(this.onDataReceived);
                }

                var theSocPkt = new SocketPacket();
                theSocPkt.m_currentSocket = sckt;

                // Start receiving any data written by the connected client asynchronously
                sckt.BeginReceive(theSocPkt.dataBuffer,
                                  0,
                                  theSocPkt.dataBuffer.Length,
                                  SocketFlags.None,
                                  this.pfnWorkerCallback,
                                  theSocPkt);
            }
            catch (SocketException ex)
            {
                // Il codice di errore 10054 indica la disconnessione del client
                if (ex.ErrorCode == 10054)
                {
                    string StopListen = this.StopListen();
                    var StartLitener = this.StartListen();
                    this.WriteLog("Il Client si è disconnesso!");
                    if (DiscClient != null)
                    {
                        DiscClient.Invoke(StopListen, StartLitener);
                    }
                }

                this.WriteLog("Socket Exception Message: " + ex.Message);
                this.WriteLog("Socket Exception Error Code: " + ex.ErrorCode);
            }
            catch (Exception ex)
            {
                this.WriteLog("Exception Message: " + ex.Message);
                this.WriteLog("Exception InnerException: " + ex.InnerException);
            }
        }

        private void WriteLog(string messaggio)
        {
            lock (g_lock)
            {
                this.f = new FileStream(this.LOG_PATH + "serverLog.log", FileMode.Append);
                this.s = new StreamWriter(this.f);

                this.s.WriteLine(DateTime.Now.ToString() + " - " + messaggio);

                this.s.Close();
                this.f.Close();
            }
        }

        #endregion Methods

        #region Classes

        public class SocketPacket
        {
            #region Fields

            public byte[] dataBuffer = new byte[1024];
            public System.Net.Sockets.Socket m_currentSocket;

            #endregion Fields

            //!< Current socket
            //!< Data buffer
        }

        #endregion Classes
    }
}

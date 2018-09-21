using System;
using System.IO;

using System.Net;
using System.Net.Sockets;
using Ferretto.VW.Utils;

namespace Ferretto.VW.InvServer
{
    public class Program
    {
        // int i = 0;

        // Delegate per segnalare l'arrivo di un nuovo messaggio
        public delegate void EventHandler();
        public event EventHandler ThrowEvent;

        // Delegate per segnalare la connessione di un client
        public delegate void ConnClientHandler();
        public event ConnClientHandler SendClientEvent;

        // Delegate per segnalare la disconnessione del cavo
        public delegate void DisconnectedSockets();
        public event DisconnectedSockets DiscSockets;

        #region - Scrittura del file di log -

        private const string LOG_PATH = "C:\\Users\\mmorbelli\\Documents\\";

        #endregion

        #region - Inizializzazione del Server Socket -

        const int NMAX_CLIENTS = 1;                                      // Maximum number of client (default value)

        const int DEFAULT_PORT = 8000;                                    // Address port default value

        public AsyncCallback pfnWorkerCallback;                           //!< Reference to a callback for requesting client socket

        private Socket m_sckMain;                                         //!< Server socket
        private Socket m_sckWorker = null;                                //!< Client sockets pool

        IPEndPoint remoteIpEndPoint;                                      // IP dell'host remoto connesso

        #endregion

        private int m_msgCounter;                                       //!< Message counter for client
        // private int m_msgLengthForClient;                               //!< Message length related to a client

        // private int m_LengthOfCurrentMessage;                           //!< Length of current message to parse
        public CBufferStream[] m_bufStream;                            //!< Data buffer stream message pool

        // Resource synchronization
        private static object g_lock = new object();                            //!< Lock object for concurrency 

        #region - File di Log
        public FileStream f;
        public StreamWriter s;
        #endregion

        // Rappresenta il messaggio ricevuto
        private string mexRic = "";

        /// <summary>
        /// Default c-tor.
        /// </summary>
        public Program()
        {
            this.m_msgCounter = 0;
            // this.m_msgLengthForClient = 0;

            // this.m_LengthOfCurrentMessage = 0;

            this.m_bufStream = new CBufferStream[NMAX_CLIENTS];
            for (int ix = 0; ix < NMAX_CLIENTS; ix++)
            { 
                this.m_bufStream[ix] = new CBufferStream();
            }

            this.m_sckMain = null;
        }

        public class SocketPacket
        {
            public System.Net.Sockets.Socket m_currentSocket;           //!< Current socket
            public byte[] dataBuffer = new byte[1024];                  //!< Data buffer
        }

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

        // Metodo per il recupero dell'ind. IP della macchina
        public string GetIPAddress()
        {
            string szHostName = Dns.GetHostName();

            // Find host by name
            IPHostEntry iphostentry = Dns.GetHostEntry(szHostName);

            // Grab the first IP addresses
            string szIP = "";
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                // address IPv4
                if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    szIP = ipaddress.ToString();
                    return szIP;
                }

                // address IPv6
                if (ipaddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    // TODO: add your implementation code here
                }
            }

            return szIP;
        }

        public string StartListen()
        {
            string sListening = "ATTIVO";

            this.WriteLog("StartListen");

            try
            {
                int port = DEFAULT_PORT;

                // Creates one SocketPermission object for access restrictions
                SocketPermission permission = new SocketPermission(NetworkAccess.Accept,     // Allowed to accept connections 
                                                                   TransportType.Tcp,        // Defines transport types 
                                                                   "",                       // The IP addresses of local host 
                                                                   SocketPermission.AllPorts // Specifies all ports 
                                                                   );

                // Ensures the code to have permission to access a Socket 
                permission.Demand();

                // Resolves a host name to an IPHostEntry instance 
                IPHostEntry ipHost = Dns.GetHostEntry("");

                this.WriteLog("ipHost = " + ipHost);

                // Gets first IP address associated with a localhost (IPv6)
                IPAddress ipAddr = null;
                foreach (IPAddress ipaddress in ipHost.AddressList)
                {
                    // IPv4
                    if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddr = ipaddress;
                        this.WriteLog("ipAddr = " + ipAddr);
                    }
                }

                // Creates a network endpoint 
                IPEndPoint ipLocal = new IPEndPoint(ipAddr, port);

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

        private void onClientConnect(IAsyncResult asyn)
        {
            this.WriteLog("onClientConnect");

            try
            {
                // Here we complete/end the BeginAccept() asynchronous call by calling EndAccept()
                // - which returns the reference to a new Socket object
                this.m_sckWorker = this.m_sckMain.EndAccept(asyn);


                // -------------------------------------

                int size = sizeof(UInt32);
                UInt32 on = 1;
                UInt32 keepAliveInterval = 10000;
                UInt32 retryInterval = 1000;
                byte[] inArray = new byte[3 * size];
                Array.Copy(BitConverter.GetBytes(on), 0, inArray, 0, size);
                Array.Copy(BitConverter.GetBytes(keepAliveInterval), 0, inArray, size, size);
                Array.Copy(BitConverter.GetBytes(retryInterval), 0, inArray, size * 2, size);
                // m_sckWorker.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, 1);
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

                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.m_currentSocket = sckt;
                // Start receiving any data written by the connected client asynchronously
                sckt.BeginReceive(theSocPkt.dataBuffer, 0,
                            theSocPkt.dataBuffer.Length,
                            SocketFlags.None, this.pfnWorkerCallback, theSocPkt);
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

            int pos = 0;

            try
            {
                lock (g_lock)
                {
                    int startTime = DateTime.Now.Millisecond;
                    int diffTime = 0;

                    SocketPacket socketData = (SocketPacket)asyn.AsyncState;

                    int iRx = 0;

                    // Complete the BeginReceive() asynchronous call by EndReceive() method
                    // which will return the number of characters written to the stream 
                    // by the client
                    iRx = socketData.m_currentSocket.EndReceive(asyn);

                    // Cache the incoming data in main data buffer stream for message
                    Array.Copy(socketData.dataBuffer, 0, this.m_bufStream[0].DataBuff1, 0, iRx);

                    byte[] msgToParse = new byte[1024];

                    // Inserisco i comandi ricevuti in una coda, che verranno poi recuperati quando andrà in esecuzione.
                    // Ipotizzo che il client invii i comandi all'inverter nella sequenza corretta, uso una Queue che è FIFO.
                    // this.m_bufStream[0].Commands.Enqueue(msgToParse);

                    // foreach (byte strByte in msgToParse)
                    // {
                    //    this.WriteLog("L'elemento in posizione " + this.i + " ha valore: " + strByte);
                    //    this.i++;
                    // }

                    Array.Copy(socketData.dataBuffer, 0, msgToParse, 0, socketData.dataBuffer.Length);

                    // Parse the message in array of chars
                    char[] chars = new char[msgToParse.Length];
                    System.Text.Decoder d = System.Text.Encoding.Unicode.GetDecoder();
                    int charLen = d.GetChars(msgToParse, 0, msgToParse.Length, chars, 0);
                    string szData = new string(chars);

                    this.m_msgCounter++;
                    this.mexRic = szData;
                    pos = this.mexRic.IndexOf("<Client Quit>");
                    if (pos > 0)
                    {
                        this.mexRic = this.mexRic.Substring(0, pos);
                    }

                    string byDataToSend = szData;

                    // Inserisco i comandi ricevuti in una coda, di stringhe.
                    // this.m_bufStream[0].CommandsStr.Enqueue(byDataToSend);

                    // Send message
                    this.sendDataToClient(byDataToSend);

                    diffTime = startTime - DateTime.Now.Millisecond;
                    this.mexRic = this.mexRic + " - tempo impiegato " + diffTime.ToString() + " ms";

                    ThrowEvent(); // Lancio l'evento che modifica l'UI

                    // Continue the waiting for data on the Socket
                    this.waitForData(this.m_sckWorker);
                }
            }
            catch (SocketException ex)
            {
                // Il codice di errore 10053 è quello sollevato con la disconnessione del cavo
                if (ex.ErrorCode == 10053)
                {
                    this.WriteLog("La connessione tra il Client ed il Server è caduta!");
                    DiscSockets();
                }

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
        /// Stop listen.
        /// Close the connection to all sockets.
        /// </summary>
        public string StopListen()
        {
            this.WriteLog("StopListen");

            this.m_msgCounter = 0; // Lo azzero, deve contare i mex ricevuti per la sessione
            string cSocket = this.closeSockets();

            return cSocket;
        }

        // C'è solo CloseSocket per chiudere il Socket?
        /// <summary>
        /// Close sockets.
        /// </summary>
        private string closeSockets()
        {
            this.WriteLog("closeSockets");

            string cSocket = "STOP";

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

        /// <summary>
        /// Send a given string data to client.
        /// </summary>
        /// <param name="index">Index of client</param>
        /// <param name="szData">The string</param>
        private void sendDataToClient(string szData)
        {
            this.WriteLog("sendDataToClient");

            Object objData = szData;
            byte[] byData = System.Text.Encoding.Unicode.GetBytes(objData.ToString());

            if (this.m_sckWorker != null)
            {
                try
                {
                    if (this.m_sckWorker.Connected)
                    {
                        lock (g_lock)
                        {
                            this.m_sckWorker.Send(byData);
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

        public void NumMessaggi(out int m_msgCounter, out string riga)
        {
            this.WriteLog("NumMessaggi");

            m_msgCounter = this.m_msgCounter;
            riga = this.mexRic;
        }

        private void WriteLog(string messaggio)
        {
            lock (g_lock)
            {
                this.f = new FileStream(LOG_PATH + "serverLog.log", FileMode.Append);
                this.s = new StreamWriter(this.f);

                this.s.WriteLine(DateTime.Now.ToString() + " - " + messaggio);

                this.s.Close();
                this.f.Close();
            }
        }

        public void ClientConn(out string IPAddressClient, out string PortClient)
        {
            this.WriteLog("ClientConn");

            IPAddressClient = this.remoteIpEndPoint.Address.ToString();
            PortClient = this.remoteIpEndPoint.Port.ToString();
        }
    }
}

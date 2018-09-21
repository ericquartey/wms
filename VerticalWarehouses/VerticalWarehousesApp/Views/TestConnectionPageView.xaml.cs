using System;
using System.Text;
using System.Windows.Threading;
using System.Windows;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Controls;
using System.Collections;

namespace Ferretto.VW.VerticalWarehousesApp.Views
{
    /// <summary>
    /// Interaction logic for TestConnectionPageView.xaml
    /// </summary>
    public partial class TestConnectionPageView : Page
    {
        public TestConnectionPageView()
        {
            this.InitializeComponent();

            this.m_hevStart = null; this.m_hevStop = null;
            this.m_opThread = null;

            this.m_msgCounter = 0;
            this.txtIP.Text = this.getIP();
            this.txtPort.Text = DEFAULT_PORT.ToString();
        }

        const int DEFAULT_PORT = 8000;
        public AsyncCallback m_pfnCallBack;               //!< Reference to a callback for connecting to server socket 

        private byte[] m_dataBuffer = new byte[10];       //!< Data buffer socket  
        private IAsyncResult m_result;                    //!< Asynchronuos operation status
        private Socket m_sckClient;                       //!< Socket client 
        private int m_msgCounter;                         //!< Messages from server counter



        private AutoResetEvent m_hevStart;                //!< Start event for automated operation 
        private AutoResetEvent m_hevStop;                 //!< Stop event for automated operation 
        private Thread m_opThread;                        //!< Thread (main operation)


        static object g_lock = new object();              //!< Lock object for concurrency 

        #region - Win32 -

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceFrequency(out long lpPerformanceFrequency);

        /*

          long t1;
          QueryPerformanceCounter(out t1);

          long t2;
          QueryPerformanceCounter(out t2);

          long frq;
          QueryPerformanceFrequency(out frq);

          long dTime = (long)( ((double)(t2 - t1) * 1000) / frq);   // ms

        */


        #endregion



        /// <summary>
        /// Data socket object.
        /// Object reference used in all asynchronous operation callbacks. 
        /// </summary>

        public class SocketPacket
        {
            public System.Net.Sockets.Socket thisSocket;         //|< Current socket instance
            public byte[] dataBuffer = new byte[1024];           //!< Data buffer
        }



       

        #region - Support functions -


        /// <summary>
        /// Get IP address of local machine.
        /// </summary>
        /// <returns>The IP address.</returns>
        private string getIP()
        {
            string strHostName = Dns.GetHostName();

            // Find host by name
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);

            // Get the first IP addresses
            string szIP = "";
            foreach (IPAddress ipaddress in iphostentry.AddressList)
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

        /// <summary>
        /// Start waiting data from the server.
        /// </summary>
        public void WaitForData()
        {
            try
            {
                if (this.m_pfnCallBack == null)
                {
                    this.m_pfnCallBack = new AsyncCallback(this.OnDataReceived);
                }
                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.thisSocket = this.m_sckClient;
                // Start listening to the data asynchronously
                this.m_result = this.m_sckClient.BeginReceive(
                  theSocPkt.dataBuffer,
                  0,
                  theSocPkt.dataBuffer.Length,
                  SocketFlags.None,
                  this.m_pfnCallBack,
                  theSocPkt
                );
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }

        }

        /// <summary>
        /// Call back function which will be invoked when the socket detects the incoming data on the stream.
        /// </summary>
        /// <param name="asyn"></param>
        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket theSockId = (SocketPacket)asyn.AsyncState;
                int iRx = theSockId.thisSocket.EndReceive(asyn);

                char[] chars = new char[iRx/* + 1*/];
                System.Text.Decoder d = System.Text.Encoding.Unicode.GetDecoder();
                int charLen = d.GetChars(theSockId.dataBuffer, 0, iRx, chars, 0);
                System.String szData = new System.String(chars);

                // Get the elapsed time (performance' stuff)
                long elapsedTime_us = this.getElapsedTime(Convert.ToInt64(szData));


                this.m_msgCounter++;

                // Update the UI
                this.Dispatcher.BeginInvoke(new Action(() => this.updateUIOnDataReceived(String.Format("{0}) - Time fly: {1} us", this.m_msgCounter, elapsedTime_us))));

                // Do something and send a reply message to server
                lock (g_lock)
                {
                    this.doSomething();
                }

                this.WaitForData();
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        /// <summary>
        /// Update UI according to the current context.
        /// </summary>
        /// <param name="bConnected">The context flag.</param>
        private void updateControls(bool bConnected)
        {
            this.Connect_Button.IsEnabled = !bConnected;
            this.Disconnect_Button.IsEnabled = bConnected;
            string szConnectStatus = bConnected ? "Connected" : "Not Connected";
            string szHandle = (this.m_sckClient == null) ? "NULL" : this.m_sckClient.Handle.ToString();

            this.tbStatus.Text = String.Format("Client [handle: {0}] is {1}", szHandle, szConnectStatus);
        }


        /// <summary>
        /// Update UI with data string received from server.
        /// </summary>
        /// <param name="szData"></param>
        private void updateUIOnDataReceived(string szData)
        {
            if (this.m_msgCounter == 0)
            {
                this.tbReceivedMsg.Text = "";
                this.tbReceivedMsg.AppendText(String.Format("Server --> data=>{0}\n", szData));
            }

        }




        /// <summary>
        /// Send a given string data to server.
        /// </summary>
        /// <param name="szData"></param>
        private void sendDataToServer(string szData)
        {
            try
            {
                Object objData = szData;

                byte[] byData = System.Text.Encoding.Unicode.GetBytes(objData.ToString());
                if (this.m_sckClient != null)
                {
                    // Send data
                    this.m_sckClient.Send(byData);
                }
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }


        /// <summary>
        /// Try to calculate the elapsed time (ns).
        /// </summary>
        /// <param name="t1"></param>
        private long getElapsedTime(long t1)
        {
            QueryPerformanceCounter(out long t2);

            QueryPerformanceFrequency(out long frq);
            long elapsedTime_us = (int)((double)(t2 - t1) * 1000000 / frq);  // us

            return elapsedTime_us;
        }


        #endregion



        #region - Threading -

        /// <summary>
        /// Create working thread.
        /// </summary>
        private void createThread()
        {
            this.m_hevStart = new AutoResetEvent(false);
            this.m_hevStop = new AutoResetEvent(false);


            // Run an internal thread to perform operation
            this.m_opThread = new Thread(this.mainThread);
            this.m_opThread.Name = "workingClientThread";
            this.m_opThread.Start();

            // 
            this.m_hevStart.Set();
        }


        /// <summary>
        /// Release resource of working thread.
        /// </summary>
        private void destroyThread()
        {
            this.m_opThread.Abort();

            this.m_hevStart.Close(); this.m_hevStart = null;
            this.m_hevStop.Close(); this.m_hevStop = null;

        }


        /// <summary>
        /// Working thread.
        /// </summary>
        private void mainThread()
        {
            const int START_ = 0;
            const int STOP_ = 1;

            WaitHandle[] handles = new WaitHandle[2];
            handles[0] = this.m_hevStart;
            handles[1] = this.m_hevStop;

            bool bExit = false;
            while (!bExit)
            {
                int code = WaitHandle.WaitAny(handles, -1);
                switch (code)
                {
                    case START_:
                        {
                            // Do nothing
                            break;
                        }
                    case STOP_:
                        {
                            // Exit from thread
                            bExit = true;
                            break;
                        }

                }
            }
        }



        #endregion



        /// <summary>
        /// Perform an operation and send a message to server.
        /// </summary>
        private void doSomething()
        {
            const int TIME = 150; // ms

            long t1;
            QueryPerformanceCounter(out t1);

            // Simply wait...
            // It takes a TIME operation length 
            Thread.Sleep(TIME);

            long t2;
            QueryPerformanceCounter(out t2);

            long frq;
            QueryPerformanceFrequency(out frq);
            int elapsedTime_ms = (int)((double)((t2 - t1) * 1000 / frq));

            // Send data to server (acknowledge)
            this.sendDataToServer(String.Format("Ack --> [client handle: {0}] Time calculation: {1} ms; # msg: {2}", this.m_sckClient.Handle, elapsedTime_ms, this.m_msgCounter));
        }






        /// <summary>
        /// Connect to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            // See if we have text on the IP and Port text fields
            if (this.txtIP.Text == "" || this.txtPort.Text == "")
            {
                MessageBox.Show("IP Address and Port Number are required to connect to the Server\n");
                return;
            }
            try
            {
                this.updateControls(false);

                // Create one SocketPermission for socket access restrictions 
                SocketPermission permission = new SocketPermission(
                  NetworkAccess.Connect,        // Connection permission 
                  TransportType.Tcp,            // Defines transport types 
                  "",                           // Gets the IP addresses 
                  SocketPermission.AllPorts     // All ports 
                );

                // Ensures the code to have permission to access a Socket 
                permission.Demand();

                // Resolves a host name to an IPHostEntry instance            
                IPHostEntry ipHost = Dns.GetHostEntry("");

                // Gets first IP address associated with a localhost 
                //IPAddress ipAddr = ipHost.AddressList[0];

                // Get the given IP address 
                IPAddress ipAddr = IPAddress.Parse(this.txtIP.Text);

                // Assign the address port
                int iPortNumber = System.Convert.ToInt16(this.txtPort.Text);

                // Create the socket instance
                this.m_sckClient = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                //m_sckClient.NoDelay = false;   // Using the Nagle algorithm 

                // Create the end point 
                IPEndPoint ipEnd = new IPEndPoint(ipAddr, iPortNumber);
                // Connect to the remote host
                this.m_sckClient.Connect(ipEnd);
                if (this.m_sckClient.Connected)
                {
                    this.updateControls(true);
                    this.tbReceivedMsg.Text = "";

                    // Wait for data asynchronously 
                    this.WaitForData();

                    // Create working thread
                    this.createThread();
                }
            }
            catch (SocketException se)
            {
                string szMsg = "\nConnection failed, is the server running?\n" + se.Message;
                MessageBox.Show(szMsg);
                this.updateControls(false);
            }
        }

        /// <summary>
        /// Disconnect from server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            // Stop running thread
            this.m_hevStop.Set();

            if (this.m_sckClient != null)
            {
                this.m_sckClient.Close();
                this.m_sckClient = null;

                this.updateControls(false);
            }
        }



        /// <summary>
        /// Send an asynchronous message to server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void Send_Click(object sender, RoutedEventArgs e)

        {
            try
            {
                // Send message to connected server

                string theMessageToSend = this.tbMsg.Text;
                byte[] msg = Encoding.Unicode.GetBytes(theMessageToSend);

                // Sends data to a connected Socket. 
                int bytesSend = this.m_sckClient.Send(msg);



                this.Send_Button.IsEnabled = true;
                this.Disconnect_Button.IsEnabled = true;
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }




        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }


        /// <summary>
        /// Close the client and the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            // Close the client
            if (this.m_sckClient != null)
            {
                this.m_sckClient.Close();
                this.m_sckClient = null;
            }

            // Delete working thread
            this.destroyThread();

            // Close application
            this.Close();
        }

        private void Close()
        {
            throw new NotImplementedException();
        }
    }// class MainWindow
}


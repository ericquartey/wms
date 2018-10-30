using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using Ferretto.VW.InvServer;
using NLog;

namespace Ferretto.VW.SWEmulatorInverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        // Logger
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Program Inverter;

        private int MessageNumber;

        #endregion Fields

        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();

            // create a new instance of eMulator inverter interface
            this.Inverter = new Program();
            this.MessageNumber = 0;

            var ipAddress = this.Inverter.GetIPAddress();

            this.TextBox.Text = "IP Address: " + ipAddress;
            this.TextBox1.Text = "0";

            this.TextBoxArea.Text = "Server is on ...";
            this.TextBoxArea.AppendText(Environment.NewLine);

            this.Btn_Stop.IsEnabled = false;

            this.BtnEsci.Background = Brushes.Red;

            // Evento per la gestione della ricezione dei messaggi
            this.Inverter.ThrowEvent += new Program.MessageEventHandler(this.UpdateInformationOnBox);

            // Event handler for client connection
            this.Inverter.SendClientEvent += new Program.ConnClientEventHandler(this.MexConnClient);

            // Event handler for hardware cable unplug
            this.Inverter.DiscSockets += new Program.DisconnectedSocketsEventHandler(this.DisconWire);

            // Event handler for client disconnection
            this.Inverter.DiscClient += new Program.DisconnectedClientEventHandler(this.DisconClient);

            this.Cmd = new List<InverterCmd>();

            this.CmdList.ItemContainerStyle = (Style)this.CmdList.Resources["itemstyle"];

            this.DataContext = this;

            logger.Log(LogLevel.Debug, String.Format("SW Emulator Inverter is running..."));
        }

        #endregion Constructors

        #region Properties

        public List<InverterCmd> Cmd { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(out int Description, int ReservedValue);

        /// <summary>
        /// Start the server to listening.
        /// </summary>
        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            // Socket starting to wait for a client
            var StartLitener = this.Inverter.StartListen();
            this.Inverter.StateOperativeInverter = (StartLitener.Equals("ERROR")) ? false : true;

            // Start Server
            this.StartMessages(StartLitener);

            this.TextBoxAS.Text = StartLitener;

            this.TextBoxArea.AppendText("Server started ...");
            this.TextBoxArea.AppendText(Environment.NewLine);

            this.Btn_Start.IsEnabled = false;
            this.Btn_Stop.IsEnabled = true;
            this.Btn_Stop.IsEnabled = true;
        }

        /// <summary>
        /// Stop the listening.
        /// </summary>
        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            var StopListen = this.Inverter.StopListen();

            // Stop the server messages
            this.StopMessages(StopListen);
        }

        /// <summary>
        /// Impose the END of an operation.
        /// </summary>
        private void Button_Done(Object sender, RoutedEventArgs e)
        {
            // The <DONE> condition is obtained via setup of status word bit line (dedicated).
            // TODO: develop this feature...
            this.Inverter.SetStateLineInverter(5, true);

            this.TextBoxArea.AppendText("DONE button is pressed");
            this.TextBoxArea.AppendText(Environment.NewLine);
        }

        /// <summary>
        /// Set an error condition (via UI).
        /// </summary>
        private void Button_Error(Object sender, RoutedEventArgs e)
        {
            this.RBNO.IsChecked = true;

            this.Inverter.SetErrorCondition = true;

            this.TextBoxArea.AppendText("ERROR message is set to the client.");
            this.TextBoxArea.AppendText(Environment.NewLine);
        }

        /// <summary>
        /// Close the application.
        /// </summary>
        private void Button_Esci(object sender, RoutedEventArgs e)
        {
            logger.Log(LogLevel.Debug, String.Format("SW Emulator Inverter quit."));
            this.MainWin?.Close();
        }

        /// <summary>
        /// On client disconnection.
        /// </summary>
        private void DisconClient(string StopListen, string StartLitener)
        {
            // Execute the following instructions in the UI thread.
            this.Dispatcher.Invoke(() =>
            {
                this.TextBoxArea.AppendText("Client is disconnected! The server is restarted...");
                this.TextBoxArea.AppendText(Environment.NewLine);

                // Stop the server
                this.StopMessages(StopListen);

                // Start Server
                this.StartMessages(StartLitener);
            });
        }

        /// <summary>
        /// On hardware cable unplug.
        /// </summary>
        private void DisconWire(string StopListen)
        {
            // Execute the following instructions in the UI thread.
            this.Dispatcher.Invoke(() =>
            {
                this.TextBoxArea.AppendText("The connection between client and server is no more active.");
                this.TextBoxArea.AppendText(Environment.NewLine);

                // Stop the server
                this.StopMessages(StopListen);
            });
        }

        /// <summary>
        /// Event handler for the connection client.
        /// </summary>
        private void MexConnClient()
        {
            this.Inverter.ClientConn(out var IPAddressClient, out var PortClient);

            // Execute the following instructions in the UI thread.
            this.Dispatcher.Invoke(() =>
            {
                this.TextBoxArea.AppendText("Connected to IP:" + IPAddressClient + ", port: " + PortClient);
                this.TextBoxArea.AppendText(Environment.NewLine);
            });
        }

        private string ParameterID2String(ParameterID paramID)
        {
            var value = string.Empty;
            switch (paramID)
            {
                case ParameterID.CONTROL_WORD_PARAM: value = "CTRL W"; break;
                case ParameterID.HOMING_CREEP_SPEED_PARAM: value = "Homing Creep Speed"; break;
                case ParameterID.HOMING_FAST_SPEED_PARAM: value = "Homing Fast Speed"; break;
                case ParameterID.HOMING_MODE_PARAM: value = "Homing Mode Value"; break;
                case ParameterID.HOMING_OFFSET_PARAM: value = "Homing Offset"; break;
                case ParameterID.POSITION_ACCELERATION_PARAM: value = "Position acc"; break;
                case ParameterID.POSITION_DECELERATION_PARAM: value = "Position dec"; break;
                case ParameterID.POSITION_TARGET_POSITION_PARAM: value = "Position Target pos"; break;
                case ParameterID.POSITION_TARGET_SPEED_PARAM: value = "Position Target Speed"; break;
                case ParameterID.SET_OPERATING_MODE_PARAM: value = "Set Operating Mode"; break;
                case ParameterID.STATUS_WORD_PARAM: value = "STATUS W"; break;
                default: value = "NOT"; break;
            }
            return value;
        }

        private void RBNO_Checked(Object sender, RoutedEventArgs e)
        {
            if (this.Inverter != null)
            {
                this.Inverter.StateOperativeInverter = false;
            }
        }

        private void RBYES_Checked(Object sender, RoutedEventArgs e)
        {
            if (this.Inverter != null)
            {
                this.Inverter.StateOperativeInverter = true;
            }
        }

        private void StartMessages(string StartLitener)
        {
            this.RBYES.IsChecked = true;

            this.TextBoxAS.Text = StartLitener;

            this.TextBoxArea.AppendText("Server is running... Wait for a new request of connection...");
            this.TextBoxArea.AppendText(Environment.NewLine);

            this.Btn_Start.IsEnabled = false;
            this.Btn_Stop.IsEnabled = true;
            this.Btn_Stop.IsEnabled = true;
        }

        private void StopMessages(string StopLiten)
        {
            this.MessageNumber = 0;
            this.TextBox1.Text = this.MessageNumber.ToString();

            this.RBNO.IsChecked = true;
            this.TextBoxAS.Text = StopLiten;

            this.TextBoxArea.AppendText("Server is stopped.");
            this.TextBoxArea.AppendText(Environment.NewLine);

            this.Btn_Start.IsEnabled = true;
            this.Btn_Stop.IsEnabled = false;

            // When the client disconnect or the server is
            // being stopped, the ListView has to be empty.
            this.Cmd.Clear();
            this.CmdList.ItemsSource = null;
            this.CmdList.ItemsSource = this.Cmd;
        }

        /// <summary>
        /// On sending response telegram to client.
        /// </summary>
        private void UpdateInformationOnBox(ParameterID paramID)
        {
            var singleCmd = new InverterCmd();
            singleCmd.CodeOp = this.ParameterID2String(paramID);

            this.MessageNumber++;

            // Execute the following instructions in the UI thread.
            this.Dispatcher.Invoke(() =>
            {
                this.Cmd.Add(singleCmd);
                this.TextBox1.Text = this.MessageNumber.ToString();

                this.TextBoxArea.AppendText("Received Parameter Code: " + paramID.ToString());
                this.TextBoxArea.AppendText(Environment.NewLine);

                // Table refresh
                this.CmdList.ItemsSource = null;
                this.CmdList.ItemsSource = this.Cmd;
            });
        }

        #endregion Methods
    }
}

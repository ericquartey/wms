using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.VW.InvServer;


using System.Diagnostics;

namespace Ferretto.VW.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private Program Inverter;

        #endregion Fields

        public List<InverterCmd> Cmd { get; set; }
        byte OpStatus;
        byte[] SendClient;
        int MessageNumber;

        #region Constructors

        public MainWindow()
        {

            this.InitializeComponent();

            this.Inverter = new Program();

            this.OpStatus = 0x01;
            this.MessageNumber = 0;
            this.Inverter.SetStateInverter = true;

            var ipAddress = this.Inverter.GetIPAddress();

            this.TextBox.Text = "IP Address: " + ipAddress;
            this.TextBox1.Text = "0";

            this.TextBoxArea.Text = "Server Acceso ...";
            this.TextBoxArea.AppendText(Environment.NewLine);

            this.Btn_Stop.IsEnabled = false;

            this.BtnEsci.Background = Brushes.Red;

            // Evento per la gestione della ricezione dei messaggi
            this.Inverter.ThrowEvent += new Program.MessageEventHandler(this.AggiornaTextBox);

            // Evento per la gestione del client connesso
            this.Inverter.SendClientEvent += new Program.ConnClientHandler(this.MexConnClient);

            // Evento per la gestione della disconnessione del cavo
            this.Inverter.DiscSockets += new Program.DisconnectedSocketsEventHandler(this.DisconWire);

            // Evento per la gestione della disconnessione del Client
            this.Inverter.DiscClient += new Program.DisconnectedClientEventHandler(this.DisconClient);

            this.Cmd = new List<InverterCmd>();

            this.CmdList.ItemContainerStyle = (Style)this.CmdList.Resources["itemstyle"];

            this.DataContext = this;
        }

        #endregion Constructors

        #region Methods

        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(out int Description, int ReservedValue);

        private void AggiornaTextBox(InverterCmd SingleCmd)
        {
            byte OpCode = Convert.ToByte(SingleCmd.CodeOp);

            this.MessageNumber++;

            this.SendClient = new byte[]{ 0x03, OpCode, this.OpStatus };

            // Serve per evitare l'errore di modificare il TextBox1 che si trova su un Task separato
            this.Dispatcher.Invoke(() =>
            {
                this.Cmd.Add(SingleCmd);
                this.TextBox1.Text = this.MessageNumber.ToString();

                this.TextBoxArea.AppendText("Received Op. Code: " + SingleCmd.CodeOp);
                this.TextBoxArea.AppendText(Environment.NewLine);

                // Table refresh 
                this.CmdList.ItemsSource = null;
                this.CmdList.ItemsSource = this.Cmd;
            });
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            // Socket starting to wait for a client
            string StartLitener = this.Inverter.StartListen();

            // Start Server
            this.StartMessages(StartLitener);

            this.TextBoxAS.Text = StartLitener;

            this.TextBoxArea.AppendText("Server Avviato ...");
            this.TextBoxArea.AppendText(Environment.NewLine);

            this.Btn_Start.IsEnabled = false;
            this.Btn_Stop.IsEnabled = true;
            this.Btn_Stop.IsEnabled = true;
 
        }

        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            string StopListen = this.Inverter.StopListen();

            // Stop the server messages
            this.StopMessages(StopListen);
        }

        private void Button_Done(Object sender, RoutedEventArgs e)
        {
            this.Inverter.sendDataToClient(this.SendClient);

            this.TextBoxArea.AppendText("Premuto pulsante DONE");
            this.TextBoxArea.AppendText(Environment.NewLine);
        }

        private void Button_Esci(object sender, RoutedEventArgs e)
        {
            if (this.MainWin != null)
            {
                this.MainWin.Close();
            }
        }

        private void DisconWire(string StopListen)
        {
            // Serve per evitare l'errore di modificare il TextBox1 che si trova su un Task separato
            this.Dispatcher.Invoke(() =>
            {
                this.TextBoxArea.AppendText("La connessione tra Client e Server non è più attiva.");
                this.TextBoxArea.AppendText(Environment.NewLine);

                // Stop the server
                this.StopMessages(StopListen);
            });
        }

        private void DisconClient(string StopListen, string StartLitener)
        {
            // Serve per evitare l'errore di modificare il TextBox1 che si trova su un Task separato
            this.Dispatcher.Invoke(() =>
            {
                this.TextBoxArea.AppendText("Il Client si è disconnesso, esecuzione del riavvio del server.");
                this.TextBoxArea.AppendText(Environment.NewLine);

                // Stop the server
                this.StopMessages(StopListen);

                // Start Server
                this.StartMessages(StartLitener);
            });
        }

        private void StartMessages(string StartLitener)
        {
            this.Dispatcher.Invoke(() =>
            {

                this.RBYES.IsChecked = true;

                this.TextBoxAS.Text = StartLitener;

                this.TextBoxArea.AppendText("Server Avviato ... in attesa di un Client");
                this.TextBoxArea.AppendText(Environment.NewLine);

                this.Btn_Start.IsEnabled = false;
                this.Btn_Stop.IsEnabled = true;
                this.Btn_Stop.IsEnabled = true;
            });
        }
        private void StopMessages(string StopLiten)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.MessageNumber = 0;
                this.TextBox1.Text = this.MessageNumber.ToString();

                this.RBNO.IsChecked = true;
                this.TextBoxAS.Text = StopLiten;

                this.TextBoxArea.AppendText("Server Arrestato");
                this.TextBoxArea.AppendText(Environment.NewLine);

                this.Btn_Start.IsEnabled = true;
                this.Btn_Stop.IsEnabled = false;

                // When the client disconnect or the server is
                // being stopped, the ListView has to be empty.
                this.Cmd.Clear();
                this.CmdList.ItemsSource = null;
                this.CmdList.ItemsSource = this.Cmd;
            });
        }

        private void MexConnClient()
        {
            this.Inverter.ClientConn(out var IPAddressClient, out var PortClient);

            // Serve per evitare l'errore di modificare il TextBox1 che si trova su un Task separato
            this.Dispatcher.Invoke(() =>
            {
                this.TextBoxArea.AppendText("Cliente connesso con IP " + IPAddressClient + " sulla porta " + PortClient);
                this.TextBoxArea.AppendText(Environment.NewLine);
            });
        }

        #endregion Methods

        private void Button_Error(Object sender, RoutedEventArgs e)
        {
            this.RBNO.IsChecked = true;

            // In risposta invio 3 bytes (lunghezza del messaggio, codice dell'errore e stato
            // dell'Inverter) che deve essere 0, perché devo simulare un errore.
            this.SendClient = new byte[] { 0x03, 0xFE, 0x00 };

            this.Inverter.sendDataToClient(this.SendClient);

            // Serve per evitare l'errore di modificare il TextBox1 che si trova su un Task separato
            this.Dispatcher.Invoke(() =>
            {
                this.TextBoxArea.AppendText("ERROR message sent to the client.");
                this.TextBoxArea.AppendText(Environment.NewLine);
            });
        }

        private void RBYES_Checked(Object sender, RoutedEventArgs e)
        {
            if (this.Inverter!=null)
            { 
                this.Inverter.SetStateInverter = true;
                this.OpStatus = 0x01;
            }
        }

        private void RBNO_Checked(Object sender, RoutedEventArgs e)
        {
            if (this.Inverter != null)
            {
                this.Inverter.SetStateInverter = false;
                this.OpStatus = 0x00;
            }
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;

using Ferretto.VW.InvServer;
using System.Windows.Media;

namespace Ferretto.VW.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        private Program programma;

        public MainWindow()
        {
            this.InitializeComponent();

            this.programma = new Program();
            string ipAddress = this.programma.GetIPAddress();

            this.TextBox.Text = "IP Address: " + ipAddress;
            this.TextBox1.Text = "0";

            this.TextBoxArea.Text = "Server Acceso ...";
            this.TextBoxArea.AppendText(Environment.NewLine);

            this.Btn_Stop.IsEnabled = false;

            this.BtnEsci.Background = Brushes.Red;

            // Evento per la gestione della ricezione dei messaggi
            this.programma.ThrowEvent += new Program.EventHandler(this.AggiornaTextBox);

            // Evento per la gestione del client connesso
            this.programma.SendClientEvent += new Program.ConnClientHandler(this.MexConnClient);

            // Evento per la gestione della disconnessione del cavo
            this.programma.DiscSockets += new Program.DisconnectedSockets(this.DisconWire);
        }

        private void AggiornaTextBox()
        {
            int numMessaggi = 0;
            string mexRic = "";
            this.programma.NumMessaggi(out numMessaggi, out mexRic);

            // Serve per evitare l'errore di modificare il TextBox1 che si trova su un Task separato
            this.Dispatcher.Invoke(() =>
            {
                this.TextBox1.Text = numMessaggi.ToString();

                this.TextBoxArea.AppendText(mexRic);
                this.TextBoxArea.AppendText(Environment.NewLine);
            });
        }

        private void MexConnClient()
        {
            string IPAddressClient = "",
            PortClient = "";

            this.programma.ClientConn(out IPAddressClient, out PortClient);

            // Serve per evitare l'errore di modificare il TextBox1 che si trova su un Task separato
            this.Dispatcher.Invoke(() =>
            {
                this.TextBoxArea.AppendText("Cliente connesso con IP " + IPAddressClient + " sulla porta " + PortClient);
                this.TextBoxArea.AppendText(Environment.NewLine);
            });
        }

        private void DisconWire()
        {
            // Serve per evitare l'errore di modificare il TextBox1 che si trova su un Task separato
            this.Dispatcher.Invoke(() =>
            {
                this.TextBoxArea.AppendText("La connessione tra Client e Server non è più attiva.");
                this.TextBoxArea.AppendText(Environment.NewLine);

                // Rilancio l'evento Click sul pulsante
                this.Btn_Stop.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            });
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            if (this.IsConnectedToInternet())
            {
                // Eseguo l'avvio del Socket per l'attesa del Client
                string sLitener = this.programma.StartListen();

                this.TextBoxAS.Text = sLitener;

                this.TextBoxArea.AppendText("Server Avviato ...");
                this.TextBoxArea.AppendText(Environment.NewLine);

                this.Btn_Start.IsEnabled = false;
                this.Btn_Stop.IsEnabled = true;
                this.Btn_Stop.IsEnabled = true;
            }
            else
            {
                this.TextBoxArea.AppendText("Server NON avviato, connessione ad internet assente.");
                this.TextBoxArea.AppendText(Environment.NewLine);
            }
        }

        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            string stopListen = this.programma.StopListen();

            this.TextBoxAS.Text = stopListen;
            this.TextBox1.Text = "0";

            this.TextBoxArea.AppendText("Server Arrestato");
            this.TextBoxArea.AppendText(Environment.NewLine);

            this.Btn_Start.IsEnabled = true;
            this.Btn_Stop.IsEnabled = false;
        }

        //Creating a function that uses the API function to check the internet connection
        public bool IsConnectedToInternet()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }

        private void Button_Esci(object sender, RoutedEventArgs e)
        {
            if (this.MainWin != null)
            {
                this.MainWin.Close();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ferretto.VW.Utils
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
            InitializeComponent();

            programma = new Program();
            string ipAddress = programma.GetIPAddress();

            TextBox.Text = "IP Address: " + ipAddress;
            TextBox1.Text = "0";

            TextBoxArea.Text = "Server Acceso ...";
            TextBoxArea.AppendText(Environment.NewLine);

            Btn_Stop.IsEnabled = false;

            BtnEsci.Background = Brushes.Red;

            // Evento per la gestione della ricezione dei messaggi
            programma.ThrowEvent += new Program.EventHandler(AggiornaTextBox);

            // Evento per la gestione del client connesso
            programma.SendClientEvent += new Program.ConnClientHandler(MexConnClient);

            // Evento per la gestione della disconnessione del cavo
            programma.DiscSockets += new Program.DisconnectedSockets(DisconWire);

        }
        private void AggiornaTextBox()
        {
            int numMessaggi = 0;
            string mexRic = "";
            programma.NumMessaggi(out numMessaggi, out mexRic);

            // Serve per evitare l'errore di modificare il TextBox1 che si trova su un Task separato
            this.Dispatcher.Invoke(() =>
            {
                TextBox1.Text = numMessaggi.ToString();

                TextBoxArea.AppendText(mexRic);
                TextBoxArea.AppendText(Environment.NewLine);
            });
        }

        private void MexConnClient()
        {
            string IPAddressClient = "",
                   PortClient = "";

            programma.ClientConn(out IPAddressClient, out PortClient);

            // Serve per evitare l'errore di modificare il TextBox1 che si trova su un Task separato
            this.Dispatcher.Invoke(() =>
            {
                TextBoxArea.AppendText("Cliente connesso con IP " + IPAddressClient + " sulla porta " + PortClient);
                TextBoxArea.AppendText(Environment.NewLine);
            });
        }

        private void DisconWire()
        {
            // Serve per evitare l'errore di modificare il TextBox1 che si trova su un Task separato
            this.Dispatcher.Invoke(() =>
            {
                TextBoxArea.AppendText("La connessione tra Client e Server non è più attiva.");
                TextBoxArea.AppendText(Environment.NewLine);

                // Rilancio l'evento Click sul pulsante
                Btn_Stop.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            });
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            if (IsConnectedToInternet())
            {
                // Eseguo l'avvio del Socket per l'attesa del Client
                string sLitener = programma.StartListen();

                TextBoxAS.Text = sLitener;

                TextBoxArea.AppendText("Server Avviato ...");
                TextBoxArea.AppendText(Environment.NewLine);

                Btn_Start.IsEnabled = false;
                Btn_Stop.IsEnabled = true;
                Btn_Stop.IsEnabled = true;
            }
            else
            {
                TextBoxArea.AppendText("Server NON avviato, connessione ad internet assente.");
                TextBoxArea.AppendText(Environment.NewLine);
            }
        }

        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            string stopListen = programma.StopListen();

            TextBoxAS.Text = stopListen;
            TextBox1.Text = "0";

            TextBoxArea.AppendText("Server Arrestato");
            TextBoxArea.AppendText(Environment.NewLine);

            Btn_Start.IsEnabled = true;
            Btn_Stop.IsEnabled = false;
        }

        //Creating a function that uses the API function to check the internet connection
        public bool IsConnectedToInternet()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }

        private void Button_Esci(object sender, RoutedEventArgs e)
        {
            if (MainWin != null)
            {
                MainWin.Close();
            }
        }
    }
}

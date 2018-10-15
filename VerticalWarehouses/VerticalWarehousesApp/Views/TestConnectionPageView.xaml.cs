using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.InverterDriver;

namespace Ferretto.VW.VerticalWarehousesApp.Views
{
    /// <summary>
    /// Interaction logic for TestConnectionPageView.xaml
    /// </summary>
    public partial class TestConnectionPageView : Page
    {
        #region Fields

        public const string IP_ADDR_INVERTER_DEFAULT = "172.16.199.200";

        public const int PORT_ADDR_INVERTER_DEFAULT = 8000;

        private CInverterDriver driver;

        #endregion Fields

        #region Constructors

        public TestConnectionPageView()
        {
            this.InitializeComponent();

            this.driver = new CInverterDriver();
            this.driver.Connected += this.Driver_Connected;
            this.driver.GetMessageFromServer += this.Driver_GetMessageFromServer;
            this.TxtBoxIP.Text = IP_ADDR_INVERTER_DEFAULT.ToString();
            this.TxtBoxPort.Text = PORT_ADDR_INVERTER_DEFAULT.ToString();
        }

        #endregion Constructors

        #region Methods

        private void Click_Close(object sender, RoutedEventArgs e)
        {
            var response = MessageBox.Show("Do you really want to exit?", "Exiting...",
                                   MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (response == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void Click_Execute(Object sender, RoutedEventArgs e)
        {
            var cmdId = CommandId.None;
            switch (this.ComboBox_Commands.SelectedIndex)
            {
                case 0:
                    {
                        this.driver.SetVerticalAxisOrigin(Convert.ToByte(this.TxtBoxP1.Text), Convert.ToSingle(this.TxtBoxP2.Text), Convert.ToSingle(this.TxtBoxP3.Text),
                                                          Convert.ToSingle(this.TxtBoxP4.Text), Convert.ToSingle(this.TxtBoxP5.Text), Convert.ToSingle(this.TxtBoxP6.Text));
                        break;
                    }
                case 1:
                    {
                        this.driver.MoveAlongVerticalAxisToPoint(Convert.ToInt16(this.TxtBoxP1.Text), Convert.ToSingle(this.TxtBoxP2.Text), Convert.ToSingle(this.TxtBoxP3.Text),
                                                                 Convert.ToSingle(this.TxtBoxP4.Text), Convert.ToSingle(this.TxtBoxP5.Text));
                        break;
                    }
                case 2:
                    {
                        this.driver.SelectMovement(Convert.ToByte(this.TxtBoxP1.Text));
                        break;
                    }
                case 3:
                    {
                        this.driver.MoveAlongHorizontalAxisWithProfile(Convert.ToSingle(this.TxtBoxP1.Text), Convert.ToSingle(this.TxtBoxP2.Text), Convert.ToInt16(this.TxtBoxP3.Text), Convert.ToInt16(this.TxtBoxP4.Text),
                                                                       Convert.ToSingle(this.TxtBoxP5.Text), Convert.ToSingle(this.TxtBoxP6.Text), Convert.ToInt16(this.TxtBoxP7.Text), Convert.ToInt16(this.TxtBoxP8.Text),
                                                                       Convert.ToSingle(this.TxtBoxP9.Text), Convert.ToSingle(this.TxtBoxP10.Text), Convert.ToInt16(this.TxtBoxP11.Text), Convert.ToInt16(this.TxtBoxP12.Text),
                                                                       Convert.ToSingle(this.TxtBoxP13.Text), Convert.ToInt16(this.TxtBoxP14.Text));
                        break;
                    }
                case 4:
                    {
                        this.driver.RunShutter(Convert.ToByte(this.TxtBoxP1.Text));
                        break;
                    }
                case 5:
                    {
                        this.driver.RunDrawerWeightRoutine(Convert.ToInt16(this.TxtBoxP1.Text), Convert.ToSingle(this.TxtBoxP2.Text), Convert.ToSingle(this.TxtBoxP3.Text), Convert.ToByte(this.TxtBoxP4.Text));
                        break;
                    }
                case 6:
                    {
                        this.driver.GetDrawerWeight(Convert.ToSingle(this.TxtBoxP1.Text));
                        break;
                    }
                case 7:
                    {
                        this.driver.Stop();
                        break;
                    }
                case 8:
                    {
                        this.driver.GetMainState();
                        break;
                    }
                case 9:
                    {
                        this.driver.GetIOState();
                        break;
                    }
                case 10:
                    {
                        this.driver.GetIOEmergencyState();
                        break;
                    }
                case 11:
                    {
                        this.driver.Set(Convert.ToInt32(this.TxtBoxP1.Text), Convert.ToByte(this.TxtBoxP2.Text));
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// Connect to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Click_Initialize(Object sender, RoutedEventArgs e)
        {
            // Assign the IP address to connect
            this.driver.IPAddressToConnect = this.TxtBoxIP.Text;
            this.driver.PortAddressToConnect = Convert.ToInt32(this.TxtBoxPort.Text);

            // Initialize the inverter driver: driver is connect
            var Result = this.driver.Initialize();
            this.Button_Initialize.IsEnabled = false;
        }

        /// <summary>
        /// Disconnect from server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Click_Terminate(Object sender, RoutedEventArgs e)
        {
            this.driver.Terminate();
        }

        private void Driver_Connected(Object sender, ConnectedEventArgs eventArgs)
        {
            var State = eventArgs.State;
            this.Dispatcher.BeginInvoke(new Action(() => this.updateUI(State)));
        }

        private void Driver_GetMessageFromServer(object sender, GetMessageFromServerEventArgs eventArgs)
        {
            var cmdId = eventArgs.CmdId;
            var Message = eventArgs.Message;
            this.Dispatcher.BeginInvoke(new Action(() => this.updateUI(cmdId, Message)));
        }

        private void PreviewTextInput_TxtBox(Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }
        private void updateUI(bool State)
        {
            this.TxtBoxStatus.Text = (State) ? "Connected" : "Not connected";
        }

        private void updateUI(CommandId cmdId, string Message)
        {
            this.TxtBoxReceiveMessageFromServer.Text = String.Format("Command id: {0} Text: {1}", cmdId.ToString(), Message);
        }

        #endregion Methods

    }
}

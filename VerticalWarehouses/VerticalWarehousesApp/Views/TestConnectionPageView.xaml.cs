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
            this.txtIP.Text = IP_ADDR_INVERTER_DEFAULT.ToString();
            this.txtPort.Text = PORT_ADDR_INVERTER_DEFAULT.ToString();
        }

        #endregion Constructors

        #region Methods

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            var response = MessageBox.Show("Do you really want to exit?", "Exiting...",
                                   MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (response == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void Button_Click(Object sender, RoutedEventArgs e)
        {
            var cmdId = CommandId.None;
            switch (this.cbxCommands.SelectedIndex)
            {
                case 0:
                    {
                        this.driver.SetVerticalAxisOrigin(Convert.ToByte(this.txtp1.Text), Convert.ToSingle(this.txtp2.Text), Convert.ToSingle(this.txtp3.Text),
                                                          Convert.ToSingle(this.txtp4.Text), Convert.ToSingle(this.txtp5.Text), Convert.ToSingle(this.txtp6.Text));
                        break;
                    }
                case 1:
                    {
                        this.driver.MoveAlongVerticalAxisToPoint(Convert.ToInt16(this.txtp1.Text), Convert.ToSingle(this.txtp2.Text), Convert.ToSingle(this.txtp3.Text),
                                                                 Convert.ToSingle(this.txtp4.Text), Convert.ToSingle(this.txtp5.Text));
                        break;
                    }
                case 2:
                    {
                        this.driver.SelectMovement(Convert.ToByte(this.txtp1.Text));
                        break;
                    }
                case 3:
                    {
                        this.driver.MoveAlongHorizontalAxisWithProfile(Convert.ToSingle(this.txtp1.Text), Convert.ToSingle(this.txtp2.Text), Convert.ToInt16(this.txtp3.Text), Convert.ToInt16(this.txtp4.Text),
                                                                       Convert.ToSingle(this.txtp5.Text), Convert.ToSingle(this.txtp6.Text), Convert.ToInt16(this.txtp7.Text), Convert.ToInt16(this.txtp8.Text),
                                                                       Convert.ToSingle(this.txtp9.Text), Convert.ToSingle(this.txtp10.Text), Convert.ToInt16(this.txtp11.Text), Convert.ToInt16(this.txtp12.Text),
                                                                       Convert.ToSingle(this.txtp13.Text), Convert.ToInt16(this.txtp14.Text));
                        break;
                    }
                case 4:
                    {
                        this.driver.RunShutter(Convert.ToByte(this.txtp1.Text));
                        break;
                    }
                case 5:
                    {
                        this.driver.RunDrawerWeightRoutine(Convert.ToInt16(this.txtp1.Text), Convert.ToSingle(this.txtp2.Text), Convert.ToSingle(this.txtp3.Text), Convert.ToByte(this.txtp4.Text));
                        break;
                    }
                case 6:
                    {
                        this.driver.GetDrawerWeight(Convert.ToSingle(this.txtp1.Text));
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
                        this.driver.Set(Convert.ToInt32(this.txtp1.Text), Convert.ToByte(this.txtp2.Text));
                        break;
                    }
            }
        }

        /// <summary>
        /// Connect to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connect_Click(Object sender, RoutedEventArgs e)
        {
            // Assign the IP address to connect
            this.driver.IPAddressToConnect = this.txtIP.Text;
            this.driver.PortAddressToConnect = Convert.ToInt32(this.txtPort.Text);

            // Initialize the inverter driver: driver is connect
            var bResult = this.driver.Initialize();
            this.Connect_Button.IsEnabled = false;
        }

        /// <summary>
        /// Disconnect from server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Disconnect_Click(Object sender, RoutedEventArgs e)
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
            var szMessage = eventArgs.Message;
            this.Dispatcher.BeginInvoke(new Action(() => this.updateUI(cmdId, szMessage)));
        }

        private void PreviewTextInput_txtp1(Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void PreviewTextInput_txtp10(Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void PreviewTextInput_txtp11(Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void PreviewTextInput_txtp12(Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void PreviewTextInput_txtp13(Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void PreviewTextInput_txtp14(Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void PreviewTextInput_txtp2(Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void PreviewTextInput_txtp3(Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void PreviewTextInput_txtp4(Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void PreviewTextInput_txtp5(Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void PreviewTextInput_txtp6(Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void PreviewTextInput_txtp7(Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void PreviewTextInput_txtp8(Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void PreviewTextInput_txtp9(Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void updateUI(bool State)
        {
            this.tbStatus.Text = (State) ? "Connected" : "Not connected";
        }

        private void updateUI(CommandId cmdId, string szMessage)
        {
            this.tbReceivedMsg.Text = String.Format("Command id: {0} Text: {1}", cmdId.ToString(), szMessage);
        }

        #endregion Methods
    }
}

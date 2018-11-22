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
using Ferretto.VW.InverterDriver;
using Ferretto.VW.ActionBlocks;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private InverterDriver inverterDriver;
        private CalibrateVerticalAxis calibrateVA;

        public MainWindow()
        {
            this.InitializeComponent();
            this.inverterDriver = new InverterDriver();
            this.inverterDriver.Connected += this.InverterDriver_Connected;

        }

        private void InverterDriver_Connected(Object sender, ConnectedEventArgs eventArgs)
        {
            var bConnected = eventArgs.State;
            if (bConnected)
            {
                this.lbStateConnection.Content = "State: connected to inverter";
            }
            else
            {
                this.lbStateConnection.Content = "State: not connected to inverter";
            }
        }

        private void Button_Click(Object sender, RoutedEventArgs e)
        {
            // At this time 1 is the default value for each variable
            int m = 5;
            short ofs = 1;
            short vFast = 1;
            short vCreep = 1;

            // bool bResult = false;

            // BResult is true only when the Server is started, we can't run Initialize in the MainWindow
            //bResult = this.inverterDriver.Initialize();

            //if (bResult)
            //{
                //this.calibrateVA.Initialize();
                this.calibrateVA.SetVAxisOrigin(m, ofs, vFast, vCreep);
            //}
        }

        private void CatchError (CalibrationStatus ErrorDescription)
        {
            this.TextBoxerrors.Text = ErrorDescription.ToString();
        }

        //private void ConnectionDone(bool connectionDone)
        //{
        //    if (connectionDone)
        //    {
        //        this.btnSend.IsEnabled = true;
        //    }
        //}

        private void btnCreateVerticalAxisClass_Click(Object sender, RoutedEventArgs e)
        {
            this.calibrateVA = new CalibrateVerticalAxis();
            this.calibrateVA.SetInverterDriverInterface = this.inverterDriver;
            this.calibrateVA.Initialize();

            this.calibrateVA.ThrowErrorEvent += new CalibrateVerticalAxis.ErrorEventHandler(this.CatchError);
        }

        private void btnConnectInverter_Click(Object sender, RoutedEventArgs e)
        {
            var success = this.inverterDriver.Initialize();
        }

        private void btnStop_Click(Object sender, RoutedEventArgs e)
        {
            this.calibrateVA.Stop();
        }
    }
}

using System;
using System.Windows;
using Ferretto.VW.ActionBlocks;
using Ferretto.VW.InverterDriver;

namespace UIVHATest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private InverterDriver inverterDriver;
        private CalibrateHorizontalAxis calibrateHA;
        
        public MainWindow()
        {
            this.InitializeComponent();
            this.inverterDriver = new InverterDriver();
            this.inverterDriver.Connected += this.InverterDriver_Connected;
        }

        private void InverterDriver_Connected(Object sender, ConnectedEventArgs eventArgs)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.lbStateConnection.Content = eventArgs.State ? "Connected to inverter" : "Not connected to inverter";
            });
        }

        private void BtnConnectInverter_Click(Object sender, RoutedEventArgs e)
        {
            var success = this.inverterDriver.Initialize();
        }

        private void BtnSend_Click(Object sender, RoutedEventArgs e)
        {
            // At this time 1 is the default value for each variable
            int m = 5;
            short ofs = 1;
            short vFast = 1;
            short vCreep = 1;

            this.calibrateHA.SetHAxisOrigin(m, ofs, vFast, vCreep);
        }

        private void BtnCreateHorizontalAxisClass_Click(Object sender, RoutedEventArgs e)
        {
            this.calibrateHA = new CalibrateHorizontalAxis();
            this.calibrateHA.SetInverterDriverInterface = this.inverterDriver;
            this.calibrateHA.Initialize();
            //this.calibrateHA.ThrowEndEvent += new CalibrateHorizontalAixsEndedEventHandler(this.Calibration);
            //this.calibrateHA.ThrowErrorEvent += new CalibrateHorizontalAxisErrorEventHandler(this.CatchError);

            this.calibrateHA.ThrowErrorEvent += this.CatchError;
            this.calibrateHA.ThrowEndEvent += this.CalibrationEnded;

            this.Dispatcher.Invoke(() =>
            {
                this.lbStateConnection.Content = "Class Created";
            });
        }

        private void CatchError(CalibrationStatus errorDescription)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.TextMessagesBox.Text = errorDescription.ToString();
            });
        }

        private void CalibrationEnded(bool result)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.TextMessagesBox.Text = result ? "Horizontal calibration ended" : "Horizontal calibration failed";
            });

            StopInverter();
        }

        private void btnStop_Click(Object sender, RoutedEventArgs e)
        {
            StopInverter();
        }

        private void StopInverter()
        {
            bool result = this.calibrateHA.StopInverter();

            this.Dispatcher.Invoke(() =>
            {
                this.lbStateConnection.Content = result ? "Horizontal Calibration stopped" : "Horizontal Calibration not stopped";
            });
        }
    }
}

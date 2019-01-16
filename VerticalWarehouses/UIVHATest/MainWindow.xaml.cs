using System;
using System.Windows;
using Ferretto.VW.ActionBlocks;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.RemoteIODriver;
using NLog;

namespace UIVHATest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private InverterDriver inverterDriver; // Instance
        private CalibrateAxes calibrateAxes;   // Instance
        private IRemoteIO remoteIO;            // Interface - Da istanziare
        
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // Logger

        public MainWindow()
        {
            this.InitializeComponent();

            logger.Log(LogLevel.Debug, "UICHATest application new starting...");

            btnSend.IsEnabled = false;
            btnStop.IsEnabled = false;

            // Inverter Driver instantiation
            this.inverterDriver = new InverterDriver();
            this.inverterDriver.Connected += this.InverterDriver_Connected;

            // RemoteIO
            this.remoteIO = new RemoteIO();
            this.remoteIO.Connect();
        }

        private void InverterDriver_Connected(Object sender, ConnectedEventArgs eventArgs)
        {
            logger.Log(LogLevel.Debug, "Inverter Driver Connected");

            this.Dispatcher.Invoke(() =>
            {
                this.lbStateConnection.Content = eventArgs.State ? "Connected to inverter" : "Not connected to inverter";
            });
        }

        private void BtnConnectInverter_Click(Object sender, RoutedEventArgs e)
        {
            var success = this.inverterDriver.Initialize();

            this.Dispatcher.Invoke(() =>
            {
                this.lbStateConnection.Content = success ? "InverterDriver istanziato" : "InverterDriver non istanziato";
            });
        }

        private void BtnStart_Click(Object sender, RoutedEventArgs e)
        {
            // At this time 1 is the default value for each variable
            int m = 5;
            short ofs = 1;
            short vFast = 1;
            short vCreep = 1;

            logger.Log(LogLevel.Debug, "PUSHED BTN START!!!");
            logger.Log(LogLevel.Debug, "Start calibration ...");
            this.calibrateAxes.SetAxesOrigin(m, ofs, vFast, vCreep);

            this.Dispatcher.Invoke(() =>
            {
                btnStop.IsEnabled = true;
                this.lbStateConnection.Content = "";
                this.TextMessagesBox.Text = "Start calibration ...";
                this.TextMessagesBoxProgressive.Text = "";
            });
        }

        private void BtnCreateAxesClass_Click(Object sender, RoutedEventArgs e)
        {
            this.calibrateAxes = new CalibrateAxes();
            this.calibrateAxes.SetInverterDriverInterface = this.inverterDriver;           
            this.calibrateAxes.SetRemoteIOInterface = this.remoteIO;
            this.calibrateAxes.Initialize();
            
            this.calibrateAxes.ThrowErrorEvent += this.CatchError;
            this.calibrateAxes.ThrowEndEvent += this.CalibrationEnded;
            
            this.Dispatcher.Invoke(() =>
            {
                this.lbStateConnection.Content = "Class Created";
                btnSend.IsEnabled = true;
            });

            // Events subscription
            this.calibrateAxes.ThrowSwitchVerticalToHorizontalEndEvent += this.SwitchVerticalToHorizontalEndEvent;
            this.calibrateAxes.ThrowHorizontalCalibrationEndEvent += this.HorizontalCalibrationEndEvent;
            this.calibrateAxes.ThrowHorizontalToVerticalEndEvent += this.SwitchHorizontalToVerticalEndEvent;
            this.calibrateAxes.ThrowVerticalCalibrationEndEvent += this.VerticalCalibrationEndEvent;
        }

        private void CatchError(string errorDescription)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.TextMessagesBox.Text = errorDescription;
            });
        }

        private void CalibrationEnded()
        {
            logger.Log(LogLevel.Debug, "Complete homing attained.");

            this.Dispatcher.Invoke(() =>
            {
                this.TextMessagesBox.Text = "Complete homing attained.";
                this.TextMessagesBoxProgressive.Text = this.TextMessagesBoxProgressive.Text + "END!";
                btnStop.IsEnabled = false;
            });

            // this.calibrateAxes.Terminate();

            // Events subscription
            this.calibrateAxes.ThrowSwitchVerticalToHorizontalEndEvent -= this.SwitchVerticalToHorizontalEndEvent;
            this.calibrateAxes.ThrowHorizontalCalibrationEndEvent -= this.HorizontalCalibrationEndEvent;
            this.calibrateAxes.ThrowHorizontalToVerticalEndEvent -= this.SwitchHorizontalToVerticalEndEvent;
            this.calibrateAxes.ThrowVerticalCalibrationEndEvent -= this.VerticalCalibrationEndEvent;
        }

        private void btnStop_Click(Object sender, RoutedEventArgs e)
        {
            logger.Log(LogLevel.Debug, "PUSHED BTN STOP!!!");

            StopInverter();
        }

        private void StopInverter()
        {
            bool result = this.calibrateAxes.StopInverter();

            logger.Log(LogLevel.Debug, "Stop Inverter");

            this.Dispatcher.Invoke(() =>
            {
                this.lbStateConnection.Content = result ? "Horizontal Calibration stopped" : "Horizontal Calibration not stopped";
                this.TextMessagesBoxProgressive.Text = this.TextMessagesBoxProgressive.Text + "Stop";
            });
        }

        private void SwitchVerticalToHorizontalEndEvent()
        {
            logger.Log(LogLevel.Debug, "Switch V --> H");

            this.Dispatcher.Invoke(() =>
            {
                this.lbStateConnection.Content = "Switch Vertical to Horizontal ended";
                this.TextMessagesBoxProgressive.Text = this.TextMessagesBoxProgressive.Text + "SwitchVToH-";
            });
        }

        private void HorizontalCalibrationEndEvent(int stepCounter)
        {
            logger.Log(LogLevel.Debug, "H Calibration ended");
            logger.Log(LogLevel.Debug, "stepCounter = {0}", stepCounter);

            this.Dispatcher.Invoke(() =>
            {
                this.lbStateConnection.Content = "Horizontal Calibration ended";
                this.TextMessagesBoxProgressive.Text = this.TextMessagesBoxProgressive.Text + "HorCal-";
            });
        }

        private void VerticalCalibrationEndEvent()
        {
            logger.Log(LogLevel.Debug, "V Calibration ended");

            this.Dispatcher.Invoke(() =>
            {
                this.lbStateConnection.Content = "Vertical Calibration ended";
                this.TextMessagesBoxProgressive.Text = this.TextMessagesBoxProgressive.Text + "VerCal-";
            });
        }

        private void SwitchHorizontalToVerticalEndEvent()
        {
            logger.Log(LogLevel.Debug, "Switch H --> V");

            this.Dispatcher.Invoke(() =>
            {
                this.lbStateConnection.Content = "Switch Horizontal to Vertical ended";
                this.TextMessagesBoxProgressive.Text = this.TextMessagesBoxProgressive.Text + "SwitchHToV-";
            });
        }
    }
}

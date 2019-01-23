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
            // We assign the half value of the actual parameters we have in V-Plus
            int acc = 500;
            int vFast = 500;
            int vCreep = 40;

            logger.Log(LogLevel.Debug, "PUSHED BTN START!!!");
            logger.Log(LogLevel.Debug, "Start calibration ...");
            this.calibrateAxes.SetAxesOrigin(acc, vFast, vCreep);

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
            this.calibrateAxes.ThrowStopEvent += this.CatchStopCalibration;


            this.Dispatcher.Invoke(() =>
            {
                this.lbStateConnection.Content = "Class Created";
                btnSend.IsEnabled = true;
            });

            // Events subscription
            this.calibrateAxes.ThrowSwitchVerticalToHorizontalEndEvent += this.SwitchVerticalToHorizontalEndEvent;
            this.calibrateAxes.ThrowCalibrationEndEvent += this.CalibrationEndEvent;
            this.calibrateAxes.ThrowHorizontalToVerticalEndEvent += this.HorizontalToVerticalEndEvent;
            this.calibrateAxes.ThrowSetUpVerticalHomingEndEvent += this.SetUpVerticalHomingEndEvent;
        }

        private void CatchError(string errorDescription)
        {
            logger.Log(LogLevel.Debug, "Catch Error");

            this.Dispatcher.Invoke(() =>
            {
                this.TextMessagesBox.Text = errorDescription;
            });
        }

        private void CatchStopCalibration(string stopMessage)
        {
            logger.Log(LogLevel.Debug, "Stop Calibration");

            this.Dispatcher.Invoke(() =>
            {
                this.TextMessagesBoxProgressive.Text = this.TextMessagesBoxProgressive.Text + "Stop";
                this.lbStateConnection.Content = stopMessage;
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

            // Events subscription
            this.calibrateAxes.ThrowSwitchVerticalToHorizontalEndEvent -= this.SwitchVerticalToHorizontalEndEvent;
            this.calibrateAxes.ThrowCalibrationEndEvent -= this.CalibrationEndEvent;
            this.calibrateAxes.ThrowHorizontalToVerticalEndEvent -= this.HorizontalToVerticalEndEvent;
            this.calibrateAxes.ThrowStopEvent -= this.CatchStopCalibration;
        }

        private void btnStop_Click(Object sender, RoutedEventArgs e)
        {
            logger.Log(LogLevel.Debug, "PUSHED BTN STOP!!!");

            StopInverter();
        }

        private void StopInverter()
        {
            this.calibrateAxes.StopInverter();

            logger.Log(LogLevel.Debug, "Stop Inverter");
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

        private void CalibrationEndEvent(string calibrationEndAxis)
        {
            logger.Log(LogLevel.Debug, calibrationEndAxis);

            this.Dispatcher.Invoke(() =>
            {
                this.lbStateConnection.Content = "Vertical Calibration ended";
                this.TextMessagesBoxProgressive.Text = this.TextMessagesBoxProgressive.Text + "VerOrHorCal-";
            });
        }

        private void HorizontalToVerticalEndEvent()
        {
            logger.Log(LogLevel.Debug, "Switch H --> V");

            this.Dispatcher.Invoke(() =>
            {
                this.lbStateConnection.Content = "Switch Horizontal to Vertical ended";
                this.TextMessagesBoxProgressive.Text = this.TextMessagesBoxProgressive.Text + "SwitchHToV-";
            });
        }

        private void SetUpVerticalHomingEndEvent()
        {
            logger.Log(LogLevel.Debug, "SetUp Vertical Homing");

            this.Dispatcher.Invoke(() =>
            {
                this.lbStateConnection.Content = "SetUp Vertical Homing parameters ended";
                this.TextMessagesBoxProgressive.Text = this.TextMessagesBoxProgressive.Text + "SetUpVP-";
            });
        }
    }
}

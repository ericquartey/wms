using System;
using System.Windows;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.ActionBlocks;

namespace UICVA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private InverterDriver inverterDriver;
        private CalibrateVerticalAxis calibrateVA;
        private PositioningDrawer positioningDrawer;

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

        private void Calibration(bool result)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (result)
                {
                    this.TextMessages.Text = "Homing done";
                }
                else
                {
                    this.TextMessages.Text = "Homing failed";
                }
            });
        }

        private void Button_Click(Object sender, RoutedEventArgs e)
        {
            // At this time 1 is the default value for each variable
            int m = 5;
            short ofs = 1;
            short vFast = 1;
            short vCreep = 1;

            this.Dispatcher.Invoke(() =>
            {
                this.TextMessages.Text = "Homing started";
            });

            this.calibrateVA.SetVAxisOrigin(m, ofs, vFast, vCreep);
        }
        private void btnCreateVerticalAxisClass_Click(Object sender, RoutedEventArgs e)
        {
            this.calibrateVA = new CalibrateVerticalAxis();
            this.calibrateVA.SetInverterDriverInterface = this.inverterDriver;
            this.calibrateVA.Initialize();

            this.calibrateVA.ThrowEndEvent += new CalibrateVerticalAxis.CalibrationEndedEventHandler(this.Calibration);
            this.calibrateVA.ThrowErrorEvent += new CalibrateVerticalAxis.ErrorEventHandler(this.CatchError);
        }

        private void btnConnectInverter_Click(Object sender, RoutedEventArgs e)
        {
            var success = this.inverterDriver.Initialize();

            if (!success)
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.TextMessages.Text = "Connection failed";
                });
            }
        }

        private void CatchError(CalibrationStatus errorStatus)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.TextMessages.Text = errorStatus.ToString();
            });
        }

        private void btnStopInverter_Click(Object sender, RoutedEventArgs e)
        {
            string opStopped = "None operation stopped";

            if (this.calibrateVA!=null)
            { 
                this.calibrateVA.StopInverter();

                opStopped = "Calibration vertical axis stopped";
            }

            if (this.positioningDrawer != null)
            {
                this.positioningDrawer.StopInverter();

                opStopped = "Vertical positioning stopped";
            }

            this.Dispatcher.Invoke(() =>
            {
                this.TextMessages.Text = opStopped;
            });
        }

        private void btnVerticalPos_Click(Object sender, RoutedEventArgs e)
        {
            this.positioningDrawer = new PositioningDrawer();
            this.positioningDrawer.SetInverterDriverInterface = this.inverterDriver;
            this.positioningDrawer.Initialize();

            this.positioningDrawer.MoveAlongVerticalAxisToPointDone_Event += new PositioningDrawer.MoveAlongVerticalAxisToPointDoneEventHandler(this.PositioningDone);
            this.positioningDrawer.Error_Event += new PositioningDrawer.ErrorEventHandler(this.CatchErrorPositioning);

            // Da rimuovere per il rilascio
            this.positioningDrawer.ReadCurrentPosition_Event += new PositioningDrawer.ReadCurrentPositionEventHandler(this.ShowCurrentPosition);
        }

        private void btnCreatePositioning_Click(Object sender, RoutedEventArgs e)
        {
            short x = 1;
            float vMax = 1;
            float acc = 1;
            float dec = 1;
            float w = 1;
            short offset = 1;

            this.Dispatcher.Invoke(() =>
            {
                this.TextMessages.Text = "Vertical positioning started";
            });

            this.positioningDrawer.MoveAlongVerticalAxisToPoint(x, vMax, acc, dec, w, offset);
        }

        private void PositioningDone(bool result)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (result)
                {
                    this.TextMessages.Text = "Positioning done";
                }
                else
                {
                    this.TextMessages.Text = "Positioning failed";
                }
            });
        }

        private void CatchErrorPositioning(string errorMessage)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.TextMessages.Text = errorMessage;
            });
        }

        private void btnHaltPositioning_Click(Object sender, RoutedEventArgs e)
        {
            this.positioningDrawer.HaltInverter();

            this.Dispatcher.Invoke(() =>
            {
                this.TextMessages.Text = "Vertical positioning halted";
            });
        }

        // da rimuovere prima del rilascio
        private void btnCurrentPosition_Click(Object sender, RoutedEventArgs e)
        {
            this.positioningDrawer.CurrentPosition();
        }

        private void ShowCurrentPosition (float currentPosition)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.CurrentPosition.Text = currentPosition.ToString();
            });
        }

        private void btnNewPosition_Click(Object sender, RoutedEventArgs e)
        {
            this.positioningDrawer.SetNewPosition(this.NewPosition.Text);
        }
    }
}

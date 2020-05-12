using System;
using System.Configuration;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Ferretto.VW.CommonUtils.Enumerations;
using Prism.Mvvm;

namespace Ferretto.VW.App.Services
{
    public class ShutterSensors : BindableBase
    {
        #region Fields

        private int bayNumber;

        private bool closed;

        private bool midWay;

        private bool open;

        private string status;

        #endregion

        #region Constructors

        public ShutterSensors()
        {
        }

        #endregion

        #region Properties

        public bool Closed { get => this.closed; set => this.SetProperty(ref this.closed, value); }

        public bool MidWay { get => this.midWay; set => this.SetProperty(ref this.midWay, value); }

        public bool Open { get => this.open; set => this.SetProperty(ref this.open, value); }

        public string Status { get => this.status; set => this.SetProperty(ref this.status, value); }

        #endregion

        #region Methods

        public void Update(bool[] sensorStates, int bayNumber)
        {
            this.bayNumber = bayNumber;
            this.Update(sensorStates);
        }

        public void Update(bool[] sensorStates)
        {
            if (this.bayNumber == 0)
            {
                return;
            }

            if (sensorStates is null)
            {
                return;
            }

            // Workaround: handle shutters with old true table (IO mapping rev3) via
            var useOldTrueTable = ConfigurationManager.AppSettings.UseOldTrueTableForShutter();

            switch (this.bayNumber)
            {
                case 1:
                    {
                        var maxArrayIndex = System.Math.Max(
                            (int)IOMachineSensors.AGLSensorAShutterBay1,
                            (int)IOMachineSensors.AGLSensorBShutterBay1);

                        if (sensorStates.Length > maxArrayIndex)
                        {
                            this.Open = (useOldTrueTable) ?
                                sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay1] && sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay1] :
                                !sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay1] && sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay1];
                            this.Closed = !sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay1] && !sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay1];
                            this.MidWay = sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay1] && !sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay1];
                        }

                        break;
                    }

                case 2:
                    {
                        var maxArrayIndex = System.Math.Max(
                            (int)IOMachineSensors.AGLSensorAShutterBay2,
                            (int)IOMachineSensors.AGLSensorBShutterBay2);

                        if (sensorStates.Length > maxArrayIndex)
                        {
                            this.Open = (useOldTrueTable) ?
                                sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay2] && sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay2] :
                                !sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay2] && sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay2];
                            this.Closed = !sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay2] && !sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay2];
                            this.MidWay = sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay2] && !sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay2];
                        }

                        break;
                    }

                case 3:
                    {
                        var maxArrayIndex = System.Math.Max(
                            (int)IOMachineSensors.AGLSensorAShutterBay3,
                            (int)IOMachineSensors.AGLSensorBShutterBay3);

                        if (sensorStates.Length > maxArrayIndex)
                        {
                            this.Open = (useOldTrueTable) ?
                                sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay3] && sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay3] :
                                !sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay3] && sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay3];
                            this.Closed = !sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay3] && !sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay3];
                            this.MidWay = sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay3] && !sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay3];
                        }

                        break;
                    }
            }

            if (this.Open)
            {
                this.Status = Resources.Localized.Get("InstallationApp.Open");
            }
            else if (this.MidWay)
            {
                this.Status = Resources.Localized.Get("InstallationApp.ShutterMidWay");
            }
            else if (this.Closed)
            {
                this.Status = Resources.Localized.Get("InstallationApp.Closed");
            }
            else
            {
                this.Status = string.Empty;
            }
        }

        #endregion
    }
}

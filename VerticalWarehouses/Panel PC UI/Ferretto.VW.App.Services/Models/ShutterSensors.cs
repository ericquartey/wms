using Ferretto.VW.CommonUtils.Enumerations;
using Prism.Mvvm;

namespace Ferretto.VW.App.Services
{
    public class ShutterSensors : BindableBase
    {
        #region Fields

        private readonly int bayNumber;

        private bool closed;

        private bool midWay;

        private bool open;

        #endregion

        #region Constructors

        public ShutterSensors(int bayNumber)
        {
            this.bayNumber = bayNumber;
        }

        #endregion

        #region Properties

        public bool Closed { get => this.closed; set => this.SetProperty(ref this.closed, value); }

        public bool MidWay { get => this.midWay; set => this.SetProperty(ref this.midWay, value); }

        public bool Open { get => this.open; set => this.SetProperty(ref this.open, value); }

        #endregion

        #region Methods

        public void Update(bool[] sensorStates)
        {
            if (sensorStates is null)
            {
                return;
            }

            switch (this.bayNumber)
            {
                case 1:
                    {
                        var maxArrayIndex = System.Math.Max(
                            (int)IOMachineSensors.AGLSensorAShutterBay1,
                            (int)IOMachineSensors.AGLSensorBShutterBay1);

                        if (sensorStates.Length > maxArrayIndex)
                        {
                            this.Open = sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay1] && sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay1];
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
                            this.Open = sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay2] && sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay2];
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
                            this.Open = sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay3] && sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay3];
                            this.Closed = !sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay3] && !sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay3];
                            this.MidWay = sensorStates[(int)IOMachineSensors.AGLSensorAShutterBay3] && !sensorStates[(int)IOMachineSensors.AGLSensorBShutterBay3];
                        }

                        break;
                    }
            }
        }

        #endregion
    }
}

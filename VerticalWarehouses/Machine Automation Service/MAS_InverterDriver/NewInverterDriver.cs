using Ferretto.VW.InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface;
using Prism.Events;

namespace Ferretto.VW.MAS_InverterDriver
{
    public delegate void EndEventHandler();

    public delegate void ErrorEventHandler();

    public partial class NewInverterDriver : INewInverterDriver
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IInverterDriver inverterDriver;

        private IInverterActions inverterAction;

        #endregion

        #region Constructors

        public NewInverterDriver(IEventAggregator eventAggregator, IInverterDriver inverterDriver)
        {
            this.inverterDriver = inverterDriver;
            this.eventAggregator = eventAggregator;
            this.inverterDriver.Initialize();
        }

        #endregion

        #region Methods

        public void Destroy()
        {
            this.inverterDriver.Terminate();
        }

        public bool[] GetSensorsStates()
        {
            if (null == this.inverterDriver) return null;

            var sensors = new bool[5];
            sensors[0] = this.inverterDriver.Brake_Resistance_Overtemperature;
            sensors[1] = this.inverterDriver.Emergency_Stop;
            sensors[2] = this.inverterDriver.Pawl_Sensor_Zero;
            sensors[3] = this.inverterDriver.Udc_Presence_Cradle_Machine;
            sensors[4] = this.inverterDriver.Udc_Presence_Cradle_Operator;

            return sensors;
        }

        #endregion
    }
}

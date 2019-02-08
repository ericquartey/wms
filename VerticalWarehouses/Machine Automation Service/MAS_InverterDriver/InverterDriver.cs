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

        private InverterDriver.InverterDriver driver;

        private IInverterActions inverterAction;

        #endregion

        #region Constructors

        public NewInverterDriver(IEventAggregator eventAggregator)
        {
            this.driver.Initialize();
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        public void Destroy()
        {
            this.driver.Terminate();
        }

        public bool[] GetSensorsStates()
        {
            if (null == this.driver)
            {
                return null;
            }

            var sensors = new bool[5];
            sensors[0] = this.driver.Brake_Resistance_Overtemperature;
            sensors[1] = this.driver.Emergency_Stop;
            sensors[2] = this.driver.Pawl_Sensor_Zero;
            sensors[3] = this.driver.Udc_Presence_Cradle_Machine;
            sensors[4] = this.driver.Udc_Presence_Cradle_Operator;

            return sensors;
        }

        #endregion
    }
}

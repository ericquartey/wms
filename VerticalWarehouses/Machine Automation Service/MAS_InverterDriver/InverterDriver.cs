using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface;
using Prism.Events;
using Ferretto.VW.MAS_InverterDriver.ActionBlocks;

namespace Ferretto.VW.MAS_InverterDriver
{
    public delegate void EndEventHandler();
    public delegate void ErrorEventHandler();

    public partial class InverterDriver : IInverterDriver
    {
        #region Fields
        
        private Ferretto.VW.InverterDriver.InverterDriver driver;
        private readonly IEventAggregator eventAggregator;
        private IInverterActions inverterAction;

        #endregion Fields

        #region Constructors

        public InverterDriver(IEventAggregator eventAggregator)
        {

            this.driver = new VW.InverterDriver.InverterDriver();
            this.driver.Initialize();
            this.eventAggregator = eventAggregator;
        }

        #endregion Constructors

        #region Methods

        public void Destroy()
        {
            this.driver.Terminate();
        }

        public void ExecuteHorizontalHoming()
        {
            return;
        }

        public void ExecuteHorizontalPosition()
        {
            return;
        }

        public void ExecuteVerticalPosition(int target, float weight)
        {
            return;
        }

        public float GetDrawerWeight()
        {
            return 0.0f;
        }

        public bool[] GetSensorsStates()
        {
            return null;
        }

        #endregion Methods
    }
}

using Ferretto.VW.MAS_IODriver.StateMachines.SetConfiguration;

namespace Ferretto.VW.MAS_IODriver
{
   public partial class IoDevice
    {
        #region Methods

        public void ExecuteSetConfiguration()
        {
            this.currentStateMachine = new SetConfigurationStateMachine(this.ioCommandQueue, this.ioSHDStatus, this.eventAggregator, this.logger);
            this.currentStateMachine.Start();
        }

        #endregion
    }
}

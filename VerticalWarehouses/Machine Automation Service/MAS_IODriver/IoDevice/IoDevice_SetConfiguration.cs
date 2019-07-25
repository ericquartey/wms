using Ferretto.VW.MAS.IODriver.StateMachines.SetConfiguration;

namespace Ferretto.VW.MAS.IODriver.IoDevice
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

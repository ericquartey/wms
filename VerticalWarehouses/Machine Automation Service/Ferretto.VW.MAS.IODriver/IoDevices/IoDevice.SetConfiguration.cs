using Ferretto.VW.MAS.IODriver.StateMachines.SetConfiguration;

namespace Ferretto.VW.MAS.IODriver.IoDevices
{
    public partial class IoDevice
    {
        #region Methods

        public void ExecuteSetConfiguration()
        {
            this.CurrentStateMachine = new SetConfigurationStateMachine(
                this.ioCommandQueue,
                this.ioStatus,
                this.deviceIndex,
                this.eventAggregator,
                this.logger);
            this.CurrentStateMachine.Start();
        }

        #endregion
    }
}

using Ferretto.VW.MAS_IODriver.StateMachines.Reset;

namespace Ferretto.VW.MAS_IODriver
{
    public partial class HostedSHDIoDriver
    {
        #region Methods

        private void ExecuteIoReset()
        {
            this.currentStateMachine = new ResetStateMachine(this.ioCommandQueue, this.ioSHDStatus, this.eventAggregator, this.logger);
            this.currentStateMachine.Start();
        }

        #endregion
    }
}

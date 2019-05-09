using Ferretto.VW.MAS_IODriver.StateMachines.Reset;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver
{
    public partial class HostedIoDriver
    {
        #region Methods

        private void ExecuteIoReset()
        {
            this.currentStateMachine = new ResetStateMachine(this.ioCommandQueue, this.eventAggregator, this.logger);
            this.currentStateMachine.Start();
        }

        #endregion
    }
}

using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_IODriver.StateMachines.Reset;

namespace Ferretto.VW.MAS_IODriver
{
    public partial class HostedIoDriver
    {
        #region Methods

        private void ExecuteIOReset(CommandMessage receivedMessage)
        {
            this.currentStateMachine = new ResetStateMachine(this.ioCommandQueue, this.eventAggregator, this.logger);
            this.currentStateMachine.Start();
        }

        #endregion
    }
}

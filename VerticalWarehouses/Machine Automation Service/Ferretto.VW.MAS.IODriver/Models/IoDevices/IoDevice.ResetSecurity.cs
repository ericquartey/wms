using System;
using Ferretto.VW.MAS.IODriver.StateMachines.ResetSecurity;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver
{
    internal partial class IoDevice
    {
        #region Methods

        public void ExecuteResetSecurity()
        {
            if (this.CurrentStateMachine != null)
            {
                this.logger.LogInformation($"Io Driver already executing operation {this.CurrentStateMachine.GetType().Name}");

                var ex = new Exception();
                this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "Io Driver already executing operation", 0));
            }
            else
            {
                this.CurrentStateMachine = new ResetSecurityStateMachine(
                    this.ioCommandQueue,
                    this.ioStatus,
                    this.mainIoDevice,
                    this.deviceIndex,
                    this.eventAggregator,
                    this.logger,
                    this.serviceScopeFactory);
                lock (this.syncAccess)
                {
                    this.commandExecuting = true;
                }

                this.CurrentStateMachine.Start();
            }
        }

        #endregion
    }
}

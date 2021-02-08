using System;
using Ferretto.VW.MAS.IODriver.StateMachines.Reset;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver
{
    internal partial class IoDevice
    {
        #region Methods

        public void ExecuteIoReset()
        {
            if (this.CurrentStateMachine != null)
            {
                this.logger.LogInformation($"Io Driver already executing operation {this.CurrentStateMachine.GetType().Name}");

                this.SendOperationErrorMessage(new IoExceptionFieldMessageData(null, "Io Driver already executing operation", 0));
            }
            else
            {
                this.CurrentStateMachine = new ResetStateMachine(
                    this.ioCommandQueue,
                    this.ioStatus,
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

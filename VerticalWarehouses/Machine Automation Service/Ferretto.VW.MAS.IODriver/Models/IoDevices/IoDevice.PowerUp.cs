using System;
using Ferretto.VW.MAS.IODriver.StateMachines.PowerUp;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver
{
    internal partial class IoDevice
    {
        #region Methods

        public void ExecuteIoPowerUp()
        {
            if (this.CurrentStateMachine != null)
            {
                this.logger.LogInformation($"Io Driver already executing operation {this.CurrentStateMachine.GetType().Name}");

                var ex = new Exception();
                this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "Io Driver already executing operation", 0));
            }
            else
            {
                this.CurrentStateMachine = new PowerUpStateMachine(
                    this.ioCommandQueue,
                    this.ioStatus,
                    this.deviceIndex,
                    this.eventAggregator,
                    this.logger,
                    this.serviceScopeFactory);

                this.CurrentStateMachine.Start();
            }
        }

        #endregion
    }
}

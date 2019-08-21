using System;
using Ferretto.VW.MAS.IODriver.StateMachines.PowerUp;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.IoDevices
{
    public partial class IoDevice
    {


        #region Methods

        public void ExecuteIoPowerUp()
        {
            if (this.CurrentStateMachine != null)
            {
                this.logger.LogInformation($"Io Driver already executing operation {this.CurrentStateMachine.GetType()}");

                var ex = new Exception();
                this.SendMessage(new IoExceptionFieldMessageData(ex, "Io Driver already executing operation", 0));
            }
            else
            {
                this.CurrentStateMachine = new PowerUpStateMachine(this.ioCommandQueue, this.ioSHDStatus, this.index, this.eventAggregator, this.logger);
                this.CurrentStateMachine.Start();
            }
        }

        #endregion
    }
}

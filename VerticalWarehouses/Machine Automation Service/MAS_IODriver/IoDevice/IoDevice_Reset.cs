using System;
using Ferretto.VW.MAS.IODriver.StateMachines.Reset;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.IoDevice
{
    public partial class IoDevice
    {
        #region Methods

        public void ExecuteIoReset()
        {
            if (this.currentStateMachine != null)
            {
                this.logger.LogInformation($"Io Driver already executing operation {this.currentStateMachine.GetType()}");

                var ex = new Exception();
                this.SendMessage(new IoExceptionFieldMessageData(ex, "Io Driver already executing operation", 0));
            }
            else
            {
                this.currentStateMachine = new ResetStateMachine(this.ioCommandQueue, this.ioSHDStatus, this.eventAggregator, this.logger);
                this.currentStateMachine.Start();
            }
        }

        #endregion
    }
}

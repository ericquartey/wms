using System;
using Ferretto.VW.MAS.IODriver.StateMachines.Reset;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.IODriver.IoDevices
{
    public partial class IoDevice
    {
        #region Methods

        public void ExecuteIoReset()
        {
            if (this.CurrentStateMachine != null)
            {
                this.logger.LogInformation($"Io Driver already executing operation {this.CurrentStateMachine.GetType()}");

                var ex = new Exception();
                this.SendMessage(new IoExceptionFieldMessageData(ex, "Io Driver already executing operation", 0));
            }
            else
            {
                this.CurrentStateMachine = new ResetStateMachine(
                    this.ioCommandQueue,
                    this.ioStatus,
                    this.deviceIndex,
                    this.eventAggregator,
                    this.logger);

                this.CurrentStateMachine.Start();
            }
        }

        #endregion
    }
}

using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.IODriver.StateMachines.PowerEnable;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver
{
    internal partial class IoDevice
    {
        #region Methods

        public void ExecutePowerEnable(FieldCommandMessage receivedMessage)
        {
            if (receivedMessage.Data is IPowerEnableFieldMessageData powerEnableMessageData)
            {
                if (this.CurrentStateMachine != null)
                {
                    if (powerEnableMessageData.Enable)
                    {
                        this.logger.LogInformation($"Io Driver already executing operation {this.CurrentStateMachine.GetType().Name}");

                        var ex = new Exception();
                        this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "Io Driver already executing operation", 0));
                    }
                    else
                    {
                        // if Enable is false I have to turn off power immediately, even if another state machine is active
                        this.logger.LogInformation($"PowerEnable Off destroys active state machine {this.CurrentStateMachine.GetType().Name}");
                        this.DestroyStateMachine();
                    }
                }

                if (this.CurrentStateMachine == null)
                {
                    this.CurrentStateMachine = new PowerEnableStateMachine(
                        powerEnableMessageData.Enable,
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
            else
            {
                this.logger.LogTrace("2:Wrong message Data data type");
                var errorNotification = new FieldNotificationMessage(
                    receivedMessage.Data,
                    "Wrong message Data data type",
                    FieldMessageActor.Any,
                    FieldMessageActor.IoDriver,
                    FieldMessageType.SensorsChanged,
                    MessageStatus.OperationError,
                    (byte)this.deviceIndex,
                    ErrorLevel.Error);

                this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
            }
        }

        #endregion
    }
}

using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.IODriver.StateMachines.SwitchAxis;
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

        public void ExecuteSwitchAxis(FieldCommandMessage receivedMessage)
        {
            this.logger.LogTrace("1:Method Start");

            if (this.CurrentStateMachine != null)
            {
                this.logger.LogInformation($"Io Driver already executing operation {this.CurrentStateMachine.GetType().Name}");

                var ex = new Exception();
                this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "Io Driver already executing operation", 0));
            }
            else if (receivedMessage.Data is ISwitchAxisFieldMessageData switchAxisMessageData)
            {
                switch (switchAxisMessageData.AxisToSwitchOn)
                {
                    case Axis.Horizontal:
                        if (this.ioStatus.CradleMotorOn)
                        {
                            var endNotification = new FieldNotificationMessage(
                                receivedMessage.Data,
                                "Switch to Horizontal axis completed",
                                FieldMessageActor.IoDriver,
                                FieldMessageActor.IoDriver,
                                FieldMessageType.SwitchAxis,
                                MessageStatus.OperationEnd,
                                (byte)this.deviceIndex);

                            this.logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                            this.eventAggregator
                                .GetEvent<FieldNotificationEvent>()
                                .Publish(endNotification);
                        }
                        else
                        {
                            this.CurrentStateMachine = new SwitchAxisStateMachine(
                                Axis.Horizontal,
                                this.ioStatus.ElevatorMotorOn,
                                this.ioCommandQueue,
                                this.ioStatus,
                                this.deviceIndex,
                                this.bayNumber,
                                this.eventAggregator,
                                this.logger,
                                this.serviceScopeFactory);

                            this.logger.LogTrace("3:Method Start State Machine");
                            lock (this.syncAccess)
                            {
                                this.commandExecuting = true;
                            }

                            this.CurrentStateMachine.Start();
                        }

                        break;

                    case Axis.Vertical:
                        if (this.ioStatus.ElevatorMotorOn)
                        {
                            var endNotification = new FieldNotificationMessage(
                                receivedMessage.Data,
                                "Switch to Vertical axis completed",
                                FieldMessageActor.IoDriver,
                                FieldMessageActor.IoDriver,
                                FieldMessageType.SwitchAxis,
                                MessageStatus.OperationEnd,
                                (byte)this.deviceIndex);

                            this.logger.LogTrace($"4:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                            this.eventAggregator
                                .GetEvent<FieldNotificationEvent>()
                                .Publish(endNotification);
                        }
                        else
                        {
                            this.CurrentStateMachine = new SwitchAxisStateMachine(
                                Axis.Vertical,
                                this.ioStatus.CradleMotorOn,
                                this.ioCommandQueue,
                                this.ioStatus,
                                this.deviceIndex,
                                this.bayNumber,
                                this.eventAggregator,
                                this.logger,
                                this.serviceScopeFactory);

                            this.logger.LogDebug("5:Method Start State Machine");
                            lock (this.syncAccess)
                            {
                                this.commandExecuting = true;
                            }

                            this.CurrentStateMachine.Start();
                        }

                        break;

                    case Axis.HorizontalAndVertical:
                        if (receivedMessage.Destination == FieldMessageActor.IoDriver)
                        {
                            var errorNotification = new FieldNotificationMessage(
                                receivedMessage.Data,
                                "Invalid I/O operation",
                                FieldMessageActor.Any,
                                FieldMessageActor.IoDriver,
                                receivedMessage.Type,
                                MessageStatus.OperationError,
                                (byte)this.deviceIndex,
                                ErrorLevel.Error);

                            this.logger.LogTrace($"6:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                            this.eventAggregator
                                .GetEvent<FieldNotificationEvent>()
                                .Publish(errorNotification);
                        }

                        break;
                }
            }
        }

        #endregion
    }
}

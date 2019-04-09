using Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver
{
    public partial class HostedIoDriver
    {
        #region Methods

        private void ExecuteSwitchAxis(FieldCommandMessage receivedMessage)
        {
            this.logger.LogDebug("1:Method Start");

            if (receivedMessage.Data is ISwitchAxisFieldMessageData switchAxisMessageData)
            {
                switch (switchAxisMessageData.AxisToSwitchOn)
                {
                    case Axis.Horizontal:
                        if (this.ioStatus.CradleMotorOn)
                        {
                            var endNotification = new FieldNotificationMessage(receivedMessage.Data, "Switch to Horizontal axis completed", FieldMessageActor.Any,
                                FieldMessageActor.IoDriver, FieldMessageType.SwitchAxis, MessageStatus.OperationEnd);

                            this.logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                            this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(endNotification);
                        }
                        else
                        {
                            this.currentStateMachine = new SwitchAxisStateMachine(Axis.Horizontal, this.ioStatus.ElevatorMotorOn, this.ioCommandQueue, this.eventAggregator, this.logger);

                            this.logger.LogDebug("3:Method Start State Machine");

                            this.currentStateMachine.Start();
                        }

                        break;

                    case Axis.Vertical:
                        if (this.ioStatus.ElevatorMotorOn)
                        {
                            var endNotification = new FieldNotificationMessage(receivedMessage.Data, "Switch to Vertical axis completed", FieldMessageActor.Any,
                                FieldMessageActor.IoDriver, FieldMessageType.SwitchAxis, MessageStatus.OperationEnd);

                            this.logger.LogTrace($"4:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                            this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(endNotification);
                        }
                        else
                        {
                            this.currentStateMachine = new SwitchAxisStateMachine(Axis.Vertical, this.ioStatus.CradleMotorOn, this.ioCommandQueue, this.eventAggregator, this.logger);

                            this.logger.LogDebug("5:Method Start State Machine");

                            this.currentStateMachine.Start();
                        }

                        break;

                    case Axis.Both:
                        if (receivedMessage.Destination == FieldMessageActor.IoDriver)
                        {
                            var errorNotification = new FieldNotificationMessage(receivedMessage.Data,
                                "Invalid I/O operation", FieldMessageActor.Any,
                                FieldMessageActor.IoDriver, receivedMessage.Type, MessageStatus.OperationError,
                                ErrorLevel.Error);

                            this.logger.LogTrace($"6:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                            this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                        }

                        break;
                }
            }

            this.logger.LogDebug("7:Method End");
        }

        #endregion
    }
}

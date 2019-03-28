using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_IODriver
{
    public partial class HostedIoDriver
    {
        #region Methods

        private void ExecuteSwitchAxis(CommandMessage receivedMessage)
        {
            this.logger.LogDebug("1:Method Start");

            if (receivedMessage.Data is ISwitchAxisMessageData)
            {
                switch (((ISwitchAxisMessageData)receivedMessage.Data).AxisToSwitch)
                {
                    case Axis.Horizontal:
                        if (this.ioStatus.CradleMotorOn)
                        {
                            var endNotification = new NotificationMessage(receivedMessage.Data, "Switch to Horizontal axis completed", MessageActor.Any,
                                MessageActor.IODriver, MessageType.SwitchAxis, MessageStatus.OperationEnd);

                            this.logger.LogTrace(string.Format("2:{0}:{1}:{2}",
                                endNotification.Type,
                                endNotification.Destination,
                                endNotification.Status));

                            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(endNotification);
                        }
                        else
                        {
                            this.currentStateMachine = new SwitchAxisSateMachine(Axis.Horizontal, this.ioStatus.ElevatorMotorOn, this.ioCommandQueue, this.eventAggregator, this.logger);

                            this.logger.LogDebug("3:Method Start Horizontal Axis");

                            this.currentStateMachine.Start();
                        }

                        break;

                    case Axis.Vertical:
                        if (this.ioStatus.ElevatorMotorOn)
                        {
                            var endNotification = new NotificationMessage(receivedMessage.Data, "Switch to Vertical axis completed", MessageActor.Any,
                                MessageActor.IODriver, MessageType.SwitchAxis, MessageStatus.OperationEnd);

                            this.logger.LogTrace(string.Format("4:{0}:{1}:{2}",
                                endNotification.Type,
                                endNotification.Destination,
                                endNotification.Status));

                            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(endNotification);
                        }
                        else
                        {
                            this.currentStateMachine = new SwitchAxisSateMachine(Axis.Vertical, this.ioStatus.CradleMotorOn, this.ioCommandQueue, this.eventAggregator, this.logger);

                            this.logger.LogDebug("5:Method Start Vertical Axis");

                            this.currentStateMachine.Start();
                        }

                        break;

                    case Axis.Both:
                        if (receivedMessage.Destination == MessageActor.IODriver)
                        {
                            var errorNotification = new NotificationMessage(receivedMessage.Data,
                                "Invalid I/O operation", MessageActor.Any,
                                MessageActor.IODriver, receivedMessage.Type, MessageStatus.OperationError,
                                ErrorLevel.Error);

                            this.logger.LogTrace(string.Format("6:{0}:{1}:{2}",
                                errorNotification.Type,
                                errorNotification.Destination,
                                errorNotification.Status));

                            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(errorNotification);
                        }

                        break;
                }
            }

            this.logger.LogDebug("7:Method End");
        }

        #endregion
    }
}

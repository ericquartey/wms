using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis;

namespace Ferretto.VW.MAS_IODriver
{
    public partial class HostedIoDriver
    {
        #region Methods

        private void ExecuteSwitchAxis(CommandMessage receivedMessage)
        {
            if (receivedMessage.Data is ISwitchAxisMessageData)
            {
                switch (((ISwitchAxisMessageData)receivedMessage.Data).AxisToSwitch)
                {
                    case Axis.Horizontal:
                        if (this.ioStatus.CradleMotorOn)
                        {
                            var endNotification = new NotificationMessage(receivedMessage.Data, "Switch to Horizontal axis completed", MessageActor.Any,
                                MessageActor.IODriver, MessageType.SwitchAxis, MessageStatus.OperationEnd);
                            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(endNotification);
                        }
                        else
                        {
                            this.currentStateMachine = new SwitchAxisSateMachine(Axis.Horizontal, this.ioStatus.ElevatorMotorOn, this.ioCommandQueue, this.eventAggregator, this.logger);
                            this.currentStateMachine.Start();
                        }
                        break;

                    case Axis.Vertical:
                        if (this.ioStatus.ElevatorMotorOn)
                        {
                            var endNotification = new NotificationMessage(receivedMessage.Data, "Switch to Vertical axis completed", MessageActor.Any,
                                MessageActor.IODriver, MessageType.SwitchAxis, MessageStatus.OperationEnd);
                            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(endNotification);
                        }
                        else
                        {
                            this.currentStateMachine = new SwitchAxisSateMachine(Axis.Vertical, this.ioStatus.CradleMotorOn, this.ioCommandQueue, this.eventAggregator, this.logger);
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
                            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(errorNotification);
                        }

                        break;
                }
            }
        }

        #endregion
    }
}

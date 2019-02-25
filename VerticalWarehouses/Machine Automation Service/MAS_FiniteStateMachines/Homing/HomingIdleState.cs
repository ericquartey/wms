using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingIdleState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private readonly INewRemoteIODriver remoteIODriver;

        private StateMachineHoming parent;

        #endregion

        #region Constructors

        public HomingIdleState(StateMachineHoming parent, INewInverterDriver driver, INewRemoteIODriver remoteIODriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = driver;
            this.remoteIODriver = remoteIODriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            this.eventAggregator.GetEvent<RemoteIODriver_NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public string Type => "Homing Idle State";

        #endregion

        #region Methods

        public void MakeOperation()
        {
            this.remoteIODriver.SwitchVerticalToHorizontal();
        }

        public void NotifyMessage(CommandMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            this.eventAggregator.GetEvent<RemoteIODriver_NotificationEvent>().Unsubscribe(this.notifyEventHandler);

            var notifyEvent = new Notification_EventParameter(OperationType.Homing, OperationStatus.Stopped, "Homing stopped", Verbosity.Info);
            this.eventAggregator.GetEvent<FiniteStateMachines_NotificationEvent>().Publish(notifyEvent);
        }

        private void notifyEventHandler(Notification_EventParameter notification)
        {
            if (notification.OperationType == OperationType.SwitchVerticalToHorizontal)
            {
                switch (notification.OperationStatus)
                {
                    case OperationStatus.End:
                        {
                            this.parent.ChangeState(new HorizontalSwitchDoneState(this.parent, this.driver, this.remoteIODriver, this.data, this.eventAggregator));
                            this.parent.MakeOperation();
                            break;
                        }
                    case OperationStatus.Error:
                        {
                            this.parent.ChangeState(new HomingErrorState(this.parent, this.driver, this.remoteIODriver, this.data, this.eventAggregator));
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            this.eventAggregator.GetEvent<RemoteIODriver_NotificationEvent>().Unsubscribe(this.notifyEventHandler);
        }

        #endregion
    }
}

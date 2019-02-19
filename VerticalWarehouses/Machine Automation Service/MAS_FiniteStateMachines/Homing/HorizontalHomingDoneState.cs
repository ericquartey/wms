using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HorizontalHomingDoneState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private readonly INewRemoteIODriver remoteIODriver;

        private StateMachineHoming parent;

        #endregion

        #region Constructors

        public HorizontalHomingDoneState(StateMachineHoming parent, INewInverterDriver iDriver, INewRemoteIODriver remoteIODriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = iDriver;
            this.remoteIODriver = remoteIODriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            if (!this.parent.HorizontalHomingAlreadyDone)
            {
                this.eventAggregator.GetEvent<RemoteIODriver_NotificationEvent>().Subscribe(this.notifyEventHandler);
            }
            else
            {
                this.parent.ChangeState(new HomingDoneState(this.parent, this.driver, this.remoteIODriver, this.data, this.eventAggregator));
            }
        }

        #endregion

        #region Properties

        public string Type => "Horizontal homing done";

        #endregion

        #region Methods

        public void MakeOperation()
        {
            if (!this.parent.HorizontalHomingAlreadyDone)
            {
                this.parent.HorizontalHomingAlreadyDone = true;
                this.remoteIODriver.SwitchHorizontalToVertical();
            }
        }

        public void NotifyMessage(Event_Message message)
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
            if (notification.OperationType == OperationType.SwitchHorizontalToVertical)
            {
                switch (notification.OperationStatus)
                {
                    case OperationStatus.End:
                        {
                            this.parent.ChangeState(new VerticalSwitchDoneState(this.parent, this.driver, this.remoteIODriver, this.data, this.eventAggregator));
                            this.parent.MakeOperation();

                            break;
                        }
                    case OperationStatus.Error:
                        {
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

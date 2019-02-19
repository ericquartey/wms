using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingDoneState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private readonly INewRemoteIODriver remoteIODriver;

        private StateMachineHoming parent;

        #endregion

        #region Constructors

        public HomingDoneState(StateMachineHoming parent, INewInverterDriver driver, INewRemoteIODriver remoteIODriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = driver;
            this.remoteIODriver = remoteIODriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            this.parent.HomingComplete = true;

            this.eventAggregator.GetEvent<RemoteIODriver_NotificationEvent>().Subscribe(this.notifyEventHandler);

            this.remoteIODriver.SwitchHorizontalToVertical();
        }

        #endregion

        #region Properties

        public string Type => "Homing Done State";

        #endregion

        #region Methods

        public void MakeOperation()
        {
        }

        public void NotifyMessage(Event_Message message)
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
        }

        private void notifyEventHandler(Notification_EventParameter notification)
        {
            if (notification.OperationType == OperationType.SwitchHorizontalToVertical)
            {
                switch (notification.OperationStatus)
                {
                    case OperationStatus.End:
                        {
                            var notifyEvent = new Notification_EventParameter(OperationType.Homing, OperationStatus.End, "Homing done", Verbosity.Info);
                            this.eventAggregator.GetEvent<FiniteStateMachines_NotificationEvent>().Publish(notifyEvent);

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

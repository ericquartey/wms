using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HorizontalSwitchDoneState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private readonly INewRemoteIODriver remoteIODriver;

        private StateMachineHoming parent;

        #endregion

        #region Constructors

        public HorizontalSwitchDoneState(StateMachineHoming parent, INewInverterDriver iDriver, INewRemoteIODriver remoteIODriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = iDriver;
            this.remoteIODriver = remoteIODriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            this.eventAggregator.GetEvent<NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public string Type => "Horizontal Switch Done";

        #endregion

        #region Methods

        public void MakeOperation()
        {
            this.driver.ExecuteHorizontalHoming();
        }

        public void NotifyMessage(CommandMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            this.driver.ExecuteHomingStop();

            this.eventAggregator.GetEvent<NotificationEvent>().Unsubscribe(this.notifyEventHandler);

            var notifyEvent = new NotificationMessage( null, "Homing stopped", MessageActor.Any, MessageActor.FiniteStateMachines, MessageType.Homing, MessageStatus.OpeerationStop, MessageVerbosity.Info);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(notifyEvent);
        }

        private void notifyEventHandler(NotificationMessage notification)
        {
            switch (notification.Status)
            {
                case MessageStatus.OperationEnd:
                    {
                        if (notification.Description == "Horizontal Calibration Ended" && !this.parent.HomingComplete)
                        {
                            this.parent.ChangeState(new HorizontalHomingDoneState(this.parent, this.driver, this.remoteIODriver, this.data, this.eventAggregator));
                            this.parent.MakeOperation();
                        }
                        break;
                    }
                case MessageStatus.OperationError:
                    {
                        this.parent.ChangeState(new HomingErrorState(this.parent, this.driver, this.remoteIODriver, this.data, this.eventAggregator));
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            this.eventAggregator.GetEvent<NotificationEvent>().Unsubscribe(this.notifyEventHandler);
        }

        #endregion
    }
}

using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming
{
    public class VerticalHomingIdleState : IState
    {
        #region Fields

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineVerticalHoming parent;

        #endregion

        #region Constructors

        public VerticalHomingIdleState(StateMachineVerticalHoming parent, INewInverterDriver driver, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = driver;
            this.eventAggregator = eventAggregator;

            this.eventAggregator.GetEvent<NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public string Type => "Vertical Homing Idle State";

        #endregion

        #region Methods

        public void MakeOperation()
        {
            this.driver.ExecuteVerticalHoming();
        }

        public void NotifyMessage(CommandMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            this.driver.ExecuteHomingStop();

            this.eventAggregator.GetEvent<NotificationEvent>().Unsubscribe(this.notifyEventHandler);

            var notifyEvent = new NotificationMessage(null, "Homing stopped", MessageActor.Any, MessageActor.FiniteStateMachines, MessageType.Homing, MessageStatus.OpeerationStop, MessageVerbosity.Info);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(notifyEvent);
        }

        private void notifyEventHandler(NotificationMessage notification)
        {
            switch (notification.Status)
            {
                case MessageStatus.OperationEnd:
                    {
                        this.parent.ChangeState(new VerticalHomingDoneState(this.parent, this.driver, this.eventAggregator));
                        break;
                    }
                case MessageStatus.OperationError:
                    {
                        this.parent.ChangeState(new VerticalHomingErrorState(this.parent, this.driver, this.eventAggregator));
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        #endregion
    }
}

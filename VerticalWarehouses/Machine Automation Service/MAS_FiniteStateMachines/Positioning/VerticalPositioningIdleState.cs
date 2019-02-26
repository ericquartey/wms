using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class VerticalPositioningIdleState : IState
    {
        #region Fields

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private readonly StateMachineVerticalPositioning parent;

        #endregion

        #region Constructors

        public VerticalPositioningIdleState(StateMachineVerticalPositioning parent, INewInverterDriver driver,
            IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = driver;
            this.eventAggregator = eventAggregator;

            this.Target = 0;
            this.Velocity = 0.0f;
            this.Acceleration = 0.0f;
            this.Deceleration = 0.0f;
            this.Weight = 0.0f;
            this.Offset = 0;
            this.AbsoluteMovement = true;

            this.eventAggregator.GetEvent<NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public bool AbsoluteMovement { get; }

        public float Acceleration { get; }

        public float Deceleration { get; }

        public short Offset { get; }

        public int Target { get; }

        public string Type => "Vertical Positioning Idle State";

        public float Velocity { get; }

        public float Weight { get; }

        #endregion

        #region Methods

        public void MakeOperation()
        {
            this.driver.ExecuteVerticalPosition(this.Target, this.Velocity, this.Acceleration, this.Deceleration,
                this.Weight, this.Offset, this.AbsoluteMovement);
        }

        public void NotifyMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            this.driver.ExecuteVerticalPositionStop();

            this.eventAggregator.GetEvent<NotificationEvent>().Unsubscribe(this.notifyEventHandler);

            var notifyEvent = new NotificationMessage(null, "Positioning stopped", MessageActor.Any,
                MessageActor.FiniteStateMachines, MessageType.Positioning, MessageStatus.OperationStop);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(notifyEvent);
        }

        private void notifyEventHandler(NotificationMessage notification)
        {
            switch (notification.Status)
            {
                case MessageStatus.OperationEnd:
                {
                    this.parent.ChangeState(new VerticalPositioningDoneState(this.parent, this.driver,
                        this.eventAggregator));
                    break;
                }
                case MessageStatus.OperationError:
                {
                    this.parent.ChangeState(
                        new VerticalPositioningErrorState(this.parent, this.driver, this.eventAggregator));
                    break;
                }
            }
        }

        #endregion
    }
}

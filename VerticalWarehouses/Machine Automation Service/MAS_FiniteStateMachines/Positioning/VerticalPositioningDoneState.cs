using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class VerticalPositioningDoneState : IState
    {
        #region Fields

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineVerticalPositioning parent;

        #endregion

        #region Constructors

        public VerticalPositioningDoneState(StateMachineVerticalPositioning parent, INewInverterDriver driver,
            IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = driver;
            this.eventAggregator = eventAggregator;

            var notifyEvent = new NotificationMessage(null, "Positioning Done", MessageActor.Any,
                MessageActor.FiniteStateMachines, MessageType.Homing, MessageStatus.OperationEnd);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(notifyEvent);
        }

        #endregion

        #region Properties

        public string Type => "Vertical Positioning Done State";

        #endregion

        #region Methods

        public void MakeOperation()
        {
        }

        public void NotifyMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public void SendCommandMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public void SendNotificationMessage(NotificationMessage message)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            var notifyEvent = new NotificationMessage(null, "Positioning Done", MessageActor.Any,
                MessageActor.FiniteStateMachines, MessageType.Homing, MessageStatus.OperationStop);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(notifyEvent);
        }

        #endregion
    }
}

using Ferretto.VW.Common_Utils.EventParameters;
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

        public VerticalPositioningDoneState(StateMachineVerticalPositioning parent, INewInverterDriver driver, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = driver;
            this.eventAggregator = eventAggregator;

            var notifyEvent = new Notification_EventParameter(OperationType.Homing, OperationStatus.End, "Positioning done", Verbosity.Info);
            this.eventAggregator.GetEvent<FiniteStateMachines_NotificationEvent>().Publish(notifyEvent);
        }

        #endregion

        #region Properties

        public string Type => "Vertical Positioning Done State";

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
            var notifyEvent = new Notification_EventParameter(OperationType.Homing, OperationStatus.Stopped, "Positioning stopped", Verbosity.Info);
            this.eventAggregator.GetEvent<FiniteStateMachines_NotificationEvent>().Publish(notifyEvent);
        }

        #endregion
    }
}

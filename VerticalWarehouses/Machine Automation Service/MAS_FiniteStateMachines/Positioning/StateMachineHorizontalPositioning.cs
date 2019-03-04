using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class StateMachineHorizontalPositioning : IState, IStateMachine
    {
        #region Fields

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private IState state;

        #endregion

        #region Constructors

        public StateMachineHorizontalPositioning(INewInverterDriver driver, IEventAggregator eventAggregator)
        {
            this.driver = driver;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public string Type => this.state.Type;

        #endregion

        #region Methods

        public void ChangeState(IState newState, CommandMessage message = null)
        {
            this.state = newState;
        }

        void IStateMachine.ProcessCommandMessage(CommandMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void ProcessNotificationMessage(NotificationMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void PublishCommandMessage(CommandMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void PublishNotificationMessage(NotificationMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void SendCommandMessage(CommandMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void SendNotificationMessage(NotificationMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void Start()
        {
            this.state = new HorizontalPositioningIdleState(this, this.driver, this.eventAggregator);

            //TODO The parameter values will be provided to the Finite State Machine component via EventAggregator message
            //TODO Or by a function method to pass the parameters inside the HorizontalPositioningIdleState state

            //TODO this.state.MakeOperation();
        }

        #endregion
    }
}

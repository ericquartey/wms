using System;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class StateMachineVerticalPositioning : IState, IStateMachine
    {
        #region Fields

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private IState state;

        #endregion

        #region Constructors

        public StateMachineVerticalPositioning(INewInverterDriver driver, IEventAggregator eventAggregator)
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

        public void MakeOperation()
        {
            this.state?.MakeOperation();
        }

        public void NotifyMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public void PublishCommandMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public void PublishMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public void PublishNotificationMessage(NotificationMessage message)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            this.state = new VerticalPositioningIdleState(this, this.driver, this.eventAggregator);

            //TODO The parameter values will be provided to the Finite State Machine component via EventAggregator message
            //TODO Or by a function method to pass the parameters inside the VerticalPositioningIdleState state

            this.state.MakeOperation();
        }

        public void Stop()
        {
            this.state?.Stop();
        }

        #endregion
    }
}

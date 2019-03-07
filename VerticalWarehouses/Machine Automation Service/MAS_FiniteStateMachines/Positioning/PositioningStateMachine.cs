using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningStateMachine : StateMachineBase, IPositioningStateMachine
    {
        #region Fields

        private readonly Axis axisMovement;

        private readonly IPositioningMessageData positioningMessageData;

        #endregion

        #region Constructors

        public PositioningStateMachine(IEventAggregator eventAggregator, IPositioningMessageData positioningMessageData)
            : base(eventAggregator)
        {
            this.axisMovement = positioningMessageData.AxisMovement;
            this.positioningMessageData = positioningMessageData;
        }

        #endregion

        #region Properties

        public IState GetState => this.CurrentState;

        public IPositioningMessageData PositioningData => this.positioningMessageData;

        #endregion

        #region Methods

        public override void Start()
        {
            this.CurrentState = new PositioningStartState(this);
        }

        #endregion
    }
}

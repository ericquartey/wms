using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningErrorState : StateBase
    {
        #region Fields

        private readonly Axis axisMovement;

        #endregion

        #region Constructors

        public PositioningErrorState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            var positioninigData = ((IPositioningStateMachine)this.parentStateMachine).PositioningData;
            this.axisMovement = positioninigData.AxisMovement;

            //TEMP Notify the error condition
            var newMessage = new NotificationMessage(null,
                string.Format("Positioning {0} Error State", this.axisMovement),
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                MessageStatus.OperationError,
                ErrorLevel.Error,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishNotificationMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => string.Format("PositioningErrorState {0}", this.axisMovement);

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

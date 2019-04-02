using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingErrorState : StateBase
    {
        #region Fields

        private readonly Axis axis;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public HomingErrorState(IStateMachine parentMachine, Axis axis, ILogger logger)
        {
            this.parentStateMachine = parentMachine;
            this.logger = logger;
            this.axis = axis;

            //TEMP Notify the error condition all the world
            var newMessage = new NotificationMessage(null,
                "Error Homing State",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Homing,
                MessageStatus.OperationError,
                ErrorLevel.Error,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishNotificationMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "HomingErrorState";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            //TEMP Add your implementation code here
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            //TEMP Add your implementation code here
        }

        #endregion
    }
}

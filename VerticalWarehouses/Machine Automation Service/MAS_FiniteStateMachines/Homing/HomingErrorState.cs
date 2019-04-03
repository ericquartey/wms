using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
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
            this.ParentStateMachine = parentMachine;
            this.logger = logger;
            this.logger.LogTrace($"1-Constructor");
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
            this.logger.LogTrace($"2-Constructor: published notification: {newMessage.Type}, {newMessage.Status}, {newMessage.Destination}");
            this.ParentStateMachine.PublishNotificationMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "HomingErrorState";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogTrace($"Command processed: {message.Type}, {message.Destination}, {message.Source}");
            //TEMP Add your implementation code here
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"Notification processed: {message.Type}, {message.Status}, {message.Destination}");
            //TEMP Add your implementation code here
        }

        #endregion
    }
}

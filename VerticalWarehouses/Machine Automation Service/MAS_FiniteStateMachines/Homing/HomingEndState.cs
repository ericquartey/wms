using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingEndState : StateBase
    {
        #region Fields

        private readonly Axis axisToStop;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public HomingEndState(IStateMachine parentMachine, Axis axisToStop, ILogger logger)
        {
            this.parentStateMachine = parentMachine;
            this.axisToStop = axisToStop;
            this.logger = logger;
            this.logger.LogTrace($"********** Homing End State ctor");

            //TEMP Send a message to stop the homing to the inverter
            var stopMessageData = new StopAxisMessageData(this.axisToStop);
            var inverterMessage = new CommandMessage(stopMessageData,
                "Homing Stop",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(inverterMessage);

            var notificationMessageData = new CalibrateAxisMessageData(this.axisToStop, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Homing Completed",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.CalibrateAxis,
                MessageStatus.OperationEnd,
                ErrorLevel.NoError,
                MessageVerbosity.Info);

            this.parentStateMachine.PublishNotificationMessage(notificationMessage);

            this.logger.LogTrace("FSM Homing End ctor");
        }

        #endregion

        #region Properties

        public override string Type => "HomingEndState";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogTrace($"FSM Homing End processCommandMessage {message.Type}");
            //TEMP Add your implementation code here
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"FSM Homing End processNotificationMessage {message.Type}");
            if (message.Type == MessageType.Homing && message.Status == MessageStatus.OperationError)
            {
                this.parentStateMachine.PublishNotificationMessage(message);
            }

            if (message.Type == MessageType.CalibrateAxis)
            {
                //TEMP Send a notification about the end (/stop) operation to all the world
                var newMessage = new NotificationMessage(null,
                    "End Homing",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.Stop,
                    message.Status,
                    ErrorLevel.NoError,
                    MessageVerbosity.Info);
                this.parentStateMachine.OnPublishNotification(newMessage);
            }

            if (message.Type == MessageType.SwitchAxis)
            {
                //TEMP Send a notification about the end (/stop) operation to all the world
                var newMessage = new NotificationMessage(null,
                    "End Homing",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.Stop,
                    message.Status,
                    ErrorLevel.NoError,
                    MessageVerbosity.Info);
                this.parentStateMachine.OnPublishNotification(newMessage);
            }
        }

        #endregion
    }
}

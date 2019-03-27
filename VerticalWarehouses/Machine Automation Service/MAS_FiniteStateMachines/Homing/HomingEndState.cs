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
            this.logger.LogTrace($"1-Constructor");

            //TEMP Send a message to stop the homing to the inverter
            var stopMessageData = new StopAxisMessageData(this.axisToStop);
            var inverterMessage = new CommandMessage(stopMessageData,
                "Homing Stop",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(inverterMessage);
            this.logger.LogTrace($"2-Constructor: published command: {inverterMessage.Type}, {inverterMessage.Destination}");

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
            this.logger.LogTrace($"3-Constructor: published notification: {notificationMessage.Type}, {notificationMessage.Status}, {notificationMessage.Destination}");
            this.parentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        #endregion

        #region Properties

        public override string Type => "HomingEndState";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogTrace($"Command processed: {message.Type}, {message.Destination}, {message.Source}");
            //TEMP Add your implementation code here
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"Notification processed: {message.Type}, {message.Status}, {message.Destination}");
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

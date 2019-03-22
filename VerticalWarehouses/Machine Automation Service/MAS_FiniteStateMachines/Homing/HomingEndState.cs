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
            this.logger?.LogTrace("FSM Homing End state ==> ");

            this.parentStateMachine = parentMachine;
            this.axisToStop = axisToStop;
            this.logger = logger;

            //TEMP Send a message to stop the homing to the inverter
            var stopMessageData = new StopAxisMessageData(this.axisToStop);
            var inverterMessage = new CommandMessage(stopMessageData,
                "Homing Stop",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(inverterMessage);

            //TEMP Send a notification about the end (/stop) operation to all the world
            var newMessage = new NotificationMessage(null,
                "End Homing",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                MessageStatus.OperationEnd,
                ErrorLevel.NoError,
                MessageVerbosity.Info);
            this.parentStateMachine.OnPublishNotification(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "HomingEndState";

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
            if (message.Type == MessageType.Homing && message.Status == MessageStatus.OperationError)
            {
                this.parentStateMachine.PublishNotificationMessage(message);
            }
        }

        #endregion
    }
}

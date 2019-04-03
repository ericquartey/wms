using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
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
            this.logger = logger;
            this.logger.LogTrace("1:HomingEndState");
            this.parentStateMachine = parentMachine;
            this.axisToStop = axisToStop;

            //TEMP Send a message to stop the homing to the inverter
            var stopMessageData = new StopAxisMessageData(this.axisToStop);
            var inverterMessage = new CommandMessage(stopMessageData,
                "Homing Stop",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.InverterReset);
            this.parentStateMachine.PublishCommandMessage(inverterMessage);
        }

        #endregion

        #region Properties

        public override string Type => "HomingEndState";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogTrace($"2:Process CommandMessage {message.Type} Source {message.Source}");
            //TEMP Add your implementation code here
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"3:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == MessageType.InverterReset)
            {
                //TEMP Homing routine is completed successfully
                var notificationMessageData = new CalibrateAxisMessageData(this.axisToStop, MessageVerbosity.Info);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    "Homing Completed",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.Homing,
                    MessageStatus.OperationEnd);

                this.parentStateMachine.PublishNotificationMessage(notificationMessage);
            }

            if (message.Type == MessageType.CalibrateAxis || message.Type == MessageType.SwitchAxis)
            {
                //TEMP Homing routine has been stopped and so not completed successfully
                var notificationMessageData = new CalibrateAxisMessageData(this.axisToStop, MessageVerbosity.Info);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    "Homing not Completed",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.Homing,
                    MessageStatus.OperationStop);

                this.parentStateMachine.PublishNotificationMessage(notificationMessage);
            }
        }

        #endregion
    }
}

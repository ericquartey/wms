using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingStartState : StateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public HomingStartState(IStateMachine parentMachine, Axis axisToCalibrate, ILogger logger)
        {
            this.logger = logger;
            this.logger.LogTrace("1:HomingStartState");

            this.parentStateMachine = parentMachine;
            this.axisToCalibrate = axisToCalibrate;

            // TEMP send a message to switch axis (to IODriver)
            var switchAxisData = new SwitchAxisMessageData(this.axisToCalibrate);
            var message = new CommandMessage(switchAxisData,
                string.Format("Switch Axis {0}", this.axisToCalibrate),
                MessageActor.IODriver,
                MessageActor.FiniteStateMachines,
                MessageType.SwitchAxis);
            this.parentStateMachine.PublishCommandMessage(message);

            var notificationMessageData = new CalibrateAxisMessageData(this.axisToCalibrate, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Starting Homing",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.CalibrateAxis,
                MessageStatus.OperationExecuting,
                ErrorLevel.NoError);

            this.parentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        #endregion

        #region Properties

        public override string Type => "HomingStartState";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogTrace($"2:Process CommandMessage {message.Type} Source {message.Source}");
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TEMP Change to homing end state (a request of stop operation has been made)
                    this.parentStateMachine.ChangeState(new HomingEndState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                    break;
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"3:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");
            if (message.Type == MessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        //TEMP Change to switch axis end state (the operation of switching axis has been done)
                        this.parentStateMachine.ChangeState(new HomingSwitchAxisDoneState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                        break;

                    case MessageStatus.OperationError:
                        //TEMP Change to error state (an error has occurred)
                        this.parentStateMachine.ChangeState(new HomingErrorState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                        break;
                }
            }
        }

        #endregion
    }
}

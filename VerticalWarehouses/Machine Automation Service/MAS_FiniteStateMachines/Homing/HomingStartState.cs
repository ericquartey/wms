﻿using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
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
            this.parentStateMachine = parentMachine;
            this.axisToCalibrate = axisToCalibrate;
            this.logger = logger;
            this.logger.LogTrace($"1-Constructor");
            // TEMP send a message to switch axis (to IODriver)
            var switchAxisData = new SwitchAxisMessageData(this.axisToCalibrate);
            var message = new CommandMessage(switchAxisData,
                string.Format("Switch Axis {0}", this.axisToCalibrate),
                MessageActor.IODriver,
                MessageActor.FiniteStateMachines,
                MessageType.SwitchAxis,
                MessageVerbosity.Info);
            this.logger.LogTrace($"2-Constructor: published command: {message.Type}, {message.Destination}");
            this.parentStateMachine.PublishCommandMessage(message);

            var notificationMessageData = new CalibrateAxisMessageData(this.axisToCalibrate, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Homing Completed",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.CalibrateAxis,
                MessageStatus.OperationExecuting,
                ErrorLevel.NoError,
                MessageVerbosity.Info);
            this.logger.LogTrace($"3-Constructor: published notification: {notificationMessage.Type}, {notificationMessage.Status}, {notificationMessage.Destination}");
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
            this.logger.LogTrace($"Command processed: {message.Type}, {message.Destination}, {message.Source}");
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TEMP Change to homing end state (a request of stop operation has been made)
                    this.parentStateMachine.ChangeState(new HomingEndState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                    break;

                default:
                    break;
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"Notification processed: {message.Type}, {message.Status}, {message.Destination}");
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

                    default:
                        break;
                }
            }
        }

        #endregion
    }
}

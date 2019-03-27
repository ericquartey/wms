using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingSwitchAxisDoneState : StateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public HomingSwitchAxisDoneState(IStateMachine parentMachine, Axis axisToCalibrate, ILogger logger)
        {
            this.parentStateMachine = parentMachine;
            this.axisToCalibrate = axisToCalibrate;
            this.logger = logger;
            this.logger.LogTrace($"1-Constructor");
            //TEMP send a message to start the homing for a horizontal axis (to inverter and other components)
            var calibrateAxisData = new CalibrateAxisMessageData(this.axisToCalibrate);
            var newMessage = new CommandMessage(calibrateAxisData,
                string.Format("Homing {0} State Started", axisToCalibrate),
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.CalibrateAxis,
                MessageVerbosity.Info);
            this.logger.LogTrace($"2-Constructor: published command: {newMessage.Type}, {newMessage.Destination}");
            this.parentStateMachine.PublishCommandMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "HomingSwitchAxisDoneState";

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
            this.logger.LogTrace($"1-Notification processed: {message.Type}, {message.Status}, {message.Destination}");
            if (message.Type == MessageType.CalibrateAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        //TEMP Change to homing calibrate end state (the operation of homing for the current axis is done successfully)
                        this.logger.LogTrace($"2-Change State to HomingCalibrateAxisDoneState");
                        this.parentStateMachine.ChangeState(new HomingCalibrateAxisDoneState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                        break;

                    case MessageStatus.OperationError:
                        //TEMP Change to error state (an error has occurred)
                        this.logger.LogTrace($"3-Change State to HomingErrorState");
                        this.parentStateMachine.ChangeState(new HomingErrorState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                        break;

                    default:
                        this.logger.LogTrace($"4-Hitted default case, no further action performed: {message.Status}");
                        break;
                }
            }
        }

        #endregion
    }
}

using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Data;
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
            this.logger = logger;
            this.logger.LogTrace("1:HomingSwitchAxisDoneState");

            this.ParentStateMachine = parentMachine;
            this.axisToCalibrate = axisToCalibrate;

            //TEMP send a message to start the homing for a horizontal axis (to inverter and other components)
            var calibrateAxisData = new HomingMessageData(this.axisToCalibrate);
            var newMessage = new CommandMessage(calibrateAxisData,
                string.Format("Homing {0} State Started", axisToCalibrate),
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.CalibrateAxis,
                MessageVerbosity.Info);
            this.logger.LogTrace($"2-Constructor: published command: {newMessage.Type}, {newMessage.Destination}");
            this.ParentStateMachine.PublishCommandMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "HomingSwitchAxisDoneState";

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
                    this.ParentStateMachine.ChangeState(new HomingEndState(this.ParentStateMachine, this.axisToCalibrate, this.logger));
                    break;

                default:
                    break;
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"3:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");
            if (message.Type == MessageType.CalibrateAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        //TEMP Change to homing calibrate end state (the operation of homing for the current axis is done successfully)
                        this.logger.LogTrace($"2-Change State to HomingCalibrateAxisDoneState");
                        this.ParentStateMachine.ChangeState(new HomingCalibrateAxisDoneState(this.ParentStateMachine, this.axisToCalibrate, this.logger));
                        break;

                    case MessageStatus.OperationError:
                        //TEMP Change to error state (an error has occurred)
                        this.logger.LogTrace($"3-Change State to HomingErrorState");
                        this.ParentStateMachine.ChangeState(new HomingErrorState(this.ParentStateMachine, this.axisToCalibrate, this.logger));
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

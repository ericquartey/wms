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
            this.logger?.LogTrace("Homing Switch Axis Done state ==> ");

            this.parentStateMachine = parentMachine;
            this.axisToCalibrate = axisToCalibrate;
            this.logger = logger;

            //TEMP send a message to start the homing for a horizontal axis (to inverter and other components)
            var calibrateAxisData = new CalibrateAxisMessageData(this.axisToCalibrate);
            var newMessage = new CommandMessage(calibrateAxisData,
                string.Format("Homing {0} State Started", axisToCalibrate),
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.CalibrateAxis,
                MessageVerbosity.Info);
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
            if (message.Type == MessageType.CalibrateAxis)
            {
                this.logger?.LogTrace(string.Format("Homing Switch Axis Done state : Process notification message {0}-{1}", message.Type, message.Status));

                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        //TEMP Change to homing calibrate end state (the operation of homing for the current axis is done successfully)
                        this.parentStateMachine.ChangeState(new HomingCalibrateAxisDoneState(this.parentStateMachine, this.axisToCalibrate, this.logger));
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

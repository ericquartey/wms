using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingSwitchAxisDoneState : StateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        #endregion

        #region Constructors

        public HomingSwitchAxisDoneState(IStateMachine parentMachine, Axis axisToCalibrate)
        {
            this.parentStateMachine = parentMachine;
            this.axisToCalibrate = axisToCalibrate;

            //TEMP send a message to start the homing for a horizontal axis (to inverter and other components)
            var calibrateAxisData = new CalibrateAxisMessageData(this.axisToCalibrate);
            var newMessage = new CommandMessage(calibrateAxisData,
                string.Format("Homing {0} State Started", axisToCalibrate),
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.CalibrateAxis, //TEMP or MessageType.Homing
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "HomingSwitchAxisDoneState";

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TODO add state business logic to stop current action
                    this.ProcessStopHoming(message);
                    break;

                default:
                    break;
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            if (message.Type == MessageType.CalibrateAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        //TEMP the homing operation is done successfully
                        this.ProcessEndHoming(message);
                        break;

                    case MessageStatus.OperationError:
                        //TEMP an error occurs
                        this.ProcessErrorHoming(message);
                        break;

                    default:
                        break;
                }
            }
        }

        private void ProcessEndHoming(NotificationMessage message)
        {
            //TEMP Change to homing calibrate end state (the operation of homing for the current axis is done)
            this.parentStateMachine.ChangeState(new HomingCalibrateAxisDoneState(this.parentStateMachine, this.axisToCalibrate));
        }

        private void ProcessErrorHoming(NotificationMessage message)
        {
            this.parentStateMachine.ChangeState(new HomingErrorState(this.parentStateMachine));
        }

        private void ProcessStopHoming(CommandMessage message)
        {
            //TEMP A request to stop the operation has been made
            var newMessage = new CommandMessage(null,
                "Stop Requested",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                MessageVerbosity.Info);

            ((IHomingStateMachine)this.parentStateMachine).IsStopRequested = true;

            this.parentStateMachine.ChangeState(new HomingEndState(this.parentStateMachine));
        }

        #endregion
    }
}

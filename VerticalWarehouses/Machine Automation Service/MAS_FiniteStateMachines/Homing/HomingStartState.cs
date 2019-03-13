using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingStartState : StateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        #endregion

        #region Constructors

        public HomingStartState(IStateMachine parentMachine, Axis axisToCalibrate)
        {
            this.parentStateMachine = parentMachine;
            this.axisToCalibrate = axisToCalibrate;

            var calibrateData = ((IHomingStateMachine)this.parentStateMachine).CalibrateData;

            // TEMP send a message to switch axis (to IODriver)
            var switchAxisData = new SwitchAxisMessageData(this.axisToCalibrate);
            var message = new CommandMessage(switchAxisData,
                string.Format("Switch Axis {0}", this.axisToCalibrate),
                MessageActor.IODriver,
                MessageActor.FiniteStateMachines,
                MessageType.SwitchAxis,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(message);
        }

        #endregion

        #region Properties

        public override string Type => "HomingStartState";

        #endregion

        #region Methods

        public override void SendCommandMessage(CommandMessage message)
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

        public override void SendNotificationMessage(NotificationMessage message)
        {
            if (message.Type == MessageType.SwitchAxis)
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
            //TEMP Change to switch axis end state (the operation of switching axis has been done)
            this.parentStateMachine.ChangeState(new HomingSwitchAxisDoneState(this.parentStateMachine, this.axisToCalibrate));
        }

        private void ProcessErrorHoming(NotificationMessage message)
        {
            this.parentStateMachine.ChangeState(new HomingErrorState(this.parentStateMachine));
        }

        private void ProcessStopHoming(CommandMessage message)
        {
            //TEMP This is a request to stop the operation
            var newMessage = new CommandMessage(null,
                "Stop Requested",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                MessageVerbosity.Info);

            ((IHomingStateMachine)this.parentStateMachine).IsStopRequested = true;
            //TEMP Change to homing end state (a request of stop operation has been made)
            this.parentStateMachine.ChangeState(new HomingEndState(this.parentStateMachine));
        }

        #endregion
    }
}

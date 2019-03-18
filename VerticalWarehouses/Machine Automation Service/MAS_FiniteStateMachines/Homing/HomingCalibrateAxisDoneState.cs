using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingCalibrateAxisDoneState : StateBase
    {
        #region Fields

        private readonly Axis axisToSwitch;

        #endregion

        #region Constructors

        public HomingCalibrateAxisDoneState(IStateMachine parentMachine, Axis axisCalibrated)
        {
            this.parentStateMachine = parentMachine;

            Axis axis;
            if (((IHomingStateMachine)this.parentStateMachine).NMaxSteps == 1)
            {
                //TEMP No axis change
                axis = axisCalibrated;
            }
            else
            {
                //TEMP axis change
                axis = (axisCalibrated == Axis.Horizontal) ? Axis.Vertical : Axis.Horizontal; //TEMP The axis is changed
            }
            this.axisToSwitch = axis;

            ((IHomingStateMachine)this.parentStateMachine).ChangeAxis(axis);

            // TEMP send a message to switch axis (to IODriver)
            var switchAxisData = new SwitchAxisMessageData(this.axisToSwitch);
            var message = new CommandMessage(switchAxisData,
                string.Format("Switch Axis {0}", this.axisToSwitch),
                MessageActor.IODriver,
                MessageActor.FiniteStateMachines,
                MessageType.SwitchAxis,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(message);
        }

        #endregion

        #region Properties

        public override string Type => "HomingCalibrateAxisDoneState";

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
            if (message.Type == MessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        //TEMP the homing operation is done successfully
                        this.ProcessEndSwitching(message);
                        break;

                    case MessageStatus.OperationError:
                        //TEMP an error occurs
                        this.ProcessErrorSwitching(message);
                        break;

                    default:
                        break;
                }
            }
        }

        private void ProcessEndSwitching(NotificationMessage message)
        {
            ((IHomingStateMachine)this.parentStateMachine).NumberOfExecutedSteps++;

            if (((IHomingStateMachine)this.parentStateMachine).NumberOfExecutedSteps == ((IHomingStateMachine)this.parentStateMachine).NMaxSteps)
            {
                //TEMP Change to end state (the operation is done)
                this.parentStateMachine.ChangeState(new HomingEndState(this.parentStateMachine, this.axisToSwitch));
            }
            else
            {
                //TEMP Change to switch end state (the operation of switch for the current axis has been done)
                this.parentStateMachine.ChangeState(new HomingSwitchAxisDoneState(this.parentStateMachine, this.axisToSwitch));
            }
        }

        private void ProcessErrorSwitching(NotificationMessage message)
        {
            //TEMP Change to error state
            this.parentStateMachine.ChangeState(new HomingErrorState(this.parentStateMachine));
        }

        private void ProcessStopHoming(CommandMessage message)
        {
            //TEMP This is a request to stop the operation
            ((IHomingStateMachine)this.parentStateMachine).IsStopRequested = true;

            this.parentStateMachine.ChangeState(new HomingEndState(this.parentStateMachine, this.axisToSwitch));
        }

        #endregion
    }
}

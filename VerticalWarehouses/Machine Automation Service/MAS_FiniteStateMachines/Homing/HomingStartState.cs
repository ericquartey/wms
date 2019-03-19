using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;

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

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TEMP Change to homing end state (a request of stop operation has been made)
                    this.parentStateMachine.ChangeState(new HomingEndState(this.parentStateMachine, this.axisToCalibrate));
                    break;

                default:
                    break;
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            if (message.Type == MessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        //TEMP Change to switch axis end state (the operation of switching axis has been done)
                        this.parentStateMachine.ChangeState(new HomingSwitchAxisDoneState(this.parentStateMachine, this.axisToCalibrate));
                        break;

                    case MessageStatus.OperationError:
                        //TEMP Change to error state (an error has occurred)
                        this.parentStateMachine.ChangeState(new HomingErrorState(this.parentStateMachine, this.axisToCalibrate));
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion
    }
}

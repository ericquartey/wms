using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingCalibrateAxisDoneState : StateBase
    {
        #region Fields

        private readonly Axis axisToSwitch;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public HomingCalibrateAxisDoneState(IStateMachine parentMachine, Axis axisCalibrated, ILogger logger)
        {
            this.parentStateMachine = parentMachine;
            this.logger = logger;
            this.logger.LogTrace($"1-Constructor");
            this.axisToSwitch = (axisCalibrated == Axis.Horizontal) ? Axis.Vertical : Axis.Horizontal;

            // TEMP send a message to switch axis (to IODriver)
            var switchAxisData = new SwitchAxisMessageData(this.axisToSwitch);
            var message = new CommandMessage(switchAxisData,
                string.Format("Switch Axis {0}", this.axisToSwitch),
                MessageActor.IODriver,
                MessageActor.FiniteStateMachines,
                MessageType.SwitchAxis,
                MessageVerbosity.Info);
            this.logger.LogTrace($"2-Constructor: published command: {message.Type}, {message.Destination}");
            this.parentStateMachine.PublishCommandMessage(message);
        }

        #endregion

        #region Properties

        public override string Type => "HomingCalibrateAxisDoneState";

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
                    this.parentStateMachine.ChangeState(new HomingEndState(this.parentStateMachine, this.axisToSwitch, this.logger));
                    break;

                default:
                    break;
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1-Notification processed: {message.Type}, {message.Status}, {message.Destination}");
            if (message.Type == MessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        //TEMP the SwitchAxis operation is done successfully
                        this.logger.LogTrace($"2-OperationEndCase: {message.Type}, {message.Status}, {message.Destination}");
                        this.ProcessEndSwitching(message);
                        break;

                    case MessageStatus.OperationError:
                        //TEMP Change to error state (an error has occurred)
                        this.logger.LogTrace($"3-Change State to HomingErrorState");
                        this.parentStateMachine.ChangeState(new HomingErrorState(this.parentStateMachine, this.axisToSwitch, this.logger));
                        break;

                    default:
                        this.logger.LogTrace($"4-Hitted defauls case");
                        break;
                }
            }
        }

        private void ProcessEndSwitching(NotificationMessage message)
        {
            if (this.parentStateMachine.OperationDone)
            {
                //TEMP Change to end state (the operation is done)
                this.parentStateMachine.ChangeState(new HomingEndState(this.parentStateMachine, this.axisToSwitch, this.logger));
            }
            else
            {
                //TEMP Change to switch end state (the operation of switch for the current axis has been done)
                this.parentStateMachine.ChangeState(new HomingSwitchAxisDoneState(this.parentStateMachine, this.axisToSwitch, this.logger));
            }
        }

        #endregion
    }
}

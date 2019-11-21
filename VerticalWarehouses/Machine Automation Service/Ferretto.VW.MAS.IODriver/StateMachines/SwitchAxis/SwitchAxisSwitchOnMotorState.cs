using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.SwitchAxis
{
    internal sealed class SwitchAxisSwitchOnMotorState : IoStateBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public SwitchAxisSwitchOnMotorState(
            Axis axisToSwitchOn,
            IoStatus status,
            IoIndex index,
            ILogger logger,
            IIoStateMachine parentStateMachine)
            : base(parentStateMachine, logger)
        {
            this.axisToSwitchOn = axisToSwitchOn;
            this.status = status;
            this.index = index;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            if (message.ValidOutputs)
            {
                this.Logger.LogTrace($"2:Axis to switch on={this.axisToSwitchOn}:Cradle motor on={message.CradleMotorOn}:Elevator motor on={message.ElevatorMotorOn}");

                if ((this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn)
                    ||
                    (this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn))
                {
                    this.Logger.LogTrace("3:Change State to EndState");
                    this.ParentStateMachine.ChangeState(new SwitchAxisEndState(this.axisToSwitchOn, this.status, this.index, this.Logger, this.ParentStateMachine));
                }
            }
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            var checkMessage = message.FormatDataOperation == ShdFormatDataOperation.Data &&
                               message.ValidOutputs;

            if (this.status.MatchOutputs(message.Outputs) && checkMessage)
            {
                this.Logger.LogTrace($"2:Axis to switch on={this.axisToSwitchOn}:Cradle motor on={message.CradleMotorOn}:Elevator motor on={message.ElevatorMotorOn}");

                if ((this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn)
                    ||
                    (this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn))
                {
                    this.Logger.LogTrace("3:Change State to EndState");
                    this.ParentStateMachine.ChangeState(new SwitchAxisEndState(this.axisToSwitchOn, this.status, this.index, this.Logger, this.ParentStateMachine));
                }
            }
        }

        public override void Start()
        {
            var switchOnAxisIoMessage = new IoWriteMessage();

            this.Logger.LogTrace($"1:Switch on axis {this.axisToSwitchOn}. IO={switchOnAxisIoMessage}");

            switch (this.axisToSwitchOn)
            {
                case Axis.Horizontal:
                    switchOnAxisIoMessage.SwitchCradleMotor(true);
                    break;

                case Axis.Vertical:
                    switchOnAxisIoMessage.SwitchElevatorMotor(true);
                    break;
            }

            switchOnAxisIoMessage.ResetSecurity = false;
            switchOnAxisIoMessage.PowerEnable = true;

            this.Logger.LogTrace($"2:{switchOnAxisIoMessage}");
            lock (this.status)
            {
                this.status.UpdateOutputStates(switchOnAxisIoMessage.Outputs);
            }

            this.ParentStateMachine.EnqueueMessage(switchOnAxisIoMessage);
        }

        #endregion
    }
}

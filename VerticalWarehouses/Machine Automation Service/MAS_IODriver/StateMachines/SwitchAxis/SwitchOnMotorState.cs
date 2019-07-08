using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_IODriver.Interface;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis
{
    public class SwitchOnMotorState : IoStateBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public SwitchOnMotorState(
            Axis axisToSwitchOn,
            IoSHDStatus status,
            ILogger logger,
            IIoStateMachine parentStateMachine)
            : base(parentStateMachine, logger)
        {
            this.axisToSwitchOn = axisToSwitchOn;
            this.status = status;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~SwitchOnMotorState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
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
                    this.ParentStateMachine.ChangeState(new EndState(this.axisToSwitchOn, this.status, this.Logger, this.ParentStateMachine));
                }
            }
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            var checkMessage = message.FormatDataOperation == Enumerations.SHDFormatDataOperation.Data &&
                               message.ValidOutputs;

            if (this.status.MatchOutputs(message.Outputs) && checkMessage)
            {
                this.Logger.LogTrace($"2:Axis to switch on={this.axisToSwitchOn}:Cradle motor on={message.CradleMotorOn}:Elevator motor on={message.ElevatorMotorOn}");

                if ((this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn)
                    ||
                    (this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn))
                {
                    this.Logger.LogTrace("3:Change State to EndState");
                    this.ParentStateMachine.ChangeState(new EndState(this.axisToSwitchOn, this.status, this.Logger, this.ParentStateMachine));
                }
            }
        }

        public override void Start()
        {
            var switchOnAxisIoMessage = new IoSHDWriteMessage();

            this.Logger.LogTrace($"1:Switch on axis io={switchOnAxisIoMessage}");

            switch (this.axisToSwitchOn)
            {
                case Axis.Horizontal:
                    switchOnAxisIoMessage.SwitchCradleMotor(true);
                    break;

                case Axis.Vertical:
                    switchOnAxisIoMessage.SwitchElevatorMotor(true);
                    break;
            }

            this.Logger.LogTrace($"2:{switchOnAxisIoMessage}");
            lock (this.status)
            {
                this.status.UpdateOutputStates(switchOnAxisIoMessage.Outputs);
            }

            this.ParentStateMachine.EnqueueMessage(switchOnAxisIoMessage);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}

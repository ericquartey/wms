using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_IODriver.Interface;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis
{
    public class SwitchOffMotorState : IoStateBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        /// <inheritdoc />
        public SwitchOffMotorState(
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

        ~SwitchOffMotorState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            if (message.ValidOutputs)
            {
                this.Logger.LogTrace($"1:this.Axis to switch on={this.axisToSwitchOn}:Cradle motor on{message.CradleMotorOn}:Elevator motor on={message.ElevatorMotorOn}");

                if ((this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn)
                    ||
                    (this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn))
                {
                    this.Logger.LogTrace("2:Change State to SwitchOnMotorState");
                    this.ParentStateMachine.ChangeState(new SwitchOnMotorState(this.axisToSwitchOn, this.status, this.Logger, this.ParentStateMachine));
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
                this.Logger.LogTrace($"2:this.Axis to switch on={this.axisToSwitchOn}:Cradle motor on{message.CradleMotorOn}:Elevator motor on={message.ElevatorMotorOn}");

                if ((this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn)
                    ||
                    (this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn))
                {
                    this.Logger.LogTrace("3:Change State to SwitchOnMotorState");
                    this.ParentStateMachine.ChangeState(new SwitchOnMotorState(this.axisToSwitchOn, this.status, this.Logger, this.ParentStateMachine));
                }
            }
        }

        public override void Start()
        {
            var switchOffAxisIoMessage = new IoSHDWriteMessage();

            this.Logger.LogTrace($"1:Switch off axis IO={switchOffAxisIoMessage}");

            switch (this.axisToSwitchOn)
            {
                case Axis.Horizontal:
                    switchOffAxisIoMessage.SwitchElevatorMotor(false);
                    break;

                case Axis.Vertical:
                    switchOffAxisIoMessage.SwitchCradleMotor(false);
                    break;
            }

            lock (this.status)
            {
                this.status.UpdateOutputStates(switchOffAxisIoMessage.Outputs);
            }
            this.ParentStateMachine.EnqueueMessage(switchOffAxisIoMessage);
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

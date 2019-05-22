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

        private readonly ILogger logger;

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        /// <inheritdoc />
        public SwitchOffMotorState(Axis axisToSwitchOn, IoSHDStatus status, ILogger logger, IIoStateMachine parentStateMachine)
        {
            logger.LogDebug("1:Method Start");

            this.axisToSwitchOn = axisToSwitchOn;
            this.status = status;
            this.ParentStateMachine = parentStateMachine;
            this.logger = logger;

            this.logger.LogDebug("2:Method End");
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
            this.logger.LogDebug("1:Method Start");

            if (message.ValidOutputs)
            {
                this.logger.LogTrace($"2:this.Axis to switch on={this.axisToSwitchOn}:Cradle motor on{message.CradleMotorOn}:Elevator motor on={message.ElevatorMotorOn}");

                if (this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn || this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn)
                {
                    this.logger.LogTrace("3:Change State to SwitchOnMotorState");
                    this.ParentStateMachine.ChangeState(new SwitchOnMotorState(this.axisToSwitchOn, this.status, this.logger, this.ParentStateMachine));
                }
            }

            this.logger.LogDebug("4:Method End");
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            var checkMessage = message.FormatDataOperation == Enumerations.SHDFormatDataOperation.Data &&
                               message.ValidOutputs;

            if (this.status.MatchOutputs(message.Outputs) && checkMessage)
            {
                this.logger.LogTrace($"2:this.Axis to switch on={this.axisToSwitchOn}:Cradle motor on{message.CradleMotorOn}:Elevator motor on={message.ElevatorMotorOn}");

                if (this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn || this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn)
                {
                    this.logger.LogTrace("3:Change State to SwitchOnMotorState");
                    this.ParentStateMachine.ChangeState(new SwitchOnMotorState(this.axisToSwitchOn, this.status, this.logger, this.ParentStateMachine));
                }
            }

            this.logger.LogDebug("4:Method End");
        }

        public override void Start()
        {
            this.logger.LogDebug("1: Method Start");

            var switchOffAxisIoMessage = new IoSHDWriteMessage();

            this.logger.LogTrace($"2:Switch off axis IO={switchOffAxisIoMessage}");

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

            this.logger.LogDebug("3:Method End");
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

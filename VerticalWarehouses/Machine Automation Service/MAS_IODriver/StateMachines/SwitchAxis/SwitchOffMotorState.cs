using Ferretto.VW.MAS_IODriver.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis
{
    public class SwitchOffMotorState : IoStateBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        /// <inheritdoc />
        public SwitchOffMotorState(Axis axisToSwitchOn, ILogger logger, IIoStateMachine parentStateMachine)
        {
            logger.LogDebug("1:Method Start");

            this.axisToSwitchOn = axisToSwitchOn;
            this.ParentStateMachine = parentStateMachine;
            this.logger = logger;

            var switchOffAxisIoMessage = new IoMessage(false);

            this.logger.LogTrace($"2:Switch off axis IO={switchOffAxisIoMessage}");

            switch (axisToSwitchOn)
            {
                case Axis.Horizontal:
                    switchOffAxisIoMessage.SwitchElevatorMotor(false);
                    break;

                case Axis.Vertical:
                    switchOffAxisIoMessage.SwitchCradleMotor(false);
                    break;
            }
            parentStateMachine.EnqueueMessage(switchOffAxisIoMessage);

            this.logger.LogDebug("4:Method End");
        }

        #endregion

        #region Destructors

        ~SwitchOffMotorState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            if (message.ValidOutputs)
            {
                this.logger.LogTrace($"2:this.Axis to switch on={this.axisToSwitchOn}:Cradle motor on{message.CradleMotorOn}:Elevator motor on={message.ElevatorMotorOn}");

                if (this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn || this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn)
                {
                    this.logger.LogTrace("3:Change State to SwitchOnMotorState");
                    this.ParentStateMachine.ChangeState(new SwitchOnMotorState(this.axisToSwitchOn, this.logger, this.ParentStateMachine));
                }
            }

            this.logger.LogDebug("4:Method End");
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

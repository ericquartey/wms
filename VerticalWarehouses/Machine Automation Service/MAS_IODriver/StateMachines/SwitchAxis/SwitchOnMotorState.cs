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

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public SwitchOnMotorState(Axis axisToSwitchOn, ILogger logger, IIoStateMachine parentStateMachine)
        {
            logger.LogDebug("1:Method Start");

            this.axisToSwitchOn = axisToSwitchOn;
            this.ParentStateMachine = parentStateMachine;
            this.logger = logger;

            ///*var switchOnAxisIoMessage = new IoSHDMessage(false);*/  // change with IoSHDWriteMessage
            //var switchOnAxisIoMessage = new IoSHDWriteMessage();

            //this.logger.LogTrace($"2:Switch on axis io={switchOnAxisIoMessage}");

            //switch (axisToSwitchOn)
            //{
            //    case Axis.Horizontal:
            //        switchOnAxisIoMessage.SwitchCradleMotor(true);
            //        break;

            //    case Axis.Vertical:
            //        switchOnAxisIoMessage.SwitchElevatorMotor(true);
            //        break;
            //}

            //this.logger.LogTrace($"3:{switchOnAxisIoMessage}");

            //parentStateMachine.EnqueueMessage(switchOnAxisIoMessage);

            this.logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~SwitchOnMotorState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        // Useless
        public override void ProcessMessage(IoSHDMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            if (message.ValidOutputs)
            {
                this.logger.LogTrace($"2:Axis to switch on={this.axisToSwitchOn}:Cradle motor on={message.CradleMotorOn}:Elevator motor on={message.ElevatorMotorOn}");

                if (this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn || this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn)
                {
                    this.logger.LogTrace("3:Change State to EndState");
                    this.ParentStateMachine.ChangeState(new EndState(this.axisToSwitchOn, this.logger, this.ParentStateMachine));
                }
            }

            this.logger.LogDebug("4:Method End");
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            if (message.FormatDataOperation == Enumerations.SHDFormatDataOperation.Data &&
                message.ValidOutputs)
            {
                this.logger.LogTrace($"2:Axis to switch on={this.axisToSwitchOn}:Cradle motor on={message.CradleMotorOn}:Elevator motor on={message.ElevatorMotorOn}");

                if (this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn || this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn)
                {
                    this.logger.LogTrace("3:Change State to EndState");
                    this.ParentStateMachine.ChangeState(new EndState(this.axisToSwitchOn, this.logger, this.ParentStateMachine));
                }
            }

            this.logger.LogDebug("4:Method End");
        }

        public override void Start()
        {
            this.logger.LogDebug("1: Method Start");

            var switchOnAxisIoMessage = new IoSHDWriteMessage();

            this.logger.LogTrace($"2:Switch on axis io={switchOnAxisIoMessage}");

            switch (this.axisToSwitchOn)
            {
                case Axis.Horizontal:
                    switchOnAxisIoMessage.SwitchCradleMotor(true);
                    break;

                case Axis.Vertical:
                    switchOnAxisIoMessage.SwitchElevatorMotor(true);
                    break;
            }

            this.logger.LogTrace($"3:{switchOnAxisIoMessage}");

            this.ParentStateMachine.EnqueueMessage(switchOnAxisIoMessage);

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

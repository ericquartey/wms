using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.SwitchAxis
{
    internal sealed class SwitchAxisStartState : IoStateBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        private readonly BayNumber bayNumber;

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        /// <inheritdoc />
        public SwitchAxisStartState(
            Axis axisToSwitchOn,
            IoStatus status,
            IoIndex index,
            BayNumber bayNumber,
            ILogger logger,
            IIoStateMachine parentStateMachine)
            : base(parentStateMachine, logger)
        {
            this.axisToSwitchOn = axisToSwitchOn;
            this.status = status;
            this.index = index;
            this.bayNumber = bayNumber;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            var checkMessage = message.FormatDataOperation == ShdFormatDataOperation.Data &&
                               message.ValidOutputs;

            if (this.status.MatchOutputs(message.Outputs) && checkMessage)
            {
                this.Logger.LogTrace($"2:this.Axis to switch on={this.axisToSwitchOn}:Cradle motor on{message.CradleMotorOn}:Elevator motor on={message.ElevatorMotorOn}");

                if ((this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn)
                    ||
                    (this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn))
                {
                    this.Logger.LogTrace("3:Change State to SwitchOnMotorState");
                    this.ParentStateMachine.ChangeState(new SwitchAxisSwitchOnMotorState(this.axisToSwitchOn, this.status, this.index, this.bayNumber, this.Logger, this.ParentStateMachine));
                }
            }
        }

        public override void Start()
        {
            var switchOffAxisIoMessage = new IoWriteMessage { PowerEnable = true, BayLightOn = this.status.OutputData?[(int)IoPorts.BayLight] ?? false };

            switch (this.axisToSwitchOn)
            {
                case Axis.Horizontal:
                    switchOffAxisIoMessage.SwitchElevatorMotor(false);
                    break;

                case Axis.Vertical:
                    switchOffAxisIoMessage.SwitchCradleMotor(false);
                    break;
            }

            this.Logger.LogDebug($"1:Switch axis start {this.axisToSwitchOn}. IO={switchOffAxisIoMessage}");

            lock (this.status)
            {
                this.status.UpdateOutputStates(switchOffAxisIoMessage.Outputs);
            }

            this.ParentStateMachine.EnqueueMessage(switchOffAxisIoMessage);
        }

        #endregion
    }
}

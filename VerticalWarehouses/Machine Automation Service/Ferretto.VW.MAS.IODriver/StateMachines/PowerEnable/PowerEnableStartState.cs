using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.PowerEnable
{
    internal sealed class PowerEnableStartState : IoStateBase
    {
        #region Fields

        private readonly IoIndex deviceIndex;

        private readonly bool enable;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        /// <inheritdoc />
        public PowerEnableStartState(
            bool enable,
            IoStatus status,
            ILogger logger,
            IIoStateMachine parentStateMachine,
            IoIndex deviceIndex)
            : base(parentStateMachine, logger)
        {
            this.enable = enable;
            this.status = status;

            this.deviceIndex = deviceIndex;

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
                this.Logger.LogTrace($"1:Power Enable on{message.PowerEnable}");

                if (message.PowerEnable == this.enable)
                {
                    this.Logger.LogTrace("3:Change State to PowerEnableEndState");
                    this.ParentStateMachine.ChangeState(new PowerEnableEndState(this.enable, this.status, this.Logger, this.ParentStateMachine, this.deviceIndex));
                }
            }
        }

        public override void Start()
        {
            var powerEnableIoMessage = new IoWriteMessage
            {
                BayLightOn = this.status.OutputData?[(int)IoPorts.BayLight] ?? false,
                EndMissionRobotOn = this.status.OutputData?[(int)IoPorts.EndMissionRobot] ?? false,
                ReadyWarehouseRobotOn = this.status.OutputData?[(int)IoPorts.ReadyWarehouseRobot] ?? false,
            };

            this.Logger.LogDebug($"1:Power Enable ={powerEnableIoMessage}");

            powerEnableIoMessage.PowerEnable = this.enable;

            lock (this.status)
            {
                this.status.UpdateOutputStates(powerEnableIoMessage.Outputs);
            }

            this.ParentStateMachine.EnqueueMessage(powerEnableIoMessage);
        }

        #endregion
    }
}

using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.MeasureProfile
{
    internal sealed class MeasureProfileStartState : IoStateBase
    {
        #region Fields

        private readonly IoIndex deviceIndex;

        private readonly bool enable;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        /// <inheritdoc />
        public MeasureProfileStartState(
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
                this.Logger.LogTrace($"1:Power Enable on{message.MeasureProfileOn}");

                if (message.MeasureProfileOn == this.enable)
                {
                    this.Logger.LogTrace("3:Change State to MeasureProfileEndState");
                    this.ParentStateMachine.ChangeState(new MeasureProfileEndState(this.enable, this.status, this.Logger, this.ParentStateMachine, this.deviceIndex));
                }
            }
        }

        public override void Start()
        {
            var message = new IoWriteMessage(this.status.OutputData);

            message.SwitchMeasureProfile(this.enable);

            this.Logger.LogDebug($"1:Measure Profile ={message}");

            lock (this.status)
            {
                this.status.UpdateOutputStates(message.Outputs);
            }

            this.ParentStateMachine.EnqueueMessage(message);
        }

        #endregion
    }
}

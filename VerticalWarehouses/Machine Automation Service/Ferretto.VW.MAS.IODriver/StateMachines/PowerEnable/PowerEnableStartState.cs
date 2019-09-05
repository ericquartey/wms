using Ferretto.VW.MAS.IODriver.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.PowerEnable
{
    public class PowerEnableStartState : IoStateBase
    {

        #region Fields

        private readonly IoIndex deviceIndex;

        private readonly bool enable;

        private readonly IoStatus status;

        private bool disposed;

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

        #region Destructors

        ~PowerEnableStartState()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

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

        public override void ProcessMessage(IoMessage message)
        {
            if (message.ValidOutputs)
            {
                this.Logger.LogTrace($"1:Power Enable on{message.PowerEnable}");

                if (message.PowerEnable == this.enable)
                {
                    this.Logger.LogTrace("2:Change State to PowerEnableEndState");
                    this.ParentStateMachine.ChangeState(new PowerEnableEndState(this.enable, this.status, this.Logger, this.ParentStateMachine, this.deviceIndex));
                }
            }
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            var checkMessage = message.FormatDataOperation == Enumerations.ShdFormatDataOperation.Data &&
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
            var powerEnableIoMessage = new IoWriteMessage();

            this.Logger.LogTrace($"1:Power Enable ={powerEnableIoMessage}");

            powerEnableIoMessage.SwitchPowerEnable(this.enable);

            lock (this.status)
            {
                this.status.UpdateOutputStates(powerEnableIoMessage.Outputs);
            }
            this.ParentStateMachine.EnqueueMessage(powerEnableIoMessage);
        }

        #endregion
    }
}

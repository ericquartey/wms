using Ferretto.VW.MAS.IODriver.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.ResetSecurity
{
    public class ResetSecurityStartState : IoStateBase
    {
        #region Fields

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public ResetSecurityStartState(
            IIoStateMachine parentStateMachine,
            IoStatus status,
            IoIndex index,
            ILogger logger)
            : base(parentStateMachine, logger)
        {
            this.status = status;
            this.index = index;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Reset Security={message.ResetSecurity}");

            if (message.ValidOutputs && !message.ResetSecurity)
            {
                this.ParentStateMachine.ChangeState(new ResetSecurityEndState(this.ParentStateMachine, this.status, this.index, this.Logger));
            }
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Reset Security={message.ResetSecurity}");

            var checkMessage = message.FormatDataOperation == Enumerations.ShdFormatDataOperation.Data &&
                message.ValidOutputs && !message.ResetSecurity;

            if (checkMessage && this.status.MatchOutputs(message.Outputs))
            {
                this.ParentStateMachine.ChangeState(new ResetSecurityEndState(this.ParentStateMachine, this.status, this.index, this.Logger));
            }
        }

        public override void Start()
        {
            var resetIoMessage = new IoWriteMessage();
            resetIoMessage.SwitchResetSecurity(true);
            resetIoMessage.SwitchPowerEnable(true);

            this.Logger.LogTrace($"1:Reset Security Pulse={resetIoMessage}");

            lock (this.status)
            {
                this.status.UpdateOutputStates(resetIoMessage.Outputs);
            }
            this.ParentStateMachine.EnqueueMessage(resetIoMessage);
        }

        #endregion
    }
}

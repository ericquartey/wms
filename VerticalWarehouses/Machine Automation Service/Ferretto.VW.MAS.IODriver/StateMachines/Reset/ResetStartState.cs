﻿using Ferretto.VW.MAS.IODriver.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.Reset
{
    public class ResetStartState : IoStateBase
    {
        #region Fields

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public ResetStartState(
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
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Outputs cleared={message.OutputsCleared}");

            if (message.ValidOutputs && message.OutputsCleared)
            {
                this.ParentStateMachine.ChangeState(new ResetEndState(this.ParentStateMachine, this.status, this.index, this.Logger));
            }
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Outputs cleared={message.OutputsCleared}");

            var checkMessage = message.FormatDataOperation == Enumerations.ShdFormatDataOperation.Data &&
                message.ValidOutputs && message.OutputsCleared;

            if (this.status.MatchOutputs(message.Outputs))
            {
                this.ParentStateMachine.ChangeState(new ResetEndState(this.ParentStateMachine, this.status, this.index, this.Logger));
            }
        }

        public override void Start()
        {
            var resetIoMessage = new IoWriteMessage();

            this.Logger.LogTrace($"1:Reset IO={resetIoMessage}");

            lock (this.status)
            {
                this.status.UpdateOutputStates(resetIoMessage.Outputs);
            }
            this.ParentStateMachine.EnqueueMessage(resetIoMessage);
        }

        #endregion
    }
}

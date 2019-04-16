using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Positioning
{
    public class StartingPositioningState : InverterStateBase
    {
        #region Fields

        private const int SEND_DELAY = 100;

        private const ushort STATUS_WORD_VALUE = 0x1037;

        private readonly IPositioningFieldMessageData data;

        private readonly ILogger logger;

        private readonly ushort parameterValue;

        private bool disposed;

        #endregion

        #region Constructors

        public StartingPositioningState(IInverterStateMachine parentStateMachine, IPositioningFieldMessageData data, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.ParentStateMachine = parentStateMachine;
            this.data = data;
            this.logger = logger;

            this.logger.LogTrace($"2:Positioning {this.data.AxisMovement} axis");

            switch (this.data.AxisMovement)
            {
                case Axis.Horizontal:
                    this.parameterValue = 0x801F;
                    break;

                case Axis.Vertical:
                    this.parameterValue = 0x001F;
                    break;
            }

            var inverterMessage =
                new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue, SEND_DELAY);

            this.logger.LogTrace($"3:inverterMessage={inverterMessage}");

            parentStateMachine.EnqueueMessage(inverterMessage);

            this.logger.LogDebug("4:Method End");
        }

        #endregion

        #region Destructors

        ~StartingPositioningState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override bool ProcessMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new ErrorState(this.ParentStateMachine, this.data, this.logger));
            }

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
            {
                this.logger.LogTrace($"3:UShortPayload={message.UShortPayload:X}:StatusWordValue={STATUS_WORD_VALUE:X}");

                if ((message.UShortPayload & STATUS_WORD_VALUE) == STATUS_WORD_VALUE)
                {
                    this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.data, this.logger));
                    returnValue = true;
                }
            }

            this.logger.LogDebug("4:Method End");

            return returnValue;
        }

        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");

            this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.data, this.logger, true));

            this.logger.LogDebug("2:Method End");
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

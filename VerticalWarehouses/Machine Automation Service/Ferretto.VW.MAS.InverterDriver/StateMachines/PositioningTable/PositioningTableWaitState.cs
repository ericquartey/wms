using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningTableWaitState : InverterStateBase
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        #endregion

        #region Constructors

        public PositioningTableWaitState(
            IInverterStateMachine parentStateMachine,
            IInverterPositioningFieldMessageData data,
            IPositioningInverterStatus inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.data = data;
        }

        #endregion

        #region Properties

        public IPositioningInverterStatus Inverter { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogDebug("Inverter Positioning Table Wait State");
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Positioning Stop requested");

            this.ParentStateMachine.ChangeState(
                new PositioningTableDisableOperationState(
                    this.ParentStateMachine,
                    this.InverterStatus as IPositioningInverterStatus,
                    this.Logger,
                    true));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            return true;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new PositioningTableErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (!this.data.WaitContinue)
                {
                    this.ParentStateMachine.ChangeState(new PositioningTableStartMovingState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                }
                else
                {
                    this.Logger.LogTrace("Waiting for Continue Command");
                }
            }
            return returnValue;
        }

        #endregion
    }
}

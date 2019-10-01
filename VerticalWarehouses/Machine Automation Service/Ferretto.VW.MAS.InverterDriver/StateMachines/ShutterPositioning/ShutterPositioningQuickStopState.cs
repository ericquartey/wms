using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.ShutterPositioning
{
    internal class ShutterPositioningQuickStopState : InverterStateBase
    {
        #region Fields

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        #endregion

        #region Constructors

        public ShutterPositioningQuickStopState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            IInverterShutterPositioningFieldMessageData shutterPositionData,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.shutterPositionData = shutterPositionData;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogDebug($"Shutter Positioning Quick Stop");
            this.InverterStatus.CommonControlWord.QuickStop = false;
            this.InverterStatus.CommonControlWord.EnableVoltage = false;

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.ControlWordParam,
                    this.InverterStatus.CommonControlWord.Value));
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Stop process already active");
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (!this.InverterStatus.CommonStatusWord.IsQuickStopTrue)
                {
                    this.ParentStateMachine.ChangeState(
                        new ShutterPositioningEndState(
                            this.ParentStateMachine,
                            this.InverterStatus,
                            this.shutterPositionData,
                            this.Logger,
                            true));

                    returnValue = true;
                }
            }

            return returnValue;
        }

        #endregion
    }
}

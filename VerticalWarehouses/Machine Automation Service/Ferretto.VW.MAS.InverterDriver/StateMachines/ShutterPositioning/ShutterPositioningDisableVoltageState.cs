using Ferretto.VW.MAS.InverterDriver.Contracts;

using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.ShutterPositioning
{
    internal class ShutterPositioningDisableVoltageState : InverterStateBase
    {
        #region Fields

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        private readonly bool stopRequested;

        #endregion

        #region Constructors

        public ShutterPositioningDisableVoltageState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            IInverterShutterPositioningFieldMessageData shutterPositionData,
            ILogger logger,
            bool stopRequested = false)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.shutterPositionData = shutterPositionData;
            this.stopRequested = stopRequested;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Shutter positioning Disable Voltage. StopRequested = {this.stopRequested}");
            this.InverterStatus.CommonControlWord.EnableVoltage = false;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, this.InverterStatus.CommonControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            if (this.stopRequested)
            {
                this.Logger.LogTrace("1:Stop process already active");
            }
            else
            {
                this.Logger.LogDebug("1:Shutter Positioning Stop requested");

                this.ParentStateMachine.ChangeState(
                    new ShutterPositioningStopState(
                        this.ParentStateMachine,
                        this.InverterStatus,
                        this.shutterPositionData,
                        this.Logger));
            }
        }

        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

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
                if (this.InverterStatus is AglInverterStatus currentStatus)
                {
                    if (!this.InverterStatus.CommonStatusWord.IsVoltageEnabled)
                    {
                        if (this.stopRequested)
                        {
                            this.ParentStateMachine.ChangeState(new ShutterPositioningSwitchOffState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));
                        }
                        returnValue = true;
                    }
                }
            }
            return returnValue;
        }

        #endregion
    }
}

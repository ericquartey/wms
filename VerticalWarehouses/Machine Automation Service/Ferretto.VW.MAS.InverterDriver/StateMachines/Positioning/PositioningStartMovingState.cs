using System.Threading;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningStartMovingState : InverterStateBase
    {
        #region Fields

        private readonly Timer axisPositionUpdateTimer;

        #endregion

        #region Constructors

        public PositioningStartMovingState(
            IInverterStateMachine parentStateMachine,
            IPositioningInverterStatus inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.Inverter = inverterStatus;
            this.axisPositionUpdateTimer = new Timer(this.RequestAxisPositionUpdate, null, -1, Timeout.Infinite);
        }

        #endregion

        #region Properties

        public IPositioningInverterStatus Inverter { get; }

        protected bool TargetPositionReached =>
            this.Inverter.PositionStatusWord.SetPointAcknowledge
            &&
            this.Inverter.PositionStatusWord.PositioningAttained;

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogDebug($"PositioningStartMoving.Start Inverter type={this.InverterStatus.GetType().Name}");

            this.Inverter.PositionControlWord.NewSetPoint = true;

            this.Logger.LogDebug("Set New Setpoint");

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.ControlWord,
                    this.Inverter.PositionControlWord.Value));

            this.axisPositionUpdateTimer.Change(250, 250);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Positioning Stop requested");

            this.axisPositionUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);

            this.ParentStateMachine.ChangeState(
                new PositioningDisableOperationState(
                    this.ParentStateMachine,
                    this.InverterStatus as IPositioningInverterStatus,
                    this.Logger,
                    true));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            if (message.ParameterId == InverterParameterId.ControlWord)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.axisPositionUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                this.ParentStateMachine.ChangeState(
                    new PositioningErrorState(
                        this.ParentStateMachine,
                        this.InverterStatus,
                        this.Logger));

                return true;
            }

            this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");

            if (this.TargetPositionReached
                && message.ParameterId == InverterParameterId.ActualPositionShaft
                )
            {
                this.Logger.LogDebug("Target position reached.");

                this.axisPositionUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                this.ParentStateMachine.ChangeState(
                    new PositioningDisableOperationState(
                        this.ParentStateMachine,
                        this.Inverter,
                        this.Logger));
            }
            else
            {
                this.Logger.LogTrace("Moving towards target position.");
            }

            return true; //INFO Next status word request handled by timer
        }

        protected override void OnDisposing()
        {
            this.axisPositionUpdateTimer?.Dispose();
        }

        private void RequestAxisPositionUpdate(object state)
        {
            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    InverterParameterId.ActualPositionShaft));
        }

        #endregion
    }
}

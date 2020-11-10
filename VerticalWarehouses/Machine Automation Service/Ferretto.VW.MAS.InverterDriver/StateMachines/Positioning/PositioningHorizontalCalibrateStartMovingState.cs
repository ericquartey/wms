using System.Threading;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningHorizontalCalibrateStartMovingState : InverterStateBase
    {
        #region Fields

        private readonly Timer axisPositionUpdateTimer;

        private readonly IInverterPositioningFieldMessageData data;

        #endregion

        #region Constructors

        public PositioningHorizontalCalibrateStartMovingState(
            IInverterStateMachine parentStateMachine,
            IInverterPositioningFieldMessageData data,
            IPositioningInverterStatus inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.data = data;
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

        public override void Start()
        {
            this.Logger.LogInformation("Starting horizontal calibration.");
            this.Inverter.PositionControlWord.ImmediateChangeSet = true;
            this.Inverter.PositionControlWord.NewSetPoint = true;

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.ControlWord,
                    this.Inverter.PositionControlWord.Value));

            this.axisPositionUpdateTimer.Change(250, 250);

            this.data.WaitContinue = true;
        }

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

        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            if (message.ParameterId == InverterParameterId.ControlWord)
            {
                return false;
            }

            return true;
        }

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

            if ((this.TargetPositionReached
                    && message.ParameterId == InverterParameterId.ActualPositionShaft
                    )
                    || !this.data.WaitContinue
                )
            {
                this.axisPositionUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                if (!this.data.WaitContinue)
                {
                    this.Logger.LogDebug("Continue command received.");
                    this.ParentStateMachine.ChangeState(
                        new PositioningHorizontalCalibrateDisableOperationState(
                            this.ParentStateMachine,
                            this.data,
                            this.Inverter,
                            this.Logger));
                }
                else
                {
                    this.Logger.LogDebug("Target position reached: horizontal calibration end");
                    this.ParentStateMachine.ChangeState(
                        new PositioningDisableOperationState(
                            this.ParentStateMachine,
                            this.Inverter,
                            this.Logger));
                }
            }
            else
            {
                this.Logger.LogTrace("Waiting for Continue Command");
            }

            return true;
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

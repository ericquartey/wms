using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningStartMovingState : InverterStateBase
    {
        #region Fields

        private readonly Timer axisPositionUpdateTimer;

        private readonly IInverterPositioningFieldMessageData data;

        private readonly IErrorsProvider errorProvider;

        private int? oldPosition;

        private DateTime startTime;

        #endregion

        #region Constructors

        public PositioningStartMovingState(
            IInverterStateMachine parentStateMachine,
            IInverterPositioningFieldMessageData data,
            IPositioningInverterStatus inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.data = data;
            this.Inverter = inverterStatus;
            this.axisPositionUpdateTimer = new Timer(this.RequestAxisPositionUpdate, null, -1, Timeout.Infinite);
            this.oldPosition = null;
            this.errorProvider = this.ParentStateMachine.GetRequiredService<IErrorsProvider>();
        }

        #endregion

        #region Properties

        public IPositioningInverterStatus Inverter { get; }

        public bool SignalsArrived { get; private set; }

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

            this.startTime = DateTime.UtcNow;
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

            if (message.ParameterId == InverterParameterId.ActualPositionShaft
                && message.SystemIndex == this.Inverter.SystemIndex
                )
            {
                if (this.TargetPositionReached)
                {
                    if (this.SignalsArrived)
                    {
                        this.Logger.LogDebug($"Target position reached, inverter {this.InverterStatus.SystemIndex}.");

                        this.axisPositionUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        this.ParentStateMachine.ChangeState(
                            new PositioningDisableOperationState(
                                this.ParentStateMachine,
                                this.Inverter,
                                this.Logger));
                    }
                }
                else
                {
                    this.SignalsArrived = false;
                    int? position = null;
                    if (this.Inverter is AngInverterStatus angInverter)
                    {
                        if (this.data.AxisMovement == Axis.Horizontal)
                        {
                            position = angInverter.CurrentPositionAxisHorizontal;
                        }
                        else
                        {
                            position = angInverter.CurrentPositionAxisVertical;
                        }
                    }
                    else if (this.Inverter is AcuInverterStatus acuInverter)
                    {
                        position = acuInverter.CurrentPosition;
                    }

                    if (position.HasValue)
                    {
                        this.Logger.LogTrace($"Inverter {this.InverterStatus.SystemIndex} moving towards target position: present {position.Value}, old {this.oldPosition}");
                        // if position doesn't change raise an alarm
                        if (this.oldPosition.HasValue
                            && Math.Abs(position.Value - this.oldPosition.Value) < 1
                            && this.data.TargetSpeed[0] > 1
                            )
                        {
                            if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > 2000)
                            {
                                this.Logger.LogError($"PositioningStartMoving position timeout, inverter {this.InverterStatus.SystemIndex}");
                                this.errorProvider.RecordNew(MachineErrorCode.StartPositioningBlocked, additionalText: $"{this.data.AxisMovement}");
                                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                                return true;
                            }
                        }
                        else
                        {
                            this.startTime = DateTime.UtcNow;
                            this.oldPosition = position;
                        }
                    }
                }
            }
            else if (message.ParameterId == InverterParameterId.DigitalInputsOutputs)
            {
                this.SignalsArrived = true;
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

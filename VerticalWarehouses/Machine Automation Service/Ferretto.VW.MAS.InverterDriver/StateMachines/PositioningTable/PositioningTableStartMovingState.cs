using System;
using System.Threading;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    /// <inheritdoc />
    public override void Start()
    {
        this.Logger.LogDebug("Set Sequence Mode");
        this.startTime = DateTime.UtcNow;
        if (this.InverterStatus is IPositioningInverterStatus currentStatus)
        {
            currentStatus.TableTravelControlWord.SequenceMode = true;
            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, currentStatus.TableTravelControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

            this.axisPositionUpdateTimer.Change(250, 250);
            this.SignalsArrived = 0;
        }
        else
        {
            this.Logger.LogError($"1:Invalid inverter status");
            this.ParentStateMachine.ChangeState(new PositioningTableErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
        }
    }

    internal class PositioningTableStartMovingState : InverterStateBase
    {
        #region Fields

        private readonly Timer axisPositionUpdateTimer;

        private int? oldPosition;

        private DateTime startTime;

        #endregion

        #region Constructors

        public PositioningTableStartMovingState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.axisPositionUpdateTimer = new Timer(this.RequestAxisPositionUpdate, null, -1, Timeout.Infinite);
            this.oldPosition = null;
        }

        #endregion

        #region Properties

        public int SignalsArrived { get; private set; }

        #endregion

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Positioning Stop requested");

            this.axisPositionUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
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
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            if (message.ParameterId == InverterParameterId.ControlWord)
            {
                if (this.InverterStatus is IPositioningInverterStatus currentStatus
                    && !currentStatus.TableTravelControlWord.StartMotionBlock
                    )
                {
                    currentStatus.TableTravelControlWord.StartMotionBlock = true;
                    var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, currentStatus.TableTravelControlWord.Value);

                    this.Logger.LogDebug($"1:inverterMessage={inverterMessage}");

                    this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
                }
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
                this.ParentStateMachine.ChangeState(new PositioningTableErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (this.InverterStatus is IPositioningInverterStatus currentStatus)
                {
                    if (currentStatus.TableTravelStatusWord.MotionBlockInProgress

                        && message.ParameterId == InverterParameterId.ActualPositionShaft
                        )
                    {
                        if (currentStatus.TableTravelStatusWord.TargetReached)
                        {
                            if (this.SignalsArrived > 1)
                            {
                                this.Logger.LogDebug("Table Position Reached !");
                                this.axisPositionUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                                this.ParentStateMachine.ChangeState(new PositioningTableDisableOperationState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                            }
                        }
                        else
                        {
                            this.SignalsArrived = 0;
                        }
                    }
                    else if (message.ParameterId == InverterParameterId.DigitalInputsOutputs)
                    {
                        this.SignalsArrived++;
                    }
                    else
                    {
                        this.Logger.LogTrace("Moving towards target position.");
                    }

                    int? position = null;
                    if (currentStatus is AngInverterStatus angInverter)
                    {
                        position = angInverter.CurrentPositionAxisHorizontal;
                    }
                    else if (currentStatus is AcuInverterStatus acuInverter)
                    {
                        position = acuInverter.CurrentPosition;
                    }

                    if (position.HasValue)
                    {
                        this.Logger.LogTrace($"Inverter {this.InverterStatus.SystemIndex} moving towards target table position: present {position.Value}, old {this.oldPosition}");
                        // if position doesn't change raise an alarm
                        if (this.oldPosition.HasValue
                            && position.Value == this.oldPosition.Value
                            && DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > 2000)
                        {
                            this.Logger.LogError($"PositioningTableStartMoving position timeout, inverter {this.InverterStatus.SystemIndex}");
                            this.ParentStateMachine.ChangeState(new PositioningTableErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                            return true;
                        }
                        this.startTime = DateTime.UtcNow;
                        this.oldPosition = position;
                    }
                }
            }
        }

            //INFO Next status word request handled by timer
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
}
}

using System.Threading;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningTableStartMovingState : InverterStateBase
    {
        #region Fields

        private readonly Timer axisPositionUpdateTimer;

        #endregion

        #region Constructors

        public PositioningTableStartMovingState(
                    IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.axisPositionUpdateTimer = new Timer(this.RequestAxisPositionUpdate, null, -1, Timeout.Infinite);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogDebug("Set Sequence Mode");
            if (this.InverterStatus is IPositioningInverterStatus currentStatus)
            {
                currentStatus.TableTravelControlWord.SequenceMode = true;
                var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, currentStatus.TableTravelControlWord.Value);

                this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

                this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

                this.axisPositionUpdateTimer.Change(250, 250);
            }
            else
            {
                this.Logger.LogError($"1:Invalid inverter status");
                this.ParentStateMachine.ChangeState(new PositioningTableErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
        }

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

                    this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

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
                        && currentStatus.TableTravelStatusWord.TargetReached
                        && message.ParameterId == InverterParameterId.ActualPositionShaft
                        )
                    {
                        this.axisPositionUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        this.ParentStateMachine.ChangeState(new PositioningTableDisableOperationState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                        this.Logger.LogDebug("Position Reached !");
                    }
                    else
                    {
                        this.Logger.LogTrace("Moving towards target position.");
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

        #endregion
    }
}

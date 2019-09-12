using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    public class PositioningTableStartMovingState : InverterStateBase
    {
        #region Constructors

        public PositioningTableStartMovingState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
        }

        #endregion

        #region Destructors

        ~PositioningTableStartMovingState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Release()
        {
        }

        /// <inheritdoc />
        public override void Start()
        {
            if (this.InverterStatus is AngInverterStatus currentStatus)
            {
                currentStatus.TableTravelControlWord.SequenceMode = true;
                currentStatus.TableTravelControlWord.StartMotionBlock = true;
            }
            //TODO complete type failure check
            this.Logger.LogDebug("Set Sequence Mode");

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, ((AngInverterStatus)this.InverterStatus).TableTravelControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new PositioningTableEndState(this.ParentStateMachine, this.InverterStatus, this.Logger, true));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            if (message.ParameterId == InverterParameterId.ControlWordParam)
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
                this.Logger.LogError($"1:message={message}:Is Error={message.IsError}");
                this.ParentStateMachine.ChangeState(new PositioningTableErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }

            if (this.InverterStatus is AngInverterStatus currentStatus)
            {
                if (currentStatus.TableTravelStatusWord.MotionBlockInProgress && currentStatus.TableTravelStatusWord.TargetReached)
                {
                    this.ParentStateMachine.ChangeState(new PositioningTableDisableOperationState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                    this.Logger.LogDebug("Position Reached !");
                }
                else
                {
                    this.Logger.LogDebug("Position Not Reached");
                }
            }

            //INFO Next status word request handled by timer
            return true;
        }

        protected override void OnDisposing()
        {
        }

        #endregion
    }
}

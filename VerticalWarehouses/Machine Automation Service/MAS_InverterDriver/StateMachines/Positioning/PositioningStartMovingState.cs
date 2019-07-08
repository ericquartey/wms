using System.Threading;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Positioning
{
    public class PositioningStartMovingState : InverterStateBase
    {
        #region Fields

        private const int STATUS_WORD_REQUEST_INTERVAL = 100;

        private bool positioningReachedReset;

        private Timer requestStatusWordMessageTimer;

        #endregion

        #region Constructors

        public PositioningStartMovingState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
        }

        #endregion

        #region Destructors

        ~PositioningStartMovingState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Release()
        {
            //TEMP Stop the timer
            this.requestStatusWordMessageTimer.Change(-1, Timeout.Infinite);
        }

        /// <inheritdoc />
        public override void Start()
        {
            if (this.InverterStatus is AngInverterStatus currentStatus)
            {
                currentStatus.PositionControlWord.NewSetPoint = true;
            }
            //TODO complete type failure check

            //TEMP Create the timer
            this.requestStatusWordMessageTimer?.Dispose();
            this.requestStatusWordMessageTimer = new Timer(this.RequestStatusWordMessage, null, -1, Timeout.Infinite);

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, ((AngInverterStatus)this.InverterStatus).PositionControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            if (message.ParameterId == InverterParameterId.ControlWordParam)
            {
                //TEMP Start the timer
                this.requestStatusWordMessageTimer.Change(STATUS_WORD_REQUEST_INTERVAL, STATUS_WORD_REQUEST_INTERVAL);

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            var returnValue = true;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }

            this.InverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (this.InverterStatus is AngInverterStatus currentStatus)
            {
                if (!currentStatus.PositionStatusWord.PositioningAttained)
                {
                    this.positioningReachedReset = true;
                }

                if (this.positioningReachedReset && currentStatus.PositionStatusWord.PositioningAttained)
                {
                    //TEMP Stop the timer
                    this.requestStatusWordMessageTimer.Change(-1, Timeout.Infinite);

                    this.ParentStateMachine.ChangeState(new PositioningDisableOperationState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                    returnValue = true;
                }
            }

            this.Logger.LogDebug($"2:Method End with return value {returnValue}");

            return returnValue;
        }

        protected override void OnDisposing()
        {
            this.requestStatusWordMessageTimer?.Dispose();
            this.requestStatusWordMessageTimer = null;
        }

        private void RequestStatusWordMessage(object state)
        {
            var readStatusWordMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.StatusWordParam);

            this.Logger.LogTrace($"1:readStatusWordMessage={readStatusWordMessage}");

            this.ParentStateMachine.EnqueueMessage(readStatusWordMessage);
        }

        #endregion
    }
}

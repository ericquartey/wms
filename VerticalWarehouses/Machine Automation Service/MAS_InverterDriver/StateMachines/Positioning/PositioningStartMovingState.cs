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

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private bool disposed;

        private bool positioningReachedReset;

        private Timer requestStatusWordMessageTimer;

        #endregion

        #region Constructors

        public PositioningStartMovingState(IInverterStateMachine parentStateMachine, IInverterStatusBase inverterStatus, ILogger logger)
        {
            logger.LogTrace("1:Method Start");

            this.ParentStateMachine = parentStateMachine;
            this.inverterStatus = inverterStatus;
            this.logger = logger;
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
            if (this.inverterStatus is AngInverterStatus currentStatus)
            {
                currentStatus.PositionControlWord.NewSetPoint = true;
            }
            //TODO complete type failure check

            //TEMP Create the timer
            this.requestStatusWordMessageTimer?.Dispose();
            this.requestStatusWordMessageTimer = new Timer(this.RequestStatusWordMessage, null, -1, Timeout.Infinite);

            var inverterMessage = new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, ((AngInverterStatus)this.inverterStatus).PositionControlWord.Value);

            this.logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

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
            this.logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            var returnValue = true;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.inverterStatus, this.logger));
            }

            this.inverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (this.inverterStatus is AngInverterStatus currentStatus)
            {
                if (!currentStatus.PositionStatusWord.PositioningAttained)
                {
                    this.positioningReachedReset = true;
                }

                if (this.positioningReachedReset && currentStatus.PositionStatusWord.PositioningAttained)
                {
                    //TEMP Stop the timer
                    this.requestStatusWordMessageTimer.Change(-1, Timeout.Infinite);

                    this.ParentStateMachine.ChangeState(new PositioningDisableOperationState(this.ParentStateMachine, this.inverterStatus, this.logger));
                    returnValue = true;
                }
            }

            this.logger.LogDebug($"2:Method End with return value {returnValue}");

            return returnValue;
        }

        protected override void Dispose(bool disposing)
        {
            this.requestStatusWordMessageTimer?.Dispose();
            this.requestStatusWordMessageTimer = null;

            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        private void RequestStatusWordMessage(object state)
        {
            var readStatusWordMessage = new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.StatusWordParam);

            this.logger.LogTrace($"1:readStatusWordMessage={readStatusWordMessage}");

            this.ParentStateMachine.EnqueueMessage(readStatusWordMessage);
        }

        #endregion
    }
}

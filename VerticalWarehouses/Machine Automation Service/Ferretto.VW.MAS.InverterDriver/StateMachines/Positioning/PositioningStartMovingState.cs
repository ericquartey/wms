using System.Threading;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    public class PositioningStartMovingState : InverterStateBase
    {
        #region Fields

        private const int STATUS_WORD_REQUEST_INTERVAL = 100;

        private Timer requestStatusWordMessageTimer;

        private object syncTimer = new object();

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
            this.Logger.LogDebug("Set New Setpoint");

            //TEMP Create the timer
            this.requestStatusWordMessageTimer?.Dispose();
            this.requestStatusWordMessageTimer = new Timer(this.RequestStatusWordMessage, null, -1, Timeout.Infinite);

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, ((AngInverterStatus)this.InverterStatus).PositionControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            //TEMP Stop the timer
            this.requestStatusWordMessageTimer.Change(-1, Timeout.Infinite);

            this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.InverterStatus, this.Logger, true));
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

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }

            if (this.InverterStatus is AngInverterStatus currentStatus)
            {
                if (currentStatus.PositionStatusWord.PositioningAttained)
                {
                    //TEMP Stop the timer
                    this.requestStatusWordMessageTimer.Change(-1, Timeout.Infinite);

                    this.ParentStateMachine.ChangeState(new PositioningDisableOperationState(this.ParentStateMachine, this.InverterStatus, this.Logger));
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
            this.requestStatusWordMessageTimer?.Dispose();
            this.requestStatusWordMessageTimer = null;
        }

        private void RequestStatusWordMessage(object state)
        {
            lock (this.syncTimer)
            {
                var readStatusWordMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.StatusWordParam);

                this.Logger.LogTrace($"1:readStatusWordMessage={readStatusWordMessage}");

                this.ParentStateMachine.EnqueueMessage(readStatusWordMessage);
            }
        }

        #endregion
    }
}

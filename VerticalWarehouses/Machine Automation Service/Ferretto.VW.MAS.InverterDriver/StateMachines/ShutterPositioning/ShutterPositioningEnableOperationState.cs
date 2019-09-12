using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.ShutterPositioning
{
    public class ShutterPositioningEnableOperationState : InverterStateBase
    {
        #region Fields

        private readonly ShutterPosition shutterDestination;

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        private ShutterPosition oldShutterPosition;

        #endregion

        #region Constructors

        public ShutterPositioningEnableOperationState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            IInverterShutterPositioningFieldMessageData shutterPositionData,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.shutterPositionData = shutterPositionData;
            this.shutterDestination = this.shutterPositionData.ShutterPosition;
        }

        #endregion

        #region Destructors

        ~ShutterPositioningEnableOperationState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Release()
        {
        }

        public override void Start()
        {
            this.InverterStatus.CommonControlWord.EnableOperation = true;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, ((AglInverterStatus)this.InverterStatus).ProfileVelocityControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);
            if (this.InverterStatus is AglInverterStatus currentStatus)
            {
                this.oldShutterPosition = currentStatus.CurrentShutterPosition;
            }
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger, true));
        }

        /// <inheritdoc/>
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
                this.Logger.LogError($"1:message={message}:Is Error={message.IsError}");
                this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));
            }

            if (message.SystemIndex == this.InverterStatus.SystemIndex)
            {
                if (this.InverterStatus is AglInverterStatus currentStatus)
                {
                    if (this.InverterStatus.CommonStatusWord.IsOperationEnabled &&
                        (currentStatus.ProfileVelocityStatusWord.TargetReached
                            && currentStatus.CurrentShutterPosition == this.shutterDestination
                            )
                        )
                    {
                        this.ParentStateMachine.ChangeState(new ShutterPositioningDisableOperationState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));
                        returnValue = true;
                    }
                    else if (this.oldShutterPosition != currentStatus.CurrentShutterPosition)
                    {
                        this.oldShutterPosition = currentStatus.CurrentShutterPosition;
                        var messageData = this.shutterPositionData;
                        messageData.ShutterPosition = this.oldShutterPosition;
                        var endNotification = new FieldNotificationMessage(
                            messageData,
                            "Shutter Positioning executing",
                            FieldMessageActor.FiniteStateMachines,
                            FieldMessageActor.InverterDriver,
                            FieldMessageType.ShutterPositioning,
                            MessageStatus.OperationExecuting,
                            this.InverterStatus.SystemIndex);

                        this.Logger.LogTrace($"1:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                        this.ParentStateMachine.PublishNotificationEvent(endNotification);
                    }
                }
            }

            return returnValue;
        }

        #endregion
    }
}

using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.ShutterPositioning
{
    internal class ShutterPositioningEnableOperationState : InverterStateBase
    {
        #region Fields

        private readonly ShutterPosition shutterDestination;

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        private ShutterPosition oldShutterPosition;

        private DateTime startTime;

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

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Shutter positioning Enable Operation");
            this.InverterStatus.CommonControlWord.EnableOperation = true;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, ((AglInverterStatus)this.InverterStatus).ProfileVelocityControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
            if (this.InverterStatus is AglInverterStatus currentStatus)
            {
                this.oldShutterPosition = currentStatus.CurrentShutterPosition;
            }
            this.startTime = DateTime.UtcNow;
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Shutter Positioning Stop requested");

            this.ParentStateMachine.ChangeState(
                new ShutterPositioningDisableOperationState(
                    this.ParentStateMachine,
                    this.InverterStatus,
                    this.shutterPositionData,
                    this.Logger));
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
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(
                    new ShutterPositioningErrorState(
                        this.ParentStateMachine,
                        this.InverterStatus,
                        this.shutterPositionData,
                        this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (message.SystemIndex == this.InverterStatus.SystemIndex)
                {
                    if (this.InverterStatus is AglInverterStatus currentStatus)
                    {
                        if (//this.InverterStatus.CommonStatusWord.IsOperationEnabled &&
                            (//currentStatus.ProfileVelocityStatusWord.TargetReached &&
                                currentStatus.CurrentShutterPosition == this.shutterDestination
                                && (this.shutterPositionData.MovementDuration == 0
                                    || DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > this.shutterPositionData.MovementDuration * 100
                                    )
                                )
                            )
                        {
                            this.Logger.LogDebug($"Shutter positioning end: destination= {this.shutterDestination}; current = {currentStatus.CurrentShutterPosition}; time = {DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds}");
                            this.ParentStateMachine.ChangeState(new ShutterPositioningDisableOperationState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));
                            returnValue = true;
                        }
                        else if (this.oldShutterPosition != currentStatus.CurrentShutterPosition)
                        {
                            this.oldShutterPosition = currentStatus.CurrentShutterPosition;

                            this.shutterPositionData.ShutterPosition = this.oldShutterPosition;

                            var endNotification = new FieldNotificationMessage(
                                this.shutterPositionData,
                                "Shutter Positioning executing",
                                FieldMessageActor.DeviceManager,
                                FieldMessageActor.InverterDriver,
                                FieldMessageType.ShutterPositioning,
                                MessageStatus.OperationExecuting,
                                this.InverterStatus.SystemIndex);

                            this.Logger.LogTrace($"1:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                            this.ParentStateMachine.PublishNotificationEvent(endNotification);
                        }
                    }
                }
            }
            return returnValue;
        }

        #endregion
    }
}

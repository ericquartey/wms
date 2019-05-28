using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.ShutterPositioning
{
    public class ShutterPositioningEnableOperationState : InverterStateBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private readonly IShutterPositioningFieldMessageData shutterPositionData;

        protected BlockingConcurrentQueue<InverterMessage> InverterCommandQueue;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningEnableOperationState(IInverterStateMachine parentStateMachine, IInverterStatusBase inverterStatus, IShutterPositioningFieldMessageData shutterPositionData, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;
            this.inverterStatus = inverterStatus;
            this.shutterPositionData = shutterPositionData;

            
        }

        #endregion

        #region Destructors

        ~ShutterPositioningEnableOperationState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");
            

            if (this.shutterPositionData.ShutterPosition != ShutterPosition.None)
            {
                this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetPosition, (short)this.shutterPositionData.ShutterPosition));
            }
            else if (this.shutterPositionData.ShutterMovementDirection != ShutterMovementDirection.None)
            {
                // Find current shutter position for current inverter
                // Validate movement request (e.g. do not allow up movement if current position is open etc..)
                // convert movement value in new current value (e.g. current position is half and movement is up new position is open
                // send new positio to inverter
                if ( this.inverterStatus is AglInverterStatus aglStatus )
                {
                    switch (aglStatus.CurrentShutterPosition)
                    {
                        case ShutterPosition.Opened:
                            if (this.shutterPositionData.ShutterMovementDirection == ShutterMovementDirection.Up)
                            {
                                var errorOpenedShutterPosition = new FieldNotificationMessage(this.shutterPositionData,
                                    "Shutter Position Already Opened Error",
                                    FieldMessageActor.Any,
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageType.ShutterPositioning,
                                    MessageStatus.OperationError,
                                    ErrorLevel.Error);

                                this.ParentStateMachine.PublishNotificationEvent(errorOpenedShutterPosition);
                            }
                            else
                            {
                                switch (this.shutterPositionData.ShutterType)
                                {
                                    case ShutterType.NoType:
                                        var errorShutter1Type = new FieldNotificationMessage(this.shutterPositionData,
                                            "Shutter Type Error",
                                            FieldMessageActor.Any,
                                            FieldMessageActor.InverterDriver,
                                            FieldMessageType.ShutterPositioning,
                                            MessageStatus.OperationError,
                                            ErrorLevel.Error);

                                        this.ParentStateMachine.PublishNotificationEvent(errorShutter1Type);
                                        break;

                                    case ShutterType.Shutter2Type:
                                        ((AglInverterStatus)this.inverterStatus).CurrentShutterPosition = ShutterPosition.Closed;
                                        break;

                                    case ShutterType.Shutter3Type:
                                        ((AglInverterStatus)this.inverterStatus).CurrentShutterPosition = ShutterPosition.Half;
                                        break;
                                }
                                this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetPosition, ((AglInverterStatus)this.inverterStatus).CurrentShutterPosition));
                            }
                            break;

                        case ShutterPosition.Half:
                            ((AglInverterStatus)this.inverterStatus).CurrentShutterPosition = this.shutterPositionData.ShutterMovementDirection == ShutterMovementDirection.Up ? ShutterPosition.Opened : ShutterPosition.Closed;
                            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetPosition, ((AglInverterStatus)this.inverterStatus).CurrentShutterPosition));
                            break;

                        case ShutterPosition.Closed:
                            if (this.shutterPositionData.ShutterMovementDirection == ShutterMovementDirection.Down)
                            {
                                 var errorClosedShutterPosition = new FieldNotificationMessage(this.shutterPositionData,
                                    "Shutter Position Already Closed Error",
                                    FieldMessageActor.Any,
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageType.ShutterPositioning,
                                    MessageStatus.OperationError,
                                    ErrorLevel.Error);

                                 this.ParentStateMachine.PublishNotificationEvent(errorClosedShutterPosition);
                            }
                            else
                            {
                                switch (this.shutterPositionData.ShutterType)
                                {
                                    case ShutterType.NoType:
                                        var errorShutter1Type = new FieldNotificationMessage(this.shutterPositionData,
                                            "Shutter Type Error",
                                            FieldMessageActor.Any,
                                            FieldMessageActor.InverterDriver,
                                            FieldMessageType.ShutterPositioning,
                                            MessageStatus.OperationError,
                                            ErrorLevel.Error);

                                        this.ParentStateMachine.PublishNotificationEvent(errorShutter1Type);
                                        break;

                                    case ShutterType.Shutter2Type:
                                        ((AglInverterStatus)this.inverterStatus).CurrentShutterPosition = ShutterPosition.Opened;
                                        break;

                                    case ShutterType.Shutter3Type:
                                        ((AglInverterStatus)this.inverterStatus).CurrentShutterPosition = ShutterPosition.Half;
                                        break;
                                }
                                this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetPosition, ((AglInverterStatus)this.inverterStatus).CurrentShutterPosition));
                            }
                            break;

                        default:
                            var errorNotification = new FieldNotificationMessage(this.shutterPositionData,
                                "Invalid Shutter position",
                                FieldMessageActor.Any,
                                FieldMessageActor.InverterDriver,
                                FieldMessageType.ShutterPositioning,
                                MessageStatus.OperationError,
                                ErrorLevel.Error);

                            this.ParentStateMachine.PublishNotificationEvent(errorNotification);
                            break;
                    }

                }
            }
            else
            {
                var errorNotification = new FieldNotificationMessage(this.shutterPositionData,
                    "Invalid Shutter Movement Direction",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.ShutterPositioning,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);
                      
                this.ParentStateMachine.PublishNotificationEvent(errorNotification);
            }

        }

        /// <inheritdoc/>
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            var returnValue = false;

            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");
            this.logger.LogTrace($"3:message={message}:Parameter ID={message.ParameterId}");

            switch (message.ParameterId)
            {
                case (InverterParameterId.ShutterTargetPosition):
                    this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetVelocityParam, this.shutterPositionData.SpeedRate));
                    break;

                case (InverterParameterId.ShutterTargetVelocityParam):
                    if (this.inverterStatus is AglInverterStatus currentStatus)
                    {
                        currentStatus.ProfileVelocityControlWord.EnableOperation = true;
                    }
                    var inverterMessage = new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, ((AglInverterStatus)this.inverterStatus).ProfileVelocityControlWord.Value);
                    this.logger.LogTrace($"3:inverterMessage={inverterMessage}");
                    this.ParentStateMachine.EnqueueMessage(inverterMessage);
                    returnValue = true;
                    break;

                default:
                    break;
            }
            
            return returnValue;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.inverterStatus, this.shutterPositionData, this.logger));
            }

            this.inverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (this.inverterStatus is AglInverterStatus currentStatus)
            {
                if (this.inverterStatus.CommonStatusWord.IsOperationEnabled && currentStatus.ProfileVelocityStatusWord.TargetReached)
                {
                    this.ParentStateMachine.ChangeState(new ShutterPositioningDisableOperationState(this.ParentStateMachine, this.inverterStatus, this.shutterPositionData, this.logger));
                    returnValue = true;
                }
            }

            return returnValue;
        }

        protected override void Dispose(bool disposing)
        {
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

        #endregion
    }
}

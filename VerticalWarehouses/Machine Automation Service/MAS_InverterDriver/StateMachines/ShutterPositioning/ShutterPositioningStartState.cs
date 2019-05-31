using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.ShutterPositioning
{
    public class ShutterPositioningStartState : InverterStateBase
    {
        #region Fields

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningStartState(IInverterStateMachine parentStateMachine, IInverterStatusBase inverterStatus, IInverterShutterPositioningFieldMessageData shutterPositionData, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;
            this.inverterStatus = inverterStatus;
            this.shutterPositionData = shutterPositionData;

            
        }

        #endregion

        #region Destructors

        ~ShutterPositioningStartState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");

            //TEMP Shutter Type neesds to be controlled.
            /*if (this.shutterPositionData.ShutterPosition != ShutterPosition.None)
            {
                this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetPosition, (ushort)this.shutterPositionData.ShutterPosition));
            }

            else if (this.shutterPositionData.ShutterMovementDirection != ShutterMovementDirection.None)
            {
                if (this.inverterStatus is AglInverterStatus aglStatus)
                {
                    switch (aglStatus.CurrentShutterPosition)
                    {
                        case ShutterPosition.Opened:
                            if (this.shutterPositionData.ShutterMovementDirection == ShutterMovementDirection.Up)
                            {
                                var errorOpenedShutterPosition = new FieldNotificationMessage(this.shutterPositionData,
                                    "Shutter Position Already Opened",
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
                                this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetPosition, (ushort)((AglInverterStatus)this.inverterStatus).CurrentShutterPosition));
                            }
                            break;

                        case ShutterPosition.Half:
                            ((AglInverterStatus)this.inverterStatus).CurrentShutterPosition = this.shutterPositionData.ShutterMovementDirection == ShutterMovementDirection.Up ? ShutterPosition.Opened : ShutterPosition.Closed;
                            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetPosition, (ushort)((AglInverterStatus)this.inverterStatus).CurrentShutterPosition));
                            break;

                        case ShutterPosition.Closed:
                            if (this.shutterPositionData.ShutterMovementDirection == ShutterMovementDirection.Down)
                            {
                                var errorClosedShutterPosition = new FieldNotificationMessage(this.shutterPositionData,
                                   "Shutter Position Already Closed",
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
                                this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetPosition, (ushort)((AglInverterStatus)this.inverterStatus).CurrentShutterPosition));
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
            }*/

            var message = new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetPosition, (ushort)this.shutterPositionData.ShutterPosition);
            var byteMessage = message.GetWriteMessage();
            this.ParentStateMachine.EnqueueMessage(message);

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
                    var data = new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetVelocityParam, this.shutterPositionData.SpeedRate);
                    var byteData = data.GetWriteMessage();

                    this.ParentStateMachine.EnqueueMessage(data);
                    break;

                case (InverterParameterId.ShutterTargetVelocityParam):

                    var byteDataReceived = message.GetWriteMessage();
                    this.ParentStateMachine.ChangeState(new ShutterPositioningEnableVoltageState(this.ParentStateMachine, this.inverterStatus, this.shutterPositionData, this.logger));

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

            return false;
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

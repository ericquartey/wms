using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.ShutterPositioning
{
    public class ShutterPositioningStartState : InverterStateBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningStartState(IInverterStateMachine parentStateMachine, IInverterStatusBase inverterStatus, IInverterShutterPositioningFieldMessageData shutterPositionData, ILogger logger)
        {
            logger.LogTrace("1:Method Start");

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

        public override void Release()
        {
        }

        public override void Start()
        {
            this.logger.LogTrace("1:Method Start");

            if (this.inverterStatus is IAglInverterStatus aglStatus)
            {
                if (aglStatus.ShutterType == ShutterType.Shutter2Type && this.shutterPositionData.ShutterPosition == ShutterPosition.Half)
                {
                    this.logger.LogTrace($"2:Error unavailable position for shutter {this.inverterStatus.SystemIndex}");

                    var errorShutterPosition = new FieldNotificationMessage(
                        this.shutterPositionData,
                        "Shutter Position destination is not available",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.ShutterPositioning,
                        MessageStatus.OperationError,
                        ErrorLevel.Error);

                    this.ParentStateMachine.PublishNotificationEvent(errorShutterPosition);

                    return;
                }

                if (aglStatus.CurrentShutterPosition == this.shutterPositionData.ShutterPosition)
                {
                    this.logger.LogTrace($"3:Warning position already reached for shutter {this.inverterStatus.SystemIndex}");

                    // TEMP If the shutter is already in the shutter position target, don't notify an error condition
                    var messageShutterPosition = new FieldNotificationMessage(
                        this.shutterPositionData,
                        "Shutter Position is already reached",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.ShutterPositioning,
                        MessageStatus.OperationEnd,
                        ErrorLevel.NoError);

                    this.ParentStateMachine.PublishNotificationEvent(messageShutterPosition);

                    return;
                }
            }

            var message = new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetPosition, (ushort)this.shutterPositionData.ShutterPosition);
            var byteMessage = message.GetWriteMessage();
            this.ParentStateMachine.EnqueueMessage(message);
        }

        /// <inheritdoc/>
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            var returnValue = false;

            this.logger.LogTrace($"1:message={message}:Is Error={message.IsError}");
            this.logger.LogTrace($"2:message={message}:Parameter ID={message.ParameterId}");

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
            this.logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

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

using Ferretto.VW.CommonUtils.Messages.Enumerations;
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

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        #endregion

        #region Constructors

        public ShutterPositioningStartState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            IInverterShutterPositioningFieldMessageData shutterPositionData,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
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
            this.Logger.LogTrace("1:Method Start");

            if (this.InverterStatus is IAglInverterStatus aglStatus)
            {
                if (aglStatus.ShutterType == ShutterType.Shutter2Type && this.shutterPositionData.ShutterPosition == ShutterPosition.Half)
                {
                    this.Logger.LogTrace($"2:Error unavailable position for shutter {this.InverterStatus.SystemIndex}");

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
                    this.Logger.LogTrace($"3:Warning position {this.shutterPositionData.ShutterPosition} already reached for shutter {this.InverterStatus.SystemIndex}");

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

            var message = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetPosition, (ushort)this.shutterPositionData.ShutterPosition);
            var byteMessage = message.GetWriteMessage();
            this.Logger.LogTrace($"4:inverterMessage={message}");
            this.ParentStateMachine.EnqueueMessage(message);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Stop");

            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger, true));
        }

        /// <inheritdoc/>
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            var returnValue = false;

            //this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");
            //this.Logger.LogTrace($"2:message={message}:Parameter ID={message.ParameterId}");

            switch (message.ParameterId)
            {
                case InverterParameterId.ShutterTargetPosition:
                    var data = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetVelocityParam, this.shutterPositionData.SpeedRate);
                    var byteData = data.GetWriteMessage();

                    this.Logger.LogTrace($"5:inverterMessage={data}");
                    this.ParentStateMachine.EnqueueMessage(data);
                    break;

                case InverterParameterId.ShutterTargetVelocityParam:

                    var byteDataReceived = message.GetWriteMessage();
                    this.ParentStateMachine.ChangeState(new ShutterPositioningEnableVoltageState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));

                    returnValue = true;
                    break;

                default:
                    break;
            }

            return returnValue;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return false;
        }

        #endregion
    }
}

using System;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
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
    public class ShutterPositioningEnableVoltageState : InverterStateBase
    {
        #region Fields

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        #endregion

        #region Constructors

        public ShutterPositioningEnableVoltageState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            IInverterShutterPositioningFieldMessageData shutterPositionData,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.shutterPositionData = shutterPositionData;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~ShutterPositioningEnableVoltageState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.InverterStatus.CommonControlWord.EnableVoltage = true;
            this.InverterStatus.CommonControlWord.QuickStop = true;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, ((AglInverterStatus)this.InverterStatus).ProfileVelocityControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);

            Enum.TryParse(this.InverterStatus.SystemIndex.ToString(), out InverterIndex inverterIndex);

            var notificationMessageData = new InverterPowerOnFieldMessageData(inverterIndex);
            var notificationMessage = new FieldNotificationMessage(
                notificationMessageData,
                $"Power On Inverter {inverterIndex}",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterPowerOn,
                MessageStatus.OperationStart);

            this.Logger.LogTrace($"2:Publishing Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));
            }

            this.InverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (this.InverterStatus.CommonStatusWord.IsVoltageEnabled &
                this.InverterStatus.CommonStatusWord.IsQuickStopTrue &
                this.InverterStatus.CommonStatusWord.IsReadyToSwitchOn
                )
            {
                this.ParentStateMachine.ChangeState(new ShutterPositioningSwitchOnState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));

                returnValue = true;
            }

            return returnValue;
        }

        #endregion
    }
}

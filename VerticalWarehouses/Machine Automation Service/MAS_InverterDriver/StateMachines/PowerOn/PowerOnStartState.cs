using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOn
{
    public class PowerOnStartState : InverterStateBase
    {
        #region Constructors

        public PowerOnStartState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.Logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~PowerOnStartState()
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
            this.InverterStatus.CommonControlWord.EnableVoltage = true;
            this.InverterStatus.CommonControlWord.QuickStop = true;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, this.InverterStatus.CommonControlWord.Value);

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

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");
            //TEMP Add your implementation code here
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
                this.ParentStateMachine.ChangeState(new PowerOnErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }

            //this.InverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (this.InverterStatus.CommonStatusWord.IsVoltageEnabled &
                this.InverterStatus.CommonStatusWord.IsQuickStopTrue &
                this.InverterStatus.CommonStatusWord.IsReadyToSwitchOn
                )
            {
                this.ParentStateMachine.ChangeState(new PowerOnSwitchOnState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                returnValue = true;
            }

            return returnValue;
        }

        #endregion
    }
}

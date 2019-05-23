using System;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.PowerOn
{
    public class PowerOnStartState : InverterStateBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerOnStartState(IInverterStateMachine parentStateMachine, IInverterStatusBase inverterStatus, ILogger logger)
        {
            this.logger = logger;
            this.logger.LogDebug("1:Method Start");

            this.ParentStateMachine = parentStateMachine;
            this.inverterStatus = inverterStatus;

            
        }

        #endregion

        #region Destructors

        ~PowerOnStartState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");

            this.inverterStatus.CommonControlWord.EnableVoltage = true;
            this.inverterStatus.CommonControlWord.QuickStop = true;

            var inverterMessage = new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, this.inverterStatus.CommonControlWord.Value);

            this.logger.LogTrace($"2:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);

            Enum.TryParse(this.inverterStatus.SystemIndex.ToString(), out InverterIndex inverterIndex);

            var notificationMessageData = new InverterPowerOnFieldMessageData(inverterIndex);
            var notificationMessage = new FieldNotificationMessage(notificationMessageData,
                $"Power On Inverter {inverterIndex}",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterPowerOn,
                MessageStatus.OperationStart);

            this.logger.LogTrace($"3:Publishing Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);

            
        }

        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new PowerOnErrorState(this.ParentStateMachine, this.inverterStatus, this.logger));
            }

            this.inverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (this.inverterStatus.CommonStatusWord.IsVoltageEnabled &
                this.inverterStatus.CommonStatusWord.IsQuickStopTrue &
                this.inverterStatus.CommonStatusWord.IsReadyToSwitchOn)
            {
                this.ParentStateMachine.ChangeState(new PowerOnSwitchOnState(this.ParentStateMachine, this.inverterStatus, this.logger));
                returnValue = true;
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

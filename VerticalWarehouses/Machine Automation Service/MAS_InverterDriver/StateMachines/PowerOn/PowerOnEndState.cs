using System;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_InverterDriver.StateMachines.PowerOn
{
    public class PowerOnEndState : InverterStateBase
    {
        #region Constructors

        public PowerOnEndState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~PowerOnEndState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            Enum.TryParse(this.InverterStatus.SystemIndex.ToString(), out InverterIndex systemIndex);

            var notificationMessageData = new InverterPowerOnFieldMessageData(systemIndex);
            var notificationMessage = new FieldNotificationMessage(
                notificationMessageData,
                "Inverter Start completed",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterPowerOn,
                MessageStatus.OperationEnd);

            this.Logger.LogTrace($"1:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return false;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        #endregion
    }
}

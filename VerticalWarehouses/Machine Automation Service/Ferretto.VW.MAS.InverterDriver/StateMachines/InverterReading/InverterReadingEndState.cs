using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.InverterReading
{
    internal class InverterReadingEndState : InverterStateBase
    {
        #region Fields

        private readonly IInverterReadingFieldMessageData inverterReadingFieldMessageData;

        #endregion

        #region Constructors

        public InverterReadingEndState(
            IInverterStateMachine parentStateMachine,
            IInverterReadingFieldMessageData inverterReadingFieldMessageData,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.inverterReadingFieldMessageData = inverterReadingFieldMessageData;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            var notificationMessageData = new InverterReadingFieldMessageData(this.inverterReadingFieldMessageData.Parameters, this.inverterReadingFieldMessageData.IsCheckInverterVersion);
            var notificationMessage = new FieldNotificationMessage(
                notificationMessageData,
                "Inverter Reading End completed",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterReading,
                MessageStatus.OperationEnd,
                this.InverterStatus.SystemIndex);

            this.Logger.LogTrace($"1:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");
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

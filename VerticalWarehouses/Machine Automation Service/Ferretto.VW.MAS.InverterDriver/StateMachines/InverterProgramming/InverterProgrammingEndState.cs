using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.InverterProgramming
{
    internal class InverterProgrammingEndState : InverterStateBase
    {
        #region Fields

        private readonly IInverterProgrammingFieldMessageData inverterProgrammingFieldMessageData;

        #endregion

        #region Constructors

        public InverterProgrammingEndState(
            IInverterStateMachine parentStateMachine,
            IInverterProgrammingFieldMessageData inverterProgrammingFieldMessageData,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.inverterProgrammingFieldMessageData = inverterProgrammingFieldMessageData;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            //var notificationMessageData = new InverterProgrammingFieldMessageData(this.inverterProgrammingFieldMessageData.Parameters, this.inverterProgrammingFieldMessageData.IsCheckInverterVersion);
            var notificationMessage = new FieldNotificationMessage(
                this.inverterProgrammingFieldMessageData,
                "Inverter programming End completed",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterProgramming,
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

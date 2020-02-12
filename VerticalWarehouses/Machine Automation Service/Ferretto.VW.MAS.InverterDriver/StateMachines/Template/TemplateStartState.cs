using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;

using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.IODriver.StateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Template
{
    internal class TemplateStartState : InverterStateBase
    {
        #region Fields

        private readonly ITemplateData templateData;

        #endregion

        #region Constructors

        public TemplateStartState(
            IInverterStateMachine parentStateMachine,
            ITemplateData templateData,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.templateData = templateData;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            //INFO Set Control Word Value or define parameter to be sent to Inverter and build the InverterMessage to be placed in inverter command queue
            this.InverterStatus.CommonControlWord.QuickStop = false;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

            var notificationMessage = new FieldNotificationMessage(
                null,
                $"Message",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterStop,
                MessageStatus.OperationStart,
                this.InverterStatus.SystemIndex);

            this.Logger.LogTrace($"2:Publishing Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            //INFO Perform required actions to stop the finite state machine, usually ending with a change state to the EndState
            this.ParentStateMachine.ChangeState(new TemplateEndState(this.ParentStateMachine, this.templateData, this.InverterStatus, this.Logger));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            //INFO This method is required to validate sent command to the inverter
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            //True means I want to request a status word.
            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            //INFO This method is required to validate command response coming from the inverter
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new TemplateErrorState(this.ParentStateMachine, this.templateData, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (!this.InverterStatus.CommonStatusWord.IsQuickStopTrue)
                {
                    this.ParentStateMachine.ChangeState(new TemplateEndState(this.ParentStateMachine, this.templateData, this.InverterStatus, this.Logger));
                    returnValue = true;
                }
            }
            //True means I got the expected response. Do not request more status words
            return returnValue;
        }

        #endregion
    }
}

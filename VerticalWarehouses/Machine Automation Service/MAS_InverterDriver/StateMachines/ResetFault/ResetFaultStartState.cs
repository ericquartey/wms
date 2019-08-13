using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.ResetFault
{
    public class ResetFaultStartState : InverterStateBase
    {
        #region Fields

        private readonly InverterIndex inverterIndex;

        #endregion

        #region Constructors

        public ResetFaultStartState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            InverterIndex inverterIndex,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.inverterIndex = inverterIndex;
        }

        #endregion

        #region Destructors

        ~ResetFaultStartState()
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
            if(this.InverterStatus == null)
            {
                this.ParentStateMachine.ChangeState(new ResetFaultEndState(this.ParentStateMachine, null, this.inverterIndex, this.Logger));
            }
            else
            {
                this.InverterStatus.CommonControlWord.FaultReset = true;

                var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, this.InverterStatus.CommonControlWord.Value);

                this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

                this.ParentStateMachine.EnqueueMessage(inverterMessage);

                var notificationMessageData = new InverterFaultFieldMessageData(this.inverterIndex);
                var notificationMessage = new FieldNotificationMessage(
                    notificationMessageData,
                    $"Fault Reset Inverter {this.inverterIndex}",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.InverterFaultReset,
                    MessageStatus.OperationStart);

                this.Logger.LogTrace($"2:Publishing Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

                this.ParentStateMachine.PublishNotificationEvent(notificationMessage);

            }
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

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new ResetFaultErrorState(this.ParentStateMachine, this.InverterStatus, this.inverterIndex, this.Logger));
            }
            else if (this.InverterStatus == null)
            {
                this.ParentStateMachine.ChangeState(new ResetFaultEndState(this.ParentStateMachine, this.InverterStatus, this.inverterIndex, this.Logger));
            }
            else if (this.InverterStatus.CommonControlWord.FaultReset)
            {
                // reset command FaultReset bit before exiting the state machine
                this.InverterStatus.CommonStatusWord.Value = message.UShortPayload;

                this.InverterStatus.CommonControlWord.FaultReset = false;

                var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, this.InverterStatus.CommonControlWord.Value);

                this.Logger.LogTrace($"2:inverterMessage={inverterMessage}");

                this.ParentStateMachine.EnqueueMessage(inverterMessage);

                returnValue = true;
            }
            else 
            {
                this.InverterStatus.CommonStatusWord.Value = message.UShortPayload;

                this.ParentStateMachine.ChangeState(new ResetFaultEndState(this.ParentStateMachine, this.InverterStatus, this.inverterIndex, this.Logger));
                returnValue = true;
            }



            return returnValue;
        }

        #endregion
    }
}

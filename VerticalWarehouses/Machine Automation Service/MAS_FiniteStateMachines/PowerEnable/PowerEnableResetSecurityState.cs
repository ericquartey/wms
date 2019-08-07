using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.PowerEnable
{
    public class PowerEnableResetSecurityState : StateBase
    {
        #region Fields

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableResetSecurityState(
            IStateMachine parentMachine,
            ILogger logger)
            : base(parentMachine, logger)
        {
        }

        #endregion

        #region Destructors

        ~PowerEnableResetSecurityState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.ResetSecurity)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState(new PowerEnableInverterStartState(this.ParentStateMachine, this.Logger));
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new PowerEnableErrorState(this.ParentStateMachine, message, this.Logger));
                        break;
                }
            }
            else if (message.Type == FieldMessageType.IoDriverException)
            {
                this.ParentStateMachine.ChangeState(new PowerEnableErrorState(this.ParentStateMachine, message, this.Logger));
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var commandMessageData = new ResetSecurityFieldMessageData();
            var commandMessage = new FieldCommandMessage(
                commandMessageData,
                $"Reset Security",
                FieldMessageActor.IoDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ResetSecurity);

            this.Logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new PowerEnableEndState(this.ParentStateMachine, this.Logger, true));
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

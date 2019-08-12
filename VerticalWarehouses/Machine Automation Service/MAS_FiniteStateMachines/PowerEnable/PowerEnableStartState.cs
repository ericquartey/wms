using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.PowerEnable
{
    public class PowerEnableStartState : StateBase
    {
        #region Fields

        private readonly bool enable;

        private readonly byte index;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableStartState(
            byte index,
            bool enable,
            IStateMachine parentMachine,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.index = index;
            this.enable = enable;
        }

        #endregion

        #region Destructors

        ~PowerEnableStartState()
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

            if (message.Type == FieldMessageType.PowerEnable)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        if (!this.enable)
                        {
                            this.ParentStateMachine.ChangeState(new PowerEnableStopInverterState(this.ParentStateMachine, InverterIndex.MainInverter, this.Logger));
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new PowerEnableResetFaultState(this.ParentStateMachine, InverterIndex.MainInverter, this.Logger));
                        }
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
            var commandMessageData = new PowerEnableFieldMessageData(this.enable);
            var commandMessage = new FieldCommandMessage(
                commandMessageData,
                $"Power Enable IO digital input",
                FieldMessageActor.IoDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.PowerEnable,
                this.index);

            this.Logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessage = new NotificationMessage(
                null,
                "Reset Security Started",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.PowerEnable,
                MessageStatus.OperationStart);

            this.Logger.LogTrace($"2:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
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

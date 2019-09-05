using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Interfaces;
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

        private readonly IPowerEnableData machineData;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableResetSecurityState(
            IStateMachine parentMachine,
            IPowerEnableData machineData)
            : base(parentMachine, machineData.Logger)
        {
            this.machineData = machineData;
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
                        this.ParentStateMachine.ChangeState(new PowerEnableEndState(this.ParentStateMachine, this.machineData));
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new PowerEnableErrorState(this.ParentStateMachine, this.machineData, message));
                        break;
                }
            }
            else if (message.Type == FieldMessageType.IoDriverException)
            {
                this.ParentStateMachine.ChangeState(new PowerEnableErrorState(this.ParentStateMachine, this.machineData, message));
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
                FieldMessageType.ResetSecurity,
                (byte)IoIndex.IoDevice1);
            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            this.Logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");
        }

        public override void Stop(StopRequestReason reason = StopRequestReason.Stop)
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new PowerEnableEndState(this.ParentStateMachine, this.machineData, true));
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

using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
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

        private readonly IPowerEnableMachineData machineData;

        private readonly IPowerEnableStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableResetSecurityState(IPowerEnableStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPowerEnableMachineData;
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
                        this.ParentStateMachine.ChangeState(new PowerEnableEndState(this.stateData));
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new PowerEnableErrorState(this.stateData));
                        break;
                }
            }
            else if (message.Type == FieldMessageType.IoDriverException)
            {
                this.stateData.FieldMessage = message;
                this.ParentStateMachine.ChangeState(new PowerEnableErrorState(this.stateData));
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

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new PowerEnableEndState(this.stateData));
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

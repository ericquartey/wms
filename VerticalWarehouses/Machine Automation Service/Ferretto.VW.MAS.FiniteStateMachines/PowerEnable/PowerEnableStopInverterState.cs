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
    public class PowerEnableStopInverterState : StateBase
    {
        #region Fields

        private InverterIndex currentInverter;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableStopInverterState(
            IStateMachine parentMachine,
            InverterIndex currentInverter,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.currentInverter = currentInverter;
        }

        #endregion

        #region Destructors

        ~PowerEnableStopInverterState()
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

            switch (message.Status)
            {
                case MessageStatus.OperationEnd:
                case MessageStatus.OperationStop:
                    this.ParentStateMachine.ChangeState(new PowerEnableEndState(this.ParentStateMachine, this.Logger));
                    break;

                case MessageStatus.OperationError:
                    this.ParentStateMachine.ChangeState(new PowerEnableErrorState(this.ParentStateMachine, message, this.Logger));
                    break;
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var inverterCommandMessageData = new InverterStopFieldMessageData(this.currentInverter);
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Stop State Machine Inverter",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterStop);
            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");
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

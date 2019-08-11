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
    public class PowerEnableResetFaultState : StateBase
    {
        #region Fields

        private bool disposed;

        private InverterIndex currentInverter;

        #endregion

        #region Constructors

        public PowerEnableResetFaultState(
            IStateMachine parentMachine,
            InverterIndex currentInverter,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.currentInverter = currentInverter;
        }

        #endregion

        #region Destructors

        ~PowerEnableResetFaultState()
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

            if (message.Type == FieldMessageType.InverterFaultReset)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        if(this.currentInverter < InverterIndex.Slave7)
                        {
                            this.currentInverter++;
                            var inverterCommandMessageData = new InverterFaultFieldMessageData(this.currentInverter);
                            var inverterCommandMessage = new FieldCommandMessage(
                                inverterCommandMessageData,
                                $"Reset Fault Inverter",
                                FieldMessageActor.InverterDriver,
                                FieldMessageActor.FiniteStateMachines,
                                FieldMessageType.InverterFaultReset);
                            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

                            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new PowerEnableResetSecurityState(this.ParentStateMachine, this.Logger));
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new PowerEnableErrorState(this.ParentStateMachine, message, this.Logger));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {

            var inverterCommandMessageData = new InverterFaultFieldMessageData(this.currentInverter);
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Reset Fault Inverter",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterFaultReset);
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

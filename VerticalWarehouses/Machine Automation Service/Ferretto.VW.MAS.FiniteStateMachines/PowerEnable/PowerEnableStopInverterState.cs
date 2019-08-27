using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.PowerEnable
{
    public class PowerEnableStopInverterState : StateBase
    {
        #region Fields

        private readonly IPowerEnableData machineData;

        private int currentInverterIndex;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableStopInverterState(
            IStateMachine parentMachine,
            IPowerEnableData machineData)
            : base(parentMachine, machineData.Logger)
        {
            this.machineData = machineData;
            this.currentInverterIndex = 0;
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

            if (message.Type == FieldMessageType.InverterStop)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.currentInverterIndex++;

                        if (this.currentInverterIndex < this.machineData.ConfiguredInverters.Count)
                        {
                            var inverterCommandMessageData = new InverterStopFieldMessageData();
                            var inverterCommandMessage = new FieldCommandMessage(
                                inverterCommandMessageData,
                                $"Reset Fault Inverter",
                                FieldMessageActor.InverterDriver,
                                FieldMessageActor.FiniteStateMachines,
                                FieldMessageType.InverterStop,
                                (byte)this.machineData.ConfiguredInverters[this.currentInverterIndex]);
                            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

                            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new PowerEnableEndState(this.ParentStateMachine, this.machineData));
                        }

                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new PowerEnableErrorState(this.ParentStateMachine, this.machineData, message));
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
            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, false, 0);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);
            this.Logger.LogTrace($"1:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            var inverterCommandMessageData = new InverterStopFieldMessageData();
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Stop State Machine Inverter",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterStop,
                (byte)this.machineData.ConfiguredInverters[this.currentInverterIndex]);
            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");
        }

        public override void Stop()
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

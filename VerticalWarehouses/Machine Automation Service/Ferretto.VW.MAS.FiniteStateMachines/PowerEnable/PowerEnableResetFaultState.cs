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
    public class PowerEnableResetFaultState : StateBase
    {

        #region Fields

        private readonly IPowerEnableMachineData machineData;

        private readonly IPowerEnableStateData stateData;

        private int currentInverterIndex;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableResetFaultState(IPowerEnableStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.RequestingBay, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPowerEnableMachineData;

            this.currentInverterIndex = 0;
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
                        this.currentInverterIndex++;

                        if (this.currentInverterIndex < this.machineData.ConfiguredInverters.Count)
                        {
                            var inverterCommandMessageData = new InverterFaultFieldMessageData();
                            var inverterCommandMessage = new FieldCommandMessage(
                                inverterCommandMessageData,
                                $"Reset Fault Inverter",
                                FieldMessageActor.InverterDriver,
                                FieldMessageActor.FiniteStateMachines,
                                FieldMessageType.InverterFaultReset,
                                (byte)this.machineData.ConfiguredInverters[this.currentInverterIndex]);
                            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

                            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new PowerEnableResetSecurityState(this.stateData));
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new PowerEnableErrorState(this.stateData));
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
            var inverterCommandMessageData = new InverterFaultFieldMessageData();
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Reset Fault Inverter",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterFaultReset,
                (byte)this.machineData.ConfiguredInverters[this.currentInverterIndex]);
            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");
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

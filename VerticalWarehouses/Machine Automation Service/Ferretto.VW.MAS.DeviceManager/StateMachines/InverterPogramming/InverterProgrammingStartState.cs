using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.InverterProgramming.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.InverterPogramming
{
    internal class InverterProgrammingStartState : StateBase
    {
        #region Fields

        private readonly IInverterProgrammingMachineData machineData;

        private readonly BayNumber requestingBay;

        private readonly IInverterProgrammingStateData stateData;

        private readonly BayNumber targetBay;

        #endregion

        #region Constructors

        public InverterProgrammingStartState(IInverterProgrammingStateData stateData)
                    : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IInverterProgrammingMachineData;
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.InverterProgramming)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState(new InverterProgrammingEndState(this.stateData));
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new InverterProgrammingErrorState(this.stateData));
                        break;
                }
            }

            if (message.Type == FieldMessageType.IoDriverException)
            {
                this.stateData.FieldMessage = message;
                this.ParentStateMachine.ChangeState(new InverterProgrammingErrorState(this.stateData));
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            var commandMessage = new FieldCommandMessage(
                null,
                $"Inverter Programming Start State Field Command",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterProgramming,
                (byte)InverterIndex.None);

            foreach (var inverter in this.machineData.InverterParametersData)
            {
                var newCommandMessage = new FieldCommandMessage(commandMessage);
                newCommandMessage.DeviceIndex = (byte)inverter.InverterIndex;

                this.ParentStateMachine.PublishFieldCommandMessage(newCommandMessage);
            }

            var notificationMessage = new NotificationMessage(
                           null,
                   $"Starting inverter Programming state on inverters",
                   MessageActor.Any,
                   MessageActor.DeviceManager,
                   MessageType.InverterPowerEnable,
                   this.requestingBay,
                   this.targetBay,
                   MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"Stop with reason: {reason}");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new InverterProgrammingEndState(this.stateData));
        }

        #endregion
    }
}

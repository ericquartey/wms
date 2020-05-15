using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.InverterProgramming.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.EntityFrameworkCore.Internal;
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

        private byte currentInverterIndex;

        #endregion

        #region Constructors

        public InverterProgrammingStartState(IInverterProgrammingStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IInverterProgrammingMachineData;
        }

        #endregion

        #region Properties

        public bool IsLastInverterProgramEnded => this.machineData.InverterParametersData.Last().InverterIndex == this.currentInverterIndex;

        public InverterParametersData NextInverterParameters
        {
            get
            {
                var data = this.machineData.InverterParametersData;
                var item = data.SingleOrDefault(p => p.InverterIndex == this.currentInverterIndex);
                var index = data.IndexOf(item) + 1;
                return index < data.Count() ? data.ToList()[index] : null;
            }
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
                        var notificationMessage = new NotificationMessage(
                                   null,
                           $"Starting inverter Programming state on inverters",
                           MessageActor.Any,
                           MessageActor.DeviceManager,
                           MessageType.InverterProgramming,
                           this.requestingBay,
                           this.targetBay,
                           MessageStatus.OperationStepEnd);
                        this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

                        if (this.NextInverterParameters is InverterParametersData nextInverterParametersData)
                        {
                            this.currentInverterIndex = nextInverterParametersData.InverterIndex;
                            var nextCommandMessageData = new InverterProgrammingFieldMessageData(nextInverterParametersData.Parameters);
                            var nextCommandMessage = new FieldCommandMessage(
                                nextCommandMessageData,
                                $"Inverter Programming Start State Field Command",
                                FieldMessageActor.InverterDriver,
                                FieldMessageActor.DeviceManager,
                                FieldMessageType.InverterProgramming,
                                nextInverterParametersData.InverterIndex);
                            this.ParentStateMachine.PublishFieldCommandMessage(nextCommandMessage);
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new InverterProgrammingEndState(this.stateData, this.Logger));
                            var notificationEndMessage = new NotificationMessage(
                                              null,
                                      $"Starting inverter Programming state on inverters",
                                      MessageActor.Any,
                                      MessageActor.DeviceManager,
                                      MessageType.InverterProgramming,
                                      this.requestingBay,
                                      this.targetBay,
                                      MessageStatus.OperationEnd);
                            this.ParentStateMachine.PublishNotificationMessage(notificationEndMessage);
                        }

                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new InverterProgrammingErrorState(this.stateData, this.Logger));
                        break;
                }
            }

            if (message.Type == FieldMessageType.IoDriverException)
            {
                this.stateData.FieldMessage = message;
                this.ParentStateMachine.ChangeState(new InverterProgrammingErrorState(this.stateData, this.Logger));
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            var mainInverter = this.machineData.InverterParametersData.First();
            var commandMessageData = new InverterProgrammingFieldMessageData(mainInverter.Parameters);
            var commandMessage = new FieldCommandMessage(
                commandMessageData,
                $"Inverter Programming Start State Field Command",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterProgramming,
                mainInverter.InverterIndex);
            this.currentInverterIndex = mainInverter.InverterIndex;

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessage = new NotificationMessage(
                           null,
                   $"Starting inverter Programming state on inverters",
                   MessageActor.Any,
                   MessageActor.DeviceManager,
                   MessageType.InverterProgramming,
                   this.requestingBay,
                   this.targetBay,
                   MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"Stop with reason: {reason}");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new InverterProgrammingEndState(this.stateData, this.Logger));
        }

        #endregion
    }
}

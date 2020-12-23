using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.InverterReading.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.InverterReading
{
    internal class InverterReadingStartState : StateBase
    {
        #region Fields

        private readonly IInverterReadingMachineData machineData;

        private readonly BayNumber requestingBay;

        private readonly IInverterReadingStateData stateData;

        private readonly BayNumber targetBay;

        private byte currentInverterIndex;

        #endregion

        #region Constructors

        public InverterReadingStartState(IInverterReadingStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IInverterReadingMachineData;
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

            if (message.Type == FieldMessageType.InverterReading)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        var notificationMessage = new NotificationMessage(
                                       null,
                               $"Starting inverter Reading state on inverters",
                               MessageActor.Any,
                               MessageActor.DeviceManager,
                               MessageType.InverterReading,
                               this.requestingBay,
                               this.targetBay,
                               MessageStatus.OperationStepEnd);
                        this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                        if (this.NextInverterParameters is InverterParametersData nextInverterParametersData)
                        {
                            this.currentInverterIndex = nextInverterParametersData.InverterIndex;
                            var nextCommandMessageData = new InverterReadingFieldMessageData(nextInverterParametersData.Parameters, nextInverterParametersData.IsCheckInverterVersion);
                            var nextCommandMessage = new FieldCommandMessage(
                                nextCommandMessageData,
                                $"Inverter Reading Start State Field Command",
                                FieldMessageActor.InverterDriver,
                                FieldMessageActor.DeviceManager,
                                FieldMessageType.InverterReading,
                                nextInverterParametersData.InverterIndex);
                            this.ParentStateMachine.PublishFieldCommandMessage(nextCommandMessage);
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new InverterReadingEndState(this.stateData, this.Logger));
                            var notificationEndMessage = new NotificationMessage(
                                              null,
                                      $"Starting inverter Reading state on inverters",
                                      MessageActor.Any,
                                      MessageActor.DeviceManager,
                                      MessageType.InverterReading,
                                      this.requestingBay,
                                      this.targetBay,
                                      MessageStatus.OperationEnd);
                            this.ParentStateMachine.PublishNotificationMessage(notificationEndMessage);
                        }

                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new InverterReadingErrorState(this.stateData, this.Logger));
                        break;
                }
            }

            if (message.Type == FieldMessageType.IoDriverException)
            {
                this.stateData.FieldMessage = message;
                this.ParentStateMachine.ChangeState(new InverterReadingErrorState(this.stateData, this.Logger));
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            var mainInverter = this.machineData.InverterParametersData.First();
            var commandMessageData = new InverterReadingFieldMessageData(mainInverter.Parameters, mainInverter.IsCheckInverterVersion);
            var commandMessage = new FieldCommandMessage(
                commandMessageData,
                $"Inverter Reading Start State Field Command",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterReading,
                mainInverter.InverterIndex);
            this.currentInverterIndex = mainInverter.InverterIndex;

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessage = new NotificationMessage(
                           null,
                   $"Starting inverter Reading state on inverters",
                   MessageActor.Any,
                   MessageActor.DeviceManager,
                   MessageType.InverterReading,
                   this.requestingBay,
                   this.targetBay,
                   MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"Stop with reason: {reason}");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new InverterReadingEndState(this.stateData, this.Logger));
        }

        #endregion
    }
}

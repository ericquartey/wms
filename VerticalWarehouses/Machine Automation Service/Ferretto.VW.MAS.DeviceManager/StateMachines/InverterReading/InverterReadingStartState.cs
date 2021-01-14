using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.InverterReading.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
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

                if (this.machineData.InverterParametersData.Any(s => s.InverterIndex > this.currentInverterIndex))
                {
                    for (int i = this.currentInverterIndex; i < (int)InverterIndex.Slave7; i++)
                    {
                        if (data.Any(p => p.InverterIndex == i))
                        {
                            return data.Single(p => p.InverterIndex == i);
                        }
                    }

                    return null;
                }
                else
                {
                    return null;
                }
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
                               new InverterReadingMessageData(this.machineData.InverterParametersData),
                               $"Starting inverter Reading state on inverters",
                               MessageActor.Any,
                               MessageActor.DeviceManager,
                               MessageType.InverterReading,
                               this.requestingBay,
                               this.targetBay,
                               MessageStatus.OperationStepEnd);
                        this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

                        if (this.NextInverterParameters is InverterParametersData nextInverterParametersData &&
                            nextInverterParametersData != null)
                        {
                            this.currentInverterIndex = nextInverterParametersData.InverterIndex;
                            var nextCommandMessageData = new InverterReadingFieldMessageData(nextInverterParametersData.Parameters, nextInverterParametersData.IsCheckInverterVersion, nextInverterParametersData.InverterIndex);
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
                            this.stateData.FieldMessage = message;
                            this.ParentStateMachine.ChangeState(new InverterReadingEndState(this.stateData, this.Logger));
                        }

                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new InverterReadingErrorState(this.stateData, this.Logger));
                        break;
                }
            }

            //if (message.Type == FieldMessageType.IoDriverException ||
            //    message.Type == FieldMessageType.InverterException)
            //{
            //    this.stateData.FieldMessage = message;
            //    this.ParentStateMachine.ChangeState(new InverterReadingErrorState(this.stateData, this.Logger));
            //}
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            var notificationMessage = new NotificationMessage(
                   new InverterReadingMessageData(this.machineData.InverterParametersData),
                   $"Starting inverter Reading state on inverters",
                   MessageActor.Any,
                   MessageActor.DeviceManager,
                   MessageType.InverterReading,
                   this.requestingBay,
                   this.targetBay,
                   MessageStatus.OperationStart);
            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            var mainInverter = this.machineData.InverterParametersData.OrderBy(s => s.InverterIndex).FirstOrDefault();
            var commandMessageData = new InverterReadingFieldMessageData(mainInverter.Parameters, mainInverter.IsCheckInverterVersion, mainInverter.InverterIndex);
            var commandMessage = new FieldCommandMessage(
                commandMessageData,
                $"Inverter Reading Start State Field Command",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterReading,
                mainInverter.InverterIndex);
            this.currentInverterIndex = mainInverter.InverterIndex;

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);
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

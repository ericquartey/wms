using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.InverterProgramming.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
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

        public InverterProgrammingStartState(IInverterProgrammingStateData stateData,
            ILogger logger)
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

            if (message.Type == FieldMessageType.InverterProgramming)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:

                        var notificationMessage = new NotificationMessage(
                               new InverterProgrammingMessageData(this.machineData.InverterParametersData),
                               $"Starting inverter Programming state on inverters",
                               MessageActor.Any,
                               MessageActor.DeviceManager,
                               MessageType.InverterProgramming,
                               this.requestingBay,
                               this.targetBay,
                               MessageStatus.OperationStepEnd);
                        this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

                        if (this.NextInverterParameters is InverterParametersData nextInverterParametersData &&
                            nextInverterParametersData != null)
                        {
                            this.currentInverterIndex = nextInverterParametersData.InverterIndex;
                            var nextCommandMessageData = new InverterProgrammingFieldMessageData(nextInverterParametersData);
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
                            this.stateData.FieldMessage = message;
                            this.ParentStateMachine.ChangeState(new InverterProgrammingEndState(this.stateData, this.Logger));
                        }

                        break;

                    case MessageStatus.OperationError:

                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new InverterProgrammingErrorState(this.stateData, this.Logger));
                        break;
                }
            }

            //if (message.Type == FieldMessageType.IoDriverException ||
            //    message.Type == FieldMessageType.InverterException ||
            //    message.Type == FieldMessageType.InverterError)
            //{
            //    this.stateData.FieldMessage = message;
            //    this.ParentStateMachine.ChangeState(new InverterProgrammingErrorState(this.stateData, this.Logger));
            //}
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverters {this.machineData.InverterParametersData.Count()}");
            var notificationMessage = new NotificationMessage(
                   new InverterProgrammingMessageData(this.machineData.InverterParametersData),
                   $"Starting inverter Programming state on inverters",
                   MessageActor.Any,
                   MessageActor.DeviceManager,
                   MessageType.InverterProgramming,
                   this.requestingBay,
                   this.targetBay,
                   MessageStatus.OperationStart);
            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            var mainInverter = this.machineData.InverterParametersData.OrderBy(s => s.InverterIndex).FirstOrDefault();
            var commandMessageData = new InverterProgrammingFieldMessageData(mainInverter);
            var commandMessage = new FieldCommandMessage(
                commandMessageData,
                $"Inverter Programming Start State Field Command",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterProgramming,
                mainInverter.InverterIndex);
            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            this.currentInverterIndex = mainInverter.InverterIndex;
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

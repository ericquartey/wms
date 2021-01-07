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

        private bool isCheckedVersion;

        private bool next;

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

                if (this.next)
                {
                    this.next = false;
                    if (this.machineData.InverterParametersData.Any(s => s.InverterIndex > this.currentInverterIndex))
                    {
                        for (int i = this.currentInverterIndex; i < (int)InverterIndex.Slave7; i++)
                        {
                            if (data.Any(p => p.InverterIndex == i))
                            {
                                return data.FirstOrDefault(p => p.InverterIndex == i);
                            }
                        }

                        return null;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (this.isCheckedVersion)
                    {
                        this.next = true;
                        return data.LastOrDefault(p => p.InverterIndex == this.currentInverterIndex);
                    }
                    else
                    {
                        this.isCheckedVersion = true;
                        return data.FirstOrDefault(p => p.InverterIndex == this.currentInverterIndex);
                    }
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
                               new InverterReadingMessageData(this.machineData.InverterParametersData),
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

                        var notificationErrorMessage = new NotificationMessage(
                               new InverterReadingMessageData(this.machineData.InverterParametersData),
                                      $"Starting inverter Reading state on inverters",
                                      MessageActor.Any,
                                      MessageActor.DeviceManager,
                                      MessageType.InverterReading,
                                      this.requestingBay,
                                      this.targetBay,
                                      MessageStatus.OperationError);
                        this.ParentStateMachine.PublishNotificationMessage(notificationErrorMessage);
                        break;
                }
            }

            if (message.Type == FieldMessageType.IoDriverException)
            {
                this.stateData.FieldMessage = message;
                this.ParentStateMachine.ChangeState(new InverterReadingErrorState(this.stateData, this.Logger));

                var notificationErrorMessage = new NotificationMessage(
                       new InverterReadingMessageData(this.machineData.InverterParametersData),
                              $"Starting inverter Reading state on inverters",
                              MessageActor.Any,
                              MessageActor.DeviceManager,
                              MessageType.InverterReading,
                              this.requestingBay,
                              this.targetBay,
                              MessageStatus.OperationError);
                this.ParentStateMachine.PublishNotificationMessage(notificationErrorMessage);
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            var mainInverter = this.machineData.InverterParametersData.Where(s => s.InverterIndex == (byte)InverterIndex.MainInverter).FirstOrDefault();
            var commandMessageData = new InverterReadingFieldMessageData(mainInverter.Parameters, mainInverter.IsCheckInverterVersion);
            var commandMessage = new FieldCommandMessage(
                commandMessageData,
                $"Inverter Reading Start State Field Command",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterReading,
                mainInverter.InverterIndex);
            this.currentInverterIndex = mainInverter.InverterIndex;
            this.isCheckedVersion = true;

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

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
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"Stop with reason: {reason}");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new InverterReadingEndState(this.stateData, this.Logger));

            var notificationErrorMessage = new NotificationMessage(
                               new InverterReadingMessageData(this.machineData.InverterParametersData),
                                      $"Starting inverter Reading state on inverters",
                                      MessageActor.Any,
                                      MessageActor.DeviceManager,
                                      MessageType.InverterReading,
                                      this.requestingBay,
                                      this.targetBay,
                                      MessageStatus.OperationStop);
            this.ParentStateMachine.PublishNotificationMessage(notificationErrorMessage);
        }

        #endregion
    }
}

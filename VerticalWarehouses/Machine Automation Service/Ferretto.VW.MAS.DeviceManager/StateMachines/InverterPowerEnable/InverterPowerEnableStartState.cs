using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.InverterPowerEnable.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.DeviceManager.InverterPowerEnable
{
    internal class InverterPowerEnableStartState : StateBase
    {
        #region Fields

        private readonly Dictionary<InverterIndex, MessageStatus> inverterResponses = new Dictionary<InverterIndex, MessageStatus>();

        private readonly IInverterPowerEnableMachineData machineData;

        private readonly IInverterPowerEnableStateData stateData;

        #endregion

        #region Constructors

        public InverterPowerEnableStartState(IInverterPowerEnableStateData stateData)
                    : base(stateData?.ParentMachine, stateData?.MachineData?.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData?.MachineData as IInverterPowerEnableMachineData;
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"{this.GetType().Name} ProcessFieldNotificationMessage: type: {message.Type}, status: {message.Status}, inverter {message.DeviceIndex}");

            if (message.Type != FieldMessageType.InverterPowerOn &&
                message.Type != FieldMessageType.InverterPowerOff &&
                message.Type != FieldMessageType.InverterStop)
            {
                return;
            }

            Enum.TryParse(typeof(InverterIndex), message.DeviceIndex.ToString(), out var messageInverterIndex);

            if (message.Status != MessageStatus.OperationStart &&
                message.Status != MessageStatus.OperationExecuting)
            {
                if (this.inverterResponses.TryGetValue((InverterIndex)messageInverterIndex, out var inverterResponse))
                {
                    inverterResponse = message.Status;
                    this.inverterResponses[(InverterIndex)messageInverterIndex] = inverterResponse;
                }
                else
                {
                    this.inverterResponses.Add((InverterIndex)messageInverterIndex, message.Status);
                }
            }

            if (this.inverterResponses.Values.Count == this.machineData.BayInverters.Count())
            {
                if (this.inverterResponses.Values.Any(r => r != MessageStatus.OperationEnd))
                {
                    this.stateData.FieldMessage = message;
                    this.ParentStateMachine.ChangeState(new InverterPowerEnableErrorState(this.stateData));
                }
                else
                {
                    if (this.machineData.Enable
                        && this.machineData.TargetBay == BayNumber.ElevatorBay
                        && this.machineData.BayInverters.Count() == 1)
                    {
                        this.ParentStateMachine.ChangeState(new InverterPowerEnableEncoderState(this.stateData));
                    }
                    else
                    {
                        foreach (var bayInverter in this.machineData.BayInverters.Where(x => x.Type == InverterType.Acu || x.Type == InverterType.Ang))
                        {
                            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.AxisPosition, true, 0);
                            var inverterMessage = new FieldCommandMessage(
                                inverterDataMessage,
                                "Update Inverter timer",
                                FieldMessageActor.InverterDriver,
                                FieldMessageActor.DeviceManager,
                                FieldMessageType.InverterSetTimer,
                                (byte)bayInverter.Index);

                            this.Logger.LogTrace($"1:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

                            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);
                        }

                        this.ParentStateMachine.ChangeState(new InverterPowerEnableEndState(this.stateData));
                    }
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start with requested state: {this.machineData.Enable} Bay: {this.machineData.TargetBay}");

            var commandMessage = new FieldCommandMessage(
                null,
                $"InverterPowerEnable Start State Field Command",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                this.machineData.Enable ? FieldMessageType.InverterPowerOn : FieldMessageType.InverterPowerOff,
                (byte)InverterIndex.None);

            foreach (var bayInverter in this.machineData.BayInverters)
            {
                var newCommandMessage = new FieldCommandMessage(commandMessage);
                newCommandMessage.DeviceIndex = (byte)bayInverter.Index;

                this.ParentStateMachine.PublishFieldCommandMessage(newCommandMessage);
            }

            var notificationMessage = new NotificationMessage(
                null,
                $"Starting power state change on inverters belonging to bay {this.machineData.TargetBay} to {this.machineData.Enable}",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.InverterPowerEnable,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"Stop with reason: {reason}");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new InverterPowerEnableEndState(this.stateData));
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.ResetFault.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.ResetFault
{
    internal class ResetFaultStartState : StateBase
    {
        #region Fields

        private readonly Dictionary<InverterIndex, MessageStatus> inverterResponses = new Dictionary<InverterIndex, MessageStatus>();

        private readonly IResetFaultMachineData machineData;

        private readonly IResetFaultStateData stateData;

        #endregion

        #region Constructors

        public ResetFaultStartState(IResetFaultStateData stateData)
            : base(stateData?.ParentMachine, stateData?.MachineData?.Logger)
        {
            this.stateData = stateData ?? throw new ArgumentNullException(nameof(stateData));
            this.machineData = stateData.MachineData as IResetFaultMachineData ?? throw new ArgumentNullException(nameof(stateData));
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            if (message.Type != FieldMessageType.InverterFaultReset)
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
                    this.ParentStateMachine.ChangeState(new ResetFaultErrorState(this.stateData));
                }
                else
                {
                    this.ParentStateMachine.ChangeState(new ResetFaultEndState(this.stateData));
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            // do nothing
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name}");
            var commandMessage = new FieldCommandMessage(
                null,
                $"Reset Inverter Fault",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterFaultReset,
                (byte)InverterIndex.None);

            foreach (var bayInverter in this.machineData.BayInverters)
            {
                var newCommandMessage = new FieldCommandMessage(commandMessage);
                newCommandMessage.DeviceIndex = (byte)bayInverter.Index;

                this.ParentStateMachine.PublishFieldCommandMessage(newCommandMessage);
            }

            var notificationMessage = new NotificationMessage(
                null,
                $"Starting Fault reset on inverters belonging to bay {this.machineData.TargetBay}",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.InverterFaultReset,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Start");
            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new ResetFaultEndState(this.stateData));
        }

        #endregion
    }
}

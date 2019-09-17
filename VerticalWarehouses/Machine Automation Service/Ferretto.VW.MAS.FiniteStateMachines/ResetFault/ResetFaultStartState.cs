using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.ResetFault.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.ResetFault
{
    public class ResetFaultStartState : StateBase
    {

        #region Fields

        private readonly Dictionary<InverterIndex, MessageStatus> inverterresponses;

        private readonly IResetFaultMachineData machineData;

        private readonly IResetFaultStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public ResetFaultStartState(IResetFaultStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IResetFaultMachineData;

            this.inverterresponses = new Dictionary<InverterIndex, MessageStatus>();
        }

        #endregion

        #region Destructors

        ~ResetFaultStartState()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            if (message.Type == FieldMessageType.InverterFaultReset)
            {
                Enum.TryParse(typeof(InverterIndex), message.DeviceIndex.ToString(), out var messageInverterIndex);

                if (message.Status != MessageStatus.OperationStart &&
                    message.Status != MessageStatus.OperationExecuting)
                {
                    if (this.inverterresponses.TryGetValue((InverterIndex)messageInverterIndex, out var inverterResponse))
                    {
                        inverterResponse = message.Status;
                        this.inverterresponses[(InverterIndex)messageInverterIndex] = inverterResponse;
                    }
                    else
                    {
                        this.inverterresponses.Add((InverterIndex)messageInverterIndex, message.Status);
                    }
                }

                if (this.inverterresponses.Values.Count == this.machineData.BayInverters.Count)
                {
                    if (this.inverterresponses.Values.Any(r => r != MessageStatus.OperationEnd))
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
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            var commandMessage = new FieldCommandMessage(
                null,
                $"Reset Inverter Fault",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterFaultReset,
                (byte)InverterIndex.None);

            foreach (var bayInverter in this.machineData.BayInverters)
            {
                var newCommandMessage = new FieldCommandMessage(commandMessage);
                newCommandMessage.DeviceIndex = (byte)bayInverter;

                this.ParentStateMachine.PublishFieldCommandMessage(newCommandMessage);
            }

            var notificationMessage = new NotificationMessage(
                null,
                $"Starting Fault reset on inverters belonging to bay {this.machineData.TargetBay}",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.InverterFaultReset,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new ResetFaultEndState(this.stateData));
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

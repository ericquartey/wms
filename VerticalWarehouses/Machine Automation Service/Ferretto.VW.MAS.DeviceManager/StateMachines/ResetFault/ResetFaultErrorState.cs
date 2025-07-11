﻿using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.ResetFault.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.ResetFault
{
    internal class ResetFaultErrorState : StateBase
    {
        #region Fields

        private readonly IResetFaultMachineData machineData;

        private readonly IResetFaultStateData stateData;

        #endregion

        #region Constructors

        public ResetFaultErrorState(IResetFaultStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IResetFaultMachineData;
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            // do nothing
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            // do nothing
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name}");
            var notificationMessage = new NotificationMessage(
                null,
                $"Inverter Fault reset failed on bay {this.machineData.TargetBay}. Filed message: {this.stateData.FieldMessage.Description}",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.InverterFaultReset,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            // do nothing
        }

        #endregion
    }
}

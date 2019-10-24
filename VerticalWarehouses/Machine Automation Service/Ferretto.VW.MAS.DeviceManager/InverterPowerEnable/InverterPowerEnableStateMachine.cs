﻿using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.InverterPowerEnable.Interfaces;
using Ferretto.VW.MAS.DeviceManager.InverterPowerEnable.Models;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.InverterPowerEnable
{
    internal class InverterPowerEnableStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IInverterPowerEnableMachineData machineData;

        private bool disposed;

        #endregion

        #region Constructors

        public InverterPowerEnableStateMachine(
            CommandMessage receivedMessage,
            IEnumerable<Inverter> bayInverters,
            IEventAggregator eventAggregator,
            ILogger<DeviceManager> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machineData = new InverterPowerEnableMachineData(
                ((InverterPowerEnableMessageData)receivedMessage.Data).Enable,
                receivedMessage.RequestingBay,
                receivedMessage.TargetBay,
                bayInverters,
                eventAggregator,
                logger,
                serviceScopeFactory);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.CurrentState.ProcessCommandMessage(message);
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.CurrentState.ProcessFieldNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.CurrentState.ProcessNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            this.Logger.LogDebug($"Start with requested state: {this.machineData?.Enable}");

            var stateData = new InverterPowerEnableStateData(this, this.machineData);
            this.ChangeState(new InverterPowerEnableStartState(stateData));
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"Stop with reason: {reason}");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop(reason);
            }
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

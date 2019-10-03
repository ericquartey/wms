using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.FiniteStateMachines.InverterPowerEnable.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.InverterPowerEnable.Models;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.InverterPowerEnable
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
            ILogger<FiniteStateMachines> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.CurrentState = new EmptyState(this.Logger);

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
            lock (this.CurrentState)
            {
                var stateData = new InverterPowerEnableStateData(this, this.machineData);
                this.CurrentState = new InverterPowerEnableStartState(stateData);
                this.CurrentState?.Start();
            }
        }

        public override void Stop(StopRequestReason reason)
        {
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

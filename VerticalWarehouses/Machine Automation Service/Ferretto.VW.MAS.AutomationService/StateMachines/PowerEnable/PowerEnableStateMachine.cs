using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable.Interfaces;
using Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable.Models;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable
{
    public class PowerEnableStateMachine : StateMachineBase
    {

        #region Fields

        private readonly IPowerEnableMachineData machineData;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableStateMachine(
            CommandMessage message,
            List<Bay> configuredBays,
            IEventAggregator eventAggregator,
            ILogger<AutomationService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(message.RequestingBay, eventAggregator, logger, serviceScopeFactory)
        {
            this.CurrentState = new EmptyState(this.Logger);

            var messageData = message.Data as PowerEnableMessageData;
            this.machineData = new PowerEnableMachineData(messageData.Enable, message.RequestingBay, configuredBays, eventAggregator, logger, serviceScopeFactory);
        }

        #endregion

        #region Destructors

        ~PowerEnableStateMachine()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.CurrentState.ProcessCommandMessage(message);
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
                var stateData = new PowerEnableStateData(this, this.machineData);
                this.CurrentState = new PowerEnableStartState(stateData);
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

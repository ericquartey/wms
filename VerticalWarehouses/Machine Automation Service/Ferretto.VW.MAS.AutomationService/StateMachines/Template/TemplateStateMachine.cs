using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.StateMachines.Template.Interfaces;
using Ferretto.VW.MAS.AutomationService.StateMachines.Template.Models;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.StateMachines.Template
{
    public class TemplateStateMachine : StateMachineBase
    {

        #region Fields

        private readonly ITemplateMachineData machineData;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateStateMachine(
            BayNumber requestingBay,
            EventAggregator eventAggregator,
            ILogger<AutomationService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(requestingBay, eventAggregator, logger, serviceScopeFactory)
        {
            this.CurrentState = new EmptyState(this.Logger);

            this.machineData = new TemplateMachineData(requestingBay, eventAggregator, logger, serviceScopeFactory);
        }

        #endregion

        #region Destructors

        ~TemplateStateMachine()
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
                var stateData = new TemplateStateData(this, this.machineData);
                this.CurrentState = new TemplateStartState(stateData);
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

using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Template.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Template.Models;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Template
{
    public class TemplateStateMachine : StateMachineBase
    {

        #region Fields

        private readonly ITemplateMachineData machineData;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateStateMachine(
            CommandMessage receivedMessage,
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.CurrentState = new EmptyState(this.Logger);

            this.machineData = new TemplateMachineData(receivedMessage.RequestingBay, receivedMessage.TargetBay, eventAggregator, logger, serviceScopeFactory);
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

using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.FiniteStateMachines.Template.Interfaces;
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

        private readonly ITemplateData templateData;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateStateMachine(
            IEventAggregator eventAggregator,
            ITemplateData templateData,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory )
            : base( eventAggregator, logger, serviceScopeFactory )
        {
            this.CurrentState = new EmptyState( logger );

            this.templateData = templateData;
        }

        #endregion

        #region Destructors

        ~TemplateStateMachine()
        {
            this.Dispose( false );
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage( CommandMessage message )
        {
            this.CurrentState.ProcessCommandMessage( message );
        }

        public override void ProcessFieldNotificationMessage( FieldNotificationMessage message )
        {
            this.CurrentState.ProcessFieldNotificationMessage( message );
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage( NotificationMessage message )
        {
            this.CurrentState.ProcessNotificationMessage( message );
        }

        /// <inheritdoc/>
        /// <inheritdoc/>
        public override void Start()
        {
            this.CurrentState = new TemplateStartState( this, this.templateData, this.Logger );
            this.CurrentState?.Start();
        }

        public override void Stop()
        {
            this.Logger.LogTrace( "1:Method Start" );

            this.CurrentState.Stop();
        }

        protected override void Dispose( bool disposing )
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;
            base.Dispose( disposing );
        }

        #endregion
    }
}

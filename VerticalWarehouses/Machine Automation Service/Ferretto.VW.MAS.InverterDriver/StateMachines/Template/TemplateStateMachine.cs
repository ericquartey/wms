using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.IODriver.StateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Template
{
    public class TemplateStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private readonly ITemplateData templateData;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateStateMachine(
            ITemplateData templateData,
            IInverterStatusBase inverterStatus,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IEventAggregator eventAggregator,
            ILogger logger )
            : base( logger, eventAggregator, inverterCommandQueue )
        {
            this.templateData = templateData;
            this.inverterStatus = inverterStatus;

            this.Logger.LogTrace( "1:Method Start" );
        }

        #endregion

        #region Destructors

        ~TemplateStateMachine()
        {
            this.Dispose( false );
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.CurrentState = new TemplateStartState( this, this.templateData, this.inverterStatus, this.Logger );
            this.CurrentState?.Start();
        }

        public override void Stop()
        {
            this.CurrentState?.Stop();
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

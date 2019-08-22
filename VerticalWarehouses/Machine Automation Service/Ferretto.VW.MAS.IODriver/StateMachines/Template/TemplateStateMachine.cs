using Ferretto.VW.MAS.IODriver.StateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.Template
{
    public class TemplateStateMachine : IoStateMachineBase
    {

        #region Fields

        private readonly IoIndex index;

        private readonly IoStatus status;

        private readonly ITemplateData templateData;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateStateMachine(ITemplateData templateData,
            IoStatus status,
            IoIndex index,
            BlockingConcurrentQueue<IoWriteMessage> ioCommandQueue,
            IEventAggregator eventAggregator,
            ILogger logger)
            : base(eventAggregator, logger)
        {
            this.templateData = templateData;
            this.IoCommandQueue = ioCommandQueue;
            this.status = status;
            this.index = index;
        }

        #endregion

        #region Destructors

        ~TemplateStateMachine()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.CurrentState.Dispose();
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        public override void Start()
        {
            this.CurrentState = new TemplateStartState(this.templateData, this.status, this.index, this.Logger, this);

            this.CurrentState?.Start();
        }

        #endregion
    }
}

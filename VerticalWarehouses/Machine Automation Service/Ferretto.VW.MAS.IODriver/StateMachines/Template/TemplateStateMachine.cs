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

        private bool isDisposed;

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

        #region Methods

        public override void Start()
        {
            this.CurrentState = new TemplateStartState(this.templateData, this.status, this.index, this.Logger, this);

            this.CurrentState?.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.CurrentState is System.IDisposable disposableState)
                {
                    disposableState.Dispose();
                }
            }

            this.isDisposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}

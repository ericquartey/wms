using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.IODriver.StateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;


namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Template
{
    internal class TemplateStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private readonly ITemplateData templateData;

        #endregion

        #region Constructors

        public TemplateStateMachine(
            ITemplateData templateData,
            IInverterStatusBase inverterStatus,
            ILogger logger,
            IEventAggregator eventAggregator,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IServiceScopeFactory serviceScopeFactory)
            : base(logger, eventAggregator, inverterCommandQueue, serviceScopeFactory)
        {
            this.templateData = templateData;
            this.inverterStatus = inverterStatus;

            this.Logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.CurrentState = new TemplateStartState(this, this.templateData, this.inverterStatus, this.Logger);
            this.CurrentState?.Start();
        }

        #endregion

        //public override void Stop()
        //{
        //    this.CurrentState?.Stop();
        //}
    }
}

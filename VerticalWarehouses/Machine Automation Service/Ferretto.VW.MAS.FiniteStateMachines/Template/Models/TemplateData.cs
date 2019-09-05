using Ferretto.VW.MAS.FiniteStateMachines.Template.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.FiniteStateMachines.Template.Models
{
    public class TemplateData : ITemplateData
    {


        #region Constructors

        public TemplateData(IEventAggregator eventAggregator,
            bool enable,
            ILogger<FiniteStateMachines> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
        }

        #endregion



        #region Properties

        public IEventAggregator EventAggregator { get; }

        public ILogger<FiniteStateMachines> Logger { get; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion
    }
}

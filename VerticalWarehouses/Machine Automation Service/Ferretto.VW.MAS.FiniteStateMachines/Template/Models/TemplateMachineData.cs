using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Template.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.Template.Models
{
    public class TemplateMachineData : ITemplateMachineData
    {


        #region Constructors

        public TemplateMachineData(BayNumber requestingBay, IEventAggregator eventAggregator, ILogger<FiniteStateMachines> logger, IServiceScopeFactory serviceScopeFactory)
        {
            this.RequestingBay = requestingBay;
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;

            this.Message = "Template Data";
        }

        #endregion



        #region Properties

        public IEventAggregator EventAggregator { get; }

        public ILogger<FiniteStateMachines> Logger { get; }

        public string Message { get; }

        public BayNumber RequestingBay { get; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion
    }
}

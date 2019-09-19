using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.FiniteStateMachines.Template.Interfaces
{
    internal interface ITemplateData
    {
        #region Properties

        IEventAggregator EventAggregator { get; }

        ILogger<FiniteStateMachines> Logger { get; }

        IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion
    }
}

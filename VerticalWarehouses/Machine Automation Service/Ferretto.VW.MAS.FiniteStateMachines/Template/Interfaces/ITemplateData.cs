using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.FiniteStateMachines.Template.Interfaces
{
    public interface ITemplateData
    {


        #region Properties

        IEventAggregator EventAggregator { get; }

        ILogger<FiniteStateMachines> Logger { get; }

        BayIndex RequestingBay { get; }

        IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion
    }
}

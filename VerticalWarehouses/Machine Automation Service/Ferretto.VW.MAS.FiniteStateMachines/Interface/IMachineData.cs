using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.FiniteStateMachines.Interface
{
    public interface IMachineData
    {


        #region Properties

        IEventAggregator EventAggregator { get; }

        ILogger<FiniteStateMachines> Logger { get; }

        BayIndex RequestingBay { get; }

        IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion
    }
}

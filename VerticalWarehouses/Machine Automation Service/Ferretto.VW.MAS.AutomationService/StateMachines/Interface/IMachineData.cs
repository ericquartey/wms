using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.StateMachines.Interface
{
    public interface IMachineData
    {


        #region Properties

        IEventAggregator EventAggregator { get; }

        ILogger<AutomationService> Logger { get; }

        BayNumber RequestingBay { get; }

        IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion
    }
}

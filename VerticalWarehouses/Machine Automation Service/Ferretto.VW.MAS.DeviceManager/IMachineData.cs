using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager
{
    internal interface IMachineData
    {
        #region Properties

        IEventAggregator EventAggregator { get; }

        ILogger<DeviceManager> Logger { get; }

        BayNumber RequestingBay { get; }

        IServiceScopeFactory ServiceScopeFactory { get; }

        BayNumber TargetBay { get; }

        #endregion
    }
}

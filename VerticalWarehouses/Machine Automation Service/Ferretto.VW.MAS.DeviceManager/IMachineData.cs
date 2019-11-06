using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager
{
    internal interface IMachineData
    {
        #region Properties

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        IEventAggregator EventAggregator { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        ILogger Logger { get; }

        BayNumber RequestingBay { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        IServiceScopeFactory ServiceScopeFactory { get; }

        BayNumber TargetBay { get; }

        #endregion
    }
}

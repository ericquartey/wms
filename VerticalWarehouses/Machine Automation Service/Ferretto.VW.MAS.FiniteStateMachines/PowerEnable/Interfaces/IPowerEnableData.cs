using System.Collections.Generic;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Interfaces
{
    public interface IPowerEnableData
    {


        #region Properties

        List<InverterIndex> ConfiguredInverters { get; }

        List<IoIndex> ConfiguredIoDevices { get; }

        bool Enable { get; }

        IEventAggregator EventAggregator { get; }

        ILogger<FiniteStateMachines> Logger { get; }

        IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion
    }
}

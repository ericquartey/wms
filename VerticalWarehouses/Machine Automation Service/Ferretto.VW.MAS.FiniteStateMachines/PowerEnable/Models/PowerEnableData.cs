using System.Collections.Generic;
using Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Models
{
    internal class PowerEnableData : IPowerEnableData
    {
        #region Constructors

        public PowerEnableData(IEventAggregator eventAggregator,
            List<IoIndex> configuredIoDevices,
            List<InverterIndex> configuredInverters,
            bool enable,
            ILogger<FiniteStateMachines> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.EventAggregator = eventAggregator;
            this.ConfiguredIoDevices = configuredIoDevices;
            this.ConfiguredInverters = configuredInverters;
            this.Enable = enable;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Properties

        public List<InverterIndex> ConfiguredInverters { get; }

        public List<IoIndex> ConfiguredIoDevices { get; }

        public bool Enable { get; }

        public IEventAggregator EventAggregator { get; }

        public ILogger<FiniteStateMachines> Logger { get; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion
    }
}

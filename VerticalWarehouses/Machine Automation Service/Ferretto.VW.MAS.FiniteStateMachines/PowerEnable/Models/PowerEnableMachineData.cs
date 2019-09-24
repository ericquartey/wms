using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Models
{
    internal class PowerEnableMachineData : IPowerEnableMachineData
    {
        #region Constructors

        public PowerEnableMachineData(
            bool enable,
            BayNumber requestingBay,
            BayNumber targetBay,
            IMachineSensorsStatus machineSensorsStatus,
            List<BayNumber> configuredBays,
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.EventAggregator = eventAggregator;
            this.Enable = enable;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
            this.RequestingBay = requestingBay;
            this.TargetBay = targetBay;
            this.MachineSensorStatus = machineSensorsStatus;
            this.ConfiguredBays = configuredBays;
        }

        #endregion

        #region Properties

        public List<BayNumber> ConfiguredBays { get; }

        public bool Enable { get; }

        public IEventAggregator EventAggregator { get; }

        public ILogger<FiniteStateMachines> Logger { get; }

        public IMachineSensorsStatus MachineSensorStatus { get; }

        public BayNumber RequestingBay { get; set; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        public BayNumber TargetBay { get; }

        #endregion
    }
}

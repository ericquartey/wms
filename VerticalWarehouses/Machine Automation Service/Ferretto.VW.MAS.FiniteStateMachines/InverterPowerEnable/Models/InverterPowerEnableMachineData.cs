using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.FiniteStateMachines.InverterPowerEnable.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.InverterPowerEnable.Models
{
    internal class InverterPowerEnableMachineData : IInverterPowerEnableMachineData
    {
        #region Constructors

        public InverterPowerEnableMachineData(
            bool enable,
            BayNumber requestingBay,
            BayNumber targetBay,
            IEnumerable<Inverter> bayInverters,
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.Enable = enable;
            this.RequestingBay = requestingBay;
            this.TargetBay = targetBay;
            this.BayInverters = bayInverters;
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Properties

        public IEnumerable<DataModels.Inverter> BayInverters { get; }

        public bool Enable { get; }

        public IEventAggregator EventAggregator { get; }

        public ILogger<FiniteStateMachines> Logger { get; }

        public BayNumber RequestingBay { get; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        public BayNumber TargetBay { get; }

        #endregion
    }
}

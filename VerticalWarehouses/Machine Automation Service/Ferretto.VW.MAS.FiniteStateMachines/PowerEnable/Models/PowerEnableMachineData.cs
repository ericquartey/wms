using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Models
{
    public class PowerEnableMachineData : IPowerEnableMachineData
    {


        #region Constructors

        public PowerEnableMachineData(
            bool enable,
            List<IoIndex> configuredIoDevices,
            List<InverterIndex> configuredInverters,
            BayIndex requestingBay,
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.EventAggregator = eventAggregator;
            this.ConfiguredIoDevices = configuredIoDevices;
            this.ConfiguredInverters = configuredInverters;
            this.Enable = enable;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
            this.RequestingBay = requestingBay;
        }

        #endregion



        #region Properties

        public List<InverterIndex> ConfiguredInverters { get; }

        public List<IoIndex> ConfiguredIoDevices { get; }

        public bool Enable { get; }

        public IEventAggregator EventAggregator { get; }

        public ILogger<FiniteStateMachines> Logger { get; }

        public BayIndex RequestingBay { get; set; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion
    }
}

using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.PowerEnable.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DeviceManager.PowerEnable.Models
{
    internal class PowerEnableMachineData : IPowerEnableMachineData
    {
        #region Constructors

        public PowerEnableMachineData(
            bool enable,
            BayNumber requestingBay,
            BayNumber targetBay,
            IMachineResourcesProvider machineResourcesProvider,
            IEventAggregator eventAggregator,
            ILogger<DeviceManager> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.EventAggregator = eventAggregator;
            this.Enable = enable;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
            this.RequestingBay = requestingBay;
            this.TargetBay = targetBay;
            this.MachineSensorStatus = machineResourcesProvider;
        }

        #endregion

        #region Properties

        public bool Enable { get; }

        public IEventAggregator EventAggregator { get; }

        public ILogger<DeviceManager> Logger { get; }

        public IMachineResourcesProvider MachineSensorStatus { get; }

        public BayNumber RequestingBay { get; set; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        public BayNumber TargetBay { get; }

        #endregion
    }
}

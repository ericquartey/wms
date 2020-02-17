using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.DeviceManager.ShutterPositioning.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;


namespace Ferretto.VW.MAS.DeviceManager.ShutterPositioning.Models
{
    internal class ShutterPositioningMachineData : IShutterPositioningMachineData
    {
        #region Constructors

        public ShutterPositioningMachineData(
            IShutterPositioningMessageData positioningMessageData,
            BayNumber requestingBay,
            BayNumber targetBay,
            InverterIndex inverterIndex,
            IMachineResourcesProvider machineResourcesProvider,
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.PositioningMessageData = positioningMessageData;
            this.RequestingBay = requestingBay;
            this.TargetBay = targetBay;
            this.InverterIndex = inverterIndex;
            this.MachineSensorsStatus = machineResourcesProvider;
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Properties

        public Timer DelayTimer { get; set; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IEventAggregator EventAggregator { get; }

        public InverterIndex InverterIndex { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public ILogger Logger { get; }

        public IMachineResourcesProvider MachineSensorsStatus { get; }

        public IShutterPositioningMessageData PositioningMessageData { get; }

        public BayNumber RequestingBay { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IServiceScopeFactory ServiceScopeFactory { get; }

        public BayNumber TargetBay { get; }

        #endregion
    }
}

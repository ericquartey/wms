using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.ProfileResolution.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.ProfileResolution.Models
{
    internal class ProfileResolutionMachineData : IProfileResolutionMachineData
    {
        #region Constructors

        public ProfileResolutionMachineData(
            MessageActor requester,
            BayNumber requestingBay,
            BayNumber targetBay,
            IPositioningMessageData messageData,
            IMachineResourcesProvider machineResourcesProvider,
            InverterIndex currentInverterIndex,
            IEventAggregator eventAggregator,
            ILogger logger,
            IBaysDataProvider baysDataProvider,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.Requester = requester;
            this.RequestingBay = requestingBay;
            this.TargetBay = targetBay;
            this.MessageData = messageData;
            this.MachineSensorStatus = machineResourcesProvider;
            this.CurrentInverterIndex = currentInverterIndex;
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
            this.BaysDataProvider = baysDataProvider;
            this.ServiceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Properties

        public IBaysDataProvider BaysDataProvider { get; }

        public InverterIndex CurrentInverterIndex { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IEventAggregator EventAggregator { get; }

        public int ExecutedSteps { get; set; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public ILogger Logger { get; }

        public IMachineResourcesProvider MachineSensorStatus { get; }

        public IPositioningMessageData MessageData { get; set; }

        public MessageActor Requester { get; }

        public BayNumber RequestingBay { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IServiceScopeFactory ServiceScopeFactory { get; }

        public BayNumber TargetBay { get; }

        #endregion
    }
}

using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Positioning.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DeviceManager.Positioning.Models
{
    internal class PositioningMachineData : IPositioningMachineData
    {
        #region Constructors

        public PositioningMachineData(
            MessageActor requester,
            BayNumber requestingBay,
            BayNumber targetBay,
            IPositioningMessageData messageData,
            IMachineResourcesProvider machineResourcesProvider,
            InverterIndex currentInverterIndex,
            IEventAggregator eventAggregator,
            ILogger logger,
            IBaysProvider baysProvider,
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
            this.BaysProvider = baysProvider;
            this.ServiceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Properties

        public IBaysProvider BaysProvider { get; }

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

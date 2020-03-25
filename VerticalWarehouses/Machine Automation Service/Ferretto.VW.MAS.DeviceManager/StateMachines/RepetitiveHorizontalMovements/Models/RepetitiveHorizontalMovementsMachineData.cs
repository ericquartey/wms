using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements.Models
{
    internal class RepetitiveHorizontalMovementsMachineData : IRepetitiveHorizontalMovementsMachineData
    {
        #region Constructors

        public RepetitiveHorizontalMovementsMachineData(
            MessageActor requester,
            BayNumber requestingBay,
            BayNumber targetBay,
            IRepetitiveHorizontalMovementsMessageData messageData,
            IMachineResourcesProvider machineResourcesProvider,
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
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
            this.BaysDataProvider = baysDataProvider;
            this.ServiceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Properties

        public bool AcquiredWeight { get; set; }

        public IBaysDataProvider BaysDataProvider { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IEventAggregator EventAggregator { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public ILogger Logger { get; }

        public IMachineResourcesProvider MachineSensorStatus { get; }

        public IRepetitiveHorizontalMovementsMessageData MessageData { get; set; }

        public int NPerformedCycles { get; set; }

        public MessageActor Requester { get; }

        public BayNumber RequestingBay { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IServiceScopeFactory ServiceScopeFactory { get; }

        public BayNumber TargetBay { get; }

        #endregion
    }
}

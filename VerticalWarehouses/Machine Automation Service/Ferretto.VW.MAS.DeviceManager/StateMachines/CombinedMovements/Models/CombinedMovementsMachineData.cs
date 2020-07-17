using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.CombinedMovements.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.CombinedMovements.Models
{
    internal class CombinedMovementsMachineData : ICombinedMovementsMachineData
    {
        #region Constructors

        public CombinedMovementsMachineData(
            MessageActor requester,
            BayNumber requestingBay,
            BayNumber targetBay,
            ICombinedMovementsMessageData messageData,
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
            this.OnHorizontalPositioningError = false;
            this.OnVerticalPositioningError = false;
            this.OnHorizontalPositioningStopped = false;
            this.OnVerticalPositioningStopped = false;
        }

        #endregion

        #region Properties

        public IBaysDataProvider BaysDataProvider { get; private set; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IEventAggregator EventAggregator { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public ILogger Logger { get; }

        public IMachineResourcesProvider MachineSensorStatus { get; private set; }

        public ICombinedMovementsMessageData MessageData { get; set; }

        public bool OnHorizontalPositioningError { get; set; }

        public bool OnHorizontalPositioningStopped { get; set; }

        public bool OnVerticalPositioningError { get; set; }

        public bool OnVerticalPositioningStopped { get; set; }

        public MessageActor Requester { get; }

        public BayNumber RequestingBay { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IServiceScopeFactory ServiceScopeFactory { get; }

        public BayNumber TargetBay { get; }

        #endregion
    }
}

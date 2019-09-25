using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.Positioning.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.Positioning.Models
{
    internal class CarouselPositioningMachineData : ICarouselPositioningMachineData
    {
        #region Constructors

        public CarouselPositioningMachineData(BayNumber requestingBay,
            BayNumber targetBay,
            IPositioningMessageData messageData,
            IMachineSensorsStatus machineSensorsStatus,
            InverterIndex currentInverterIndex,
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            IBaysProvider baysProvider,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.RequestingBay = requestingBay;
            this.TargetBay = targetBay;
            this.MessageData = messageData;
            this.MachineSensorStatus = machineSensorsStatus;
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

        public IEventAggregator EventAggregator { get; }

        public int ExecutedSteps { get; set; }

        public ILogger<FiniteStateMachines> Logger { get; }

        public IMachineSensorsStatus MachineSensorStatus { get; }

        public IPositioningMessageData MessageData { get; set; }

        public BayNumber RequestingBay { get; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        public BayNumber TargetBay { get; }

        #endregion
    }
}

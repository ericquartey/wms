using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning.Models
{
    public class ShutterPositioningMachineData : IShutterPositioningMachineData
    {


        #region Constructors

        public ShutterPositioningMachineData(
            IShutterPositioningMessageData positioningMessageData,
            BayNumber requestingBay,
            InverterIndex inverterIndex,
            IMachineSensorsStatus machineSensorsStatus,
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.PositioningMessageData = positioningMessageData;
            this.RequestingBay = requestingBay;
            this.InverterIndex = inverterIndex;
            this.MachineSensorsStatus = machineSensorsStatus;
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
        }

        #endregion



        #region Properties

        public Timer DelayTimer { get; set; }

        public IEventAggregator EventAggregator { get; }

        public InverterIndex InverterIndex { get; }

        public ILogger<FiniteStateMachines> Logger { get; }

        public IMachineSensorsStatus MachineSensorsStatus { get; }

        public IShutterPositioningMessageData PositioningMessageData { get; }

        public BayNumber RequestingBay { get; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        public BayNumber TargetBay { get; }

        #endregion
    }
}

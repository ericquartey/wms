using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Homing.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Homing.Models
{
    internal class HomingMachineData : IHomingMachineData
    {
        #region Constructors

        public HomingMachineData(
            bool isOneTonMachine,
            int? loadingUnitId,
            BayNumber requestingBay,
            BayNumber targetBay,
            IMachineResourcesProvider machineResourcesProvider,
            InverterIndex inverterIndex,
            bool showErrors,
            bool turnBack,
            bool bypassSensor,
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.IsOneTonMachine = isOneTonMachine;
            this.LoadingUnitId = loadingUnitId;
            this.RequestingBay = requestingBay;
            this.TargetBay = targetBay;
            this.MachineSensorStatus = machineResourcesProvider;
            this.CurrentInverterIndex = inverterIndex;
            this.ShowErrors = showErrors;
            this.TurnBack = turnBack;
            this.BypassSensor = bypassSensor;
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Properties

        public Axis AxisToCalibrate { get; set; }

        public bool BypassSensor { get; set; }

        public Calibration CalibrationType { get; set; }

        public InverterIndex CurrentInverterIndex { get; set; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IEventAggregator EventAggregator { get; }

        public double HorizontalStartingPosition { get; set; }

        public InverterIndex InverterIndexOld { get; set; }

        public bool IsOneTonMachine { get; }

        public int? LoadingUnitId { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public ILogger Logger { get; }

        public IMachineResourcesProvider MachineSensorStatus { get; }

        public int MaximumSteps { get; set; }

        public int NumberOfExecutedSteps { get; set; }

        public Axis RequestedAxisToCalibrate { get; set; }

        public BayNumber RequestingBay { get; set; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IServiceScopeFactory ServiceScopeFactory { get; }

        public bool ShowErrors { get; }

        public BayNumber TargetBay { get; }

        public bool TurnBack { get; }

        public double VerticalStartingPosition { get; set; }

        #endregion
    }
}

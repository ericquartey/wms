using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Homing.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DeviceManager.Homing.Models
{
    internal class HomingMachineData : IHomingMachineData
    {
        #region Constructors

        public HomingMachineData(
            bool isOneKMachine,
            BayNumber requestingBay,
            BayNumber targetBay,
            IMachineResourcesProvider machineResourcesProvider,
            InverterIndex inverterIndex,
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.IsOneKMachine = isOneKMachine;
            this.RequestingBay = requestingBay;
            this.TargetBay = targetBay;
            this.MachineSensorStatus = machineResourcesProvider;
            this.CurrentInverterIndex = inverterIndex;
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Properties

        public Axis AxisToCalibrate { get; set; }

        public Calibration CalibrationType { get; set; }

        public InverterIndex CurrentInverterIndex { get; set; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IEventAggregator EventAggregator { get; }

        public InverterIndex InverterIndexOld { get; set; }

        public bool IsOneKMachine { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public ILogger Logger { get; }

        public IMachineResourcesProvider MachineSensorStatus { get; }

        public int MaximumSteps { get; set; }

        public int NumberOfExecutedSteps { get; set; }

        public BayNumber RequestingBay { get; set; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IServiceScopeFactory ServiceScopeFactory { get; }

        public BayNumber TargetBay { get; }

        #endregion
    }
}

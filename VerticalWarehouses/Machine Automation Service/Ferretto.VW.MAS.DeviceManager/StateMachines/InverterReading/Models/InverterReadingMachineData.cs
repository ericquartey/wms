using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.InverterReading.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.InverterReading.Models
{
    internal class InverterReadingMachineData : IInverterReadingMachineData
    {
        #region Constructors

        public InverterReadingMachineData(
            IEnumerable<InverterParametersData> InverterParametersData,
            BayNumber requestingBay,
            BayNumber targetBay,
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
            this.InverterParametersData = InverterParametersData;
            this.RequestingBay = requestingBay;
            this.TargetBay = targetBay;
        }

        #endregion

        #region Properties

        public bool Enable { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IEventAggregator EventAggregator { get; }

        public IEnumerable<InverterParametersData> InverterParametersData { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public ILogger Logger { get; }

        public BayNumber RequestingBay { get; set; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IServiceScopeFactory ServiceScopeFactory { get; }

        public BayNumber TargetBay { get; }

        #endregion
    }
}

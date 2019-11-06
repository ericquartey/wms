using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.ResetFault.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.ResetFault.Models
{
    internal class ResetFaultMachineData : IResetFaultMachineData
    {
        #region Constructors

        public ResetFaultMachineData(
            BayNumber requestingBay,
            BayNumber targetBay,
            IEnumerable<Inverter> bayInverters,
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.RequestingBay = requestingBay;
            this.TargetBay = targetBay;
            this.BayInverters = bayInverters;
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Properties

        public IEnumerable<Inverter> BayInverters { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IEventAggregator EventAggregator { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public ILogger Logger { get; }

        public BayNumber RequestingBay { get; }

        [Obsolete("Replace this reference with DI or ServiceProvider.")]
        public IServiceScopeFactory ServiceScopeFactory { get; }

        public BayNumber TargetBay { get; }

        #endregion
    }
}

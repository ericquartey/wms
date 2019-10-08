using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.ResetFault.Models
{
    internal class ResetFaultMachineData : Interfaces.IResetFaultMachineData
    {
        #region Constructors

        public ResetFaultMachineData(
            BayNumber requestingBay,
            BayNumber targetBay,
            IEnumerable<DataModels.Inverter> bayInverters,
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
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

        public IEnumerable<DataModels.Inverter> BayInverters { get; }

        public IEventAggregator EventAggregator { get; }

        public ILogger<FiniteStateMachines> Logger { get; }

        public BayNumber RequestingBay { get; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        public BayNumber TargetBay { get; }

        #endregion
    }
}

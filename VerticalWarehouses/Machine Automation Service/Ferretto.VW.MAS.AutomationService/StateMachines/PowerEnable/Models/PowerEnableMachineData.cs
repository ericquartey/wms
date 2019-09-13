using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable.Models
{
    public class PowerEnableMachineData : IPowerEnableMachineData
    {


        #region Constructors

        public PowerEnableMachineData(bool requestedPowerState, BayNumber requestingBay, List<Bay> ConfiguredBays, IEventAggregator eventAggregator, ILogger<AutomationService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            this.RequestedPowerState = requestedPowerState;
            this.RequestingBay = requestingBay;
            this.ConfiguredBays = ConfiguredBays;
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
        }

        #endregion



        #region Properties

        public List<Bay> ConfiguredBays { get; }

        public IEventAggregator EventAggregator { get; }

        public ILogger<AutomationService> Logger { get; }

        public bool RequestedPowerState { get; }

        public BayNumber RequestingBay { get; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion
    }
}

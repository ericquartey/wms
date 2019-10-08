using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.ResetSecurity.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DeviceManager.ResetSecurity.Models
{
    internal class ResetSecurityMachineData : IResetSecurityMachineData
    {
        #region Constructors

        public ResetSecurityMachineData(
            BayNumber requestingBay,
            BayNumber targetBay,
            IEventAggregator eventAggregator,
            ILogger<DeviceManager> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.RequestingBay = requestingBay;
            this.TargetBay = targetBay;
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Properties

        public IEventAggregator EventAggregator { get; }

        public ILogger<DeviceManager> Logger { get; }

        public BayNumber RequestingBay { get; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        public BayNumber TargetBay { get; }

        #endregion
    }
}

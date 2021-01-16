using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.DataLayer;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed class LoadingUnitsAdapterProvider : ILoadingUnitsAdapterProvider
    {
        #region Fields

        private readonly IErrorsProvider errorsProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineLoadingUnitsAdapterWebService loadingUnitsAdapterWebService;

        private readonly ILogger<LoadingUnitsAdapterProvider> logger;

        #endregion

        #region Constructors

        public LoadingUnitsAdapterProvider(
            IEventAggregator eventAggregator,
            IErrorsProvider errorsProvider,
            ILogger<LoadingUnitsAdapterProvider> logger,
            IMachineLoadingUnitsAdapterWebService machineLoadingUnitsAdapterWebService)
        {
            this.eventAggregator = eventAggregator;
            this.errorsProvider = errorsProvider;
            this.logger = logger;
            this.loadingUnitsAdapterWebService = machineLoadingUnitsAdapterWebService;
        }

        #endregion

        #region Methods

        Task<LoadingUnitSchedulerRequest> ILoadingUnitsAdapterProvider.WithdrawAsync(int id, int bayId)
        {
            this.logger.LogDebug($"Calling adapter to move load unit {id} back from bay {bayId}.");
            return this.loadingUnitsAdapterWebService.WithdrawAsync(id, bayId);
        }

        #endregion
    }
}

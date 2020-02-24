using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ferretto.VW.MAS.AutomationService
{
    public class ReadinessHealthCheck : IHealthCheck
    {
        #region Fields

        private readonly IDataLayerService dataLayerService;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        #endregion

        #region Constructors

        public ReadinessHealthCheck(IDataLayerService dataLayerService, IMachineVolatileDataProvider machineVolatileDataProvider)
        {
            if (dataLayerService == null)
            {
                throw new System.ArgumentNullException(nameof(dataLayerService));
            }

            if (machineVolatileDataProvider == null)
            {
                throw new System.ArgumentNullException(nameof(machineVolatileDataProvider));
            }

            this.machineVolatileDataProvider = machineVolatileDataProvider;
            this.dataLayerService = dataLayerService;
        }

        #endregion

        #region Methods

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (this.dataLayerService.IsReady && this.machineVolatileDataProvider.IsAutomationServiceReady)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy(Resources.General.ServiceStartupSequenceCompleted));
            }

            return Task.FromResult(
                HealthCheckResult.Unhealthy(Resources.General.ServiceIsInitializing));
        }

        #endregion
    }
}

using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ferretto.VW.MAS.AutomationService
{
    public class LivelinessHealthCheck : IHealthCheck
    {
        #region Fields

        private readonly IDataLayerService dataLayerService;

        private readonly IDbContextRedundancyService<DataLayerContext> redundancyService;

        #endregion

        #region Constructors

        public LivelinessHealthCheck(IDbContextRedundancyService<DataLayerContext> redundancyService, IDataLayerService dataLayerService)
        {
            if (dataLayerService == null)
            {
                throw new System.ArgumentNullException(nameof(dataLayerService));
            }

            this.redundancyService = redundancyService ?? throw new System.ArgumentNullException(nameof(redundancyService));

            this.dataLayerService = dataLayerService;
        }

        #endregion

        #region Methods

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (this.redundancyService.IsActiveDbInhibited)
            {
                return Task.FromResult(
                  HealthCheckResult.Unhealthy("Unrecoverable database failure."));
            }

            if (this.redundancyService.IsStandbyDbInhibited)
            {
                return Task.FromResult(
                  HealthCheckResult.Degraded("Standby DB channel is inhibited."));
            }

            if (!this.dataLayerService.IsReady)
            {
                return Task.FromResult(
                    HealthCheckResult.Unhealthy("Datalayer not ready"));
            }

            return Task.FromResult(
                HealthCheckResult.Healthy());
        }

        #endregion
    }
}

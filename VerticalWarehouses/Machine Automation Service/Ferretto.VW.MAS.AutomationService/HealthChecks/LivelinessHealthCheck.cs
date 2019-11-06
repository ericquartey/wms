using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ferretto.VW.MAS.AutomationService
{
    public class LivelinessHealthCheck : IHealthCheck
    {
        #region Fields

        private readonly IDbContextRedundancyService<DataLayerContext> redundancyService;

        #endregion

        #region Constructors

        public LivelinessHealthCheck(IDbContextRedundancyService<DataLayerContext> redundancyService)
        {
            this.redundancyService = redundancyService ?? throw new System.ArgumentNullException(nameof(redundancyService));
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

            return Task.FromResult(
                HealthCheckResult.Healthy());
        }

        #endregion
    }
}

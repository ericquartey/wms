using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ferretto.VW.MAS.AutomationService.Filters
{
    internal class ReadinessFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly ReadinessHealthCheck readinessHealthCheck;

        #endregion

        #region Constructors

        public ReadinessFilter(IDataLayerService dataLayerService, IMachineVolatileDataProvider machineVolatileDataProvider)
        {
            if (dataLayerService == null)
            {
                throw new ArgumentNullException(nameof(dataLayerService));
            }

            this.readinessHealthCheck = new ReadinessHealthCheck(dataLayerService, machineVolatileDataProvider);
        }

        #endregion

        #region Methods

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var healthCheckResult = await this.readinessHealthCheck.CheckHealthAsync(null);
            if (healthCheckResult.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }
            else
            {
                await next();
            }
        }

        #endregion
    }
}

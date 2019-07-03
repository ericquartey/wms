using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Filters
{
    public class SendNotificationsFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly ILogger<SendNotificationsFilter> logger;

        private readonly INotificationService notificationService;

        #endregion

        #region Constructors

        public SendNotificationsFilter(
            INotificationService notificationService,
            ILogger<SendNotificationsFilter> logger)
        {
            this.notificationService = notificationService;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var resultContext = await next();
            if ((resultContext.Result is IStatusCodeActionResult result &&
                result.StatusCode != StatusCodes.Status400BadRequest &&
                result.StatusCode != StatusCodes.Status404NotFound &&
                result.StatusCode != StatusCodes.Status422UnprocessableEntity) ||
                resultContext.Result is FileResult)
            {
                await this.notificationService.SendNotificationsAsync();
            }
            else
            {
                if (resultContext.Result is ObjectResult objectResult
                    && objectResult.Value is ProblemDetails problemDetails)
                {
                    this.logger.LogInformation(problemDetails.Detail);
                }

                this.notificationService.Clear();
            }
        }

        #endregion
    }
}

using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Ferretto.WMS.Data.WebAPI.Filters
{
    public class SendNotificationsFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly INotificationService notificationService;

        #endregion

        #region Constructors

        public SendNotificationsFilter(INotificationService notificationService)
        {
            this.notificationService = notificationService;
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
                this.notificationService.Clear();
            }
        }

        #endregion
    }
}

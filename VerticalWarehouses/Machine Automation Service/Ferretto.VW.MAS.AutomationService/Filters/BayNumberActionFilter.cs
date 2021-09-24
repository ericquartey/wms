using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ferretto.VW.MAS.AutomationService.Filters
{
    public class BayNumberActionFilter : IAsyncActionFilter
    {
        #region Fields

        public static readonly string HeaderName = "Bay-Number";

        #endregion

        #region Methods

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Controller is IRequestingBayController baseController)
            {
                var bayNumberHeaders = context.HttpContext.Request.Headers[HeaderName];

                if (bayNumberHeaders.Count == 0)
                {
                    context.Result = new BadRequestObjectResult(new ProblemDetails
                    {
                        Title = Resources.General.ResourceManager.GetString("InternalServerErrorTitle", CommonUtils.Culture.Actual),
                        Detail = "The Bay-Number request header was not found.",
                    });
                }
                else
                {
                    if (Enum.TryParse<BayNumber>(bayNumberHeaders[0], out var bayNumber))
                    {
                        baseController.BayNumber = bayNumber;
                    }
                    else
                    {
                        context.Result = new BadRequestObjectResult(new ProblemDetails
                        {
                            Title = Resources.General.ResourceManager.GetString("InternalServerErrorTitle", CommonUtils.Culture.Actual),
                            Detail = "Cannot parse bay number.",
                        });
                    }
                }
            }

            if (context.Result is null && next != null)
            {
                await next().ConfigureAwait(true);
            }
        }

        #endregion
    }
}

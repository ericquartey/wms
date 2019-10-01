using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ferretto.VW.MAS.AutomationService.Filters
{
    public class BayNumberFilter : IAsyncActionFilter
    {
        #region Methods

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.Controller is BaseAutomationController baseController)
            {
                var bayNumberHeaders = context.HttpContext.Request.Headers["Bay-Number"];

                if (bayNumberHeaders.Count == 0)
                {
                    context.Result = new BadRequestObjectResult(new ProblemDetails
                    {
                        Title = MAS.Resources.General.BadRequestTitle,
                        Detail = "The Bay-Number request header was not found."
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
                            Title = MAS.Resources.General.BadRequestTitle,
                            Detail = "Cannot parse bay number."
                        });
                    }
                }
            }

            if (context.Result is null)
            {
                await next();
            }
        }

        #endregion
    }
}

using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ferretto.VW.MAS.AutomationService.Filters
{
    public class BayNumberFilter : IAsyncActionFilter
    {


        #region Methods

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var bayIndexHeaders = context.HttpContext.Request.Headers["Bay-Number"];

            if (bayIndexHeaders.Count == 0)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            else
            {
                if (context.Controller is BaseAutomationController baseController)
                {
                    try
                    {
                        baseController.BayIndex = Enum.Parse<BayNumber>(bayIndexHeaders[0]);
                        await next();
                    }
                    catch
                    {
                        context.Result = new StatusCodeResult(StatusCodes.Status400BadRequest);
                    }
                }
                else
                {
                    context.Result = new StatusCodeResult(StatusCodes.Status400BadRequest);
                }
            }
        }

        #endregion
    }
}

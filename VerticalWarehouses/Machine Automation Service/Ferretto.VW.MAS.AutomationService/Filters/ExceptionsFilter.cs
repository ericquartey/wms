using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Filters
{
    public class ExceptionsFilter : IActionFilter
    {
        #region Fields

        private readonly ILogger<ExceptionsFilter> logger;

        #endregion

        #region Constructors

        public ExceptionsFilter(ILogger<ExceptionsFilter> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Exception != null
                &&
                context.ExceptionHandled == false)
            {
                if (context.Exception is WMS.Data.WebAPI.Contracts.WmsWebApiException<WMS.Data.WebAPI.Contracts.ProblemDetails> ex)
                {
                    if (ex.Result != null)
                    {
                        context.Result = new ObjectResult(ex.Result)
                        {
                            StatusCode = ex.StatusCode,
                        };
                    }
                    else
                    {
                        context.Result = new ObjectResult(null)
                        {
                            StatusCode = ex.StatusCode,
                        };
                    }
                }
                else if (context.Exception is WMS.Data.WebAPI.Contracts.WmsWebApiException)
                {
                    context.Result = new BadRequestObjectResult(new ProblemDetails
                    {
                        Title = Resources.General.ResourceManager.GetString("BadRequestTitle", CommonUtils.Culture.Actual),
                        Detail = context.Exception.Message,
                    });
                }
                else if (context.Exception is DataLayer.EntityNotFoundException)
                {
                    context.Result = new NotFoundObjectResult(new ProblemDetails
                    {
                        Title = Resources.General.ResourceManager.GetString("NotFoundTitle", CommonUtils.Culture.Actual),
                        Detail = context.Exception.Message,
                    });
                }
                else if (context.Exception is ArgumentException)
                {
                    context.Result = new BadRequestObjectResult(new ProblemDetails
                    {
                        Title = Resources.General.ResourceManager.GetString("BadRequestTitle", CommonUtils.Culture.Actual),
                        Detail = context.Exception.Message,
                    });
                }
                else if (context.Exception is InvalidOperationException)
                {
                    context.Result = new UnprocessableEntityObjectResult(new ProblemDetails
                    {
                        Title = Resources.General.ResourceManager.GetString("UnprocessableEntityTitle", CommonUtils.Culture.Actual),
                        Detail = context.Exception.Message,
                    });
                }
                else
                {
                    context.Result = new ObjectResult(new ProblemDetails
                    {
                        Title = Resources.General.ResourceManager.GetString("InternalServerErrorTitle", CommonUtils.Culture.Actual),
                        Detail = context.Exception.Message,
                    })
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                    };
                }

                context.ExceptionHandled = true;

                this.logger.LogError(
                    context.Exception,
                    $"{context.ActionDescriptor.DisplayName} completed with an error (code={(context.Result as ObjectResult).StatusCode}).");
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // do nothing
        }

        #endregion
    }
}

﻿using System;
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
            if (context.Exception != null
                &&
                context.ExceptionHandled == false)
            {
                if (context.Exception is DataLayer.EntityNotFoundException)
                {
                    context.Result = new NotFoundObjectResult(new ProblemDetails
                    {
                        Title = Resources.General.NotFoundTitle,
                        Detail = context.Exception.Message
                    });
                }
                else if (context.Exception is ArgumentOutOfRangeException)
                {
                    context.Result = new BadRequestObjectResult(new ProblemDetails
                    {
                        Title = Resources.General.BadRequestTitle,
                        Detail = context.Exception.Message
                    });
                }
                else if (context.Exception is InvalidOperationException)
                {
                    context.Result = new UnprocessableEntityObjectResult(new ProblemDetails
                    {
                        Title = Resources.General.UnprocessableEntityTitle,
                        Detail = context.Exception.Message
                    });
                }
                else
                {
                    context.Result = new ObjectResult(new ProblemDetails
                    {
                        Title = Resources.General.InternalServerErrorTitle,
                        Detail = context.Exception.Message
                    })
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
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

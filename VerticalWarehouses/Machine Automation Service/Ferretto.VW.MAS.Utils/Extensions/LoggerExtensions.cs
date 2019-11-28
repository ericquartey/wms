using System;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.Utils
{
    public static class LoggerExtensions
    {
        #region Methods

        public static IApplicationBuilder UseMessageLogging(this IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            FieldCommandMessage.logger = app.ApplicationServices.GetRequiredService<ILogger<Message>>();
            // TODO: add here the loggers related to the other classes

            return app;
        }

        #endregion
    }
}

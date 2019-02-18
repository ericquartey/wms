using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        #region Fields

        private readonly ILogger logger;
        private readonly RequestDelegate next;

        #endregion

        #region Constructors

        public RequestResponseLoggingMiddleware(
            ILogger<RequestResponseLoggingMiddleware> logger,
            RequestDelegate next)
        {
            this.next = next;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                return;
            }

            this.logger.LogInformation(await FormatRequestAsync(context.Request));

            await this.next(context);
        }

        private static async Task<string> FormatRequestAsync(HttpRequest request)
        {
            var body = request.Body;
            request.EnableRewind();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            var bodyAsText = default(string);
            while (await request.Body.ReadAsync(buffer, 0, buffer.Length) > 0)
            {
                bodyAsText = Encoding.UTF8.GetString(buffer);
                request.Body = body;
            }

            return
                $"{request.Scheme} " +
                $"{request.Host}{request.Path} " +
                $"{request.QueryString} " +
                $"{bodyAsText?.Replace('\r', ' ').Replace('\n', ' ')}";
        }

        #endregion
    }
}

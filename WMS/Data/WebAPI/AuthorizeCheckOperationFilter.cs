using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ferretto.WMS.Data.WebAPI
{
    internal class AuthorizeCheckOperationFilter : IOperationFilter
    {
        #region Methods

        public void Apply(Operation operation, OperationFilterContext context)
        {
            // Check for authorize attribute
            context.ApiDescription.TryGetMethodInfo(out var methodInfo);

            var hasAuthorize = methodInfo.CustomAttributes
               .Any(attr => attr.AttributeType == typeof(AuthorizeAttribute));

            if (hasAuthorize)
            {
                operation.Responses.Add("401", new Response { Description = "Unauthorized" });
                operation.Responses.Add("403", new Response { Description = "Forbidden" });

                operation.Security = new List<IDictionary<string, IEnumerable<string>>>();
                operation.Security.Add(new Dictionary<string, IEnumerable<string>>
                {
                    { "oauth2", new[] { "wms-data" } }
                });
            }
        }

        #endregion
    }
}

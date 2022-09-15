using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ferretto.VW.MAS.AutomationService.Filters
{
    public class BayNumberOperationFilter : IOperationFilter
    {
        #region Methods

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            if (operation is null)
            {
                throw new System.ArgumentNullException(nameof(operation));
            }

            if (operation.Parameters is null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            if (context.MethodInfo.DeclaringType.GetInterface(nameof(IRequestingBayController)) != null)
            {
                operation.Parameters.Add(
                    new OpenApiParameter
                    {
                        Name = BayNumberActionFilter.HeaderName,
                        In = ParameterLocation.Header,
                        Required = true,
                    });
            }
        }

        #endregion
    }
}

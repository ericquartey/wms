using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ferretto.VW.MAS.AutomationService.Filters
{
    public class BayNumberOperationFilter : IOperationFilter
    {
        #region Methods

        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters is null)
            {
                operation.Parameters = new List<IParameter>();
            }

            if (context.MethodInfo.DeclaringType.GetInterface(nameof(IRequestingBayController)) != null)
            {
                operation.Parameters.Add(
                    new NonBodyParameter
                    {
                        Name = BayNumberActionFilter.HeaderName,
                        In = "header",
                        Type = "string",
                        Required = true,
                    });
            }
        }

        #endregion
    }
}

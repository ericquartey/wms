using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            operation.Parameters.Add(
                new NonBodyParameter
                {
                    Name = BayNumberActionFilter.HeaderName,
                    In = "header",
                    Type = "string",
                    Required = true,
                });
        }

        #endregion
    }
}

using System.Linq;
using Ferretto.WMS.Data.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace Ferretto.WMS.Data.WebAPI.Filters
{
    public class NormalizeTakeValueFilter : IActionFilter
    {
        #region Fields

        private readonly IConfiguration configuration;

        #endregion

        #region Constructors

        public NormalizeTakeValueFilter(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        #endregion

        #region Properties

        protected int MaxTake => this.configuration.GetMaxPageSize();

        #endregion

        #region Methods

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // nothing to do here
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                return;
            }

            const string takeParameterName = "take";
            var isTakeAllowed = context.ActionDescriptor.Parameters.Any(x => x.Name == takeParameterName);
            if (!isTakeAllowed)
            {
                return;
            }

            var isTakeSpecified = context.ActionArguments.TryGetValue(takeParameterName, out var takeObject);
            var isTakeInvalid = takeObject == null || (takeObject is int take && (take <= 0 || take > this.MaxTake));

            if (!isTakeSpecified || isTakeInvalid)
            {
                context.ActionArguments[takeParameterName] = this.MaxTake;
            }
        }

        #endregion
    }
}

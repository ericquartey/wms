using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace Ferretto.WMS.Data.WebAPI.Filters
{
    public class NormalizeTakeValueFilter : IActionFilter
    {
        #region Fields

        private const int defaultMaxTake = 1000;

        private readonly IConfiguration configuration;

        #endregion

        #region Constructors

        public NormalizeTakeValueFilter(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        #endregion

        #region Properties

        protected int MaxTake =>
            int.TryParse(this.configuration.GetValue<string>("MaxTake"), out var configValue)
                ? configValue
                : defaultMaxTake;

        #endregion

        #region Methods

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // do something after the action executes
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

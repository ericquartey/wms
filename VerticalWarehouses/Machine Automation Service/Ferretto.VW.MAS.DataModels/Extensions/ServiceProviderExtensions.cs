using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataModels.Extensions
{
    public static class ServiceProviderExtensions
    {
        #region Methods

        public static IConditionEvaluator GetConditionEvaluator(this IServiceProvider serviceProvider, ErrorConditionAttribute attribute)
        {
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (attribute is null)
            {
                throw new ArgumentNullException(nameof(attribute));
            }

            var conditionEvaluator = serviceProvider.GetService(attribute.ConditionEvaluatorType) as IConditionEvaluator;

            System.Diagnostics.Debug.Assert(
                conditionEvaluator != null,
                "The resolved type should always be of type ICondition");

            return conditionEvaluator;
        }

        #endregion
    }
}

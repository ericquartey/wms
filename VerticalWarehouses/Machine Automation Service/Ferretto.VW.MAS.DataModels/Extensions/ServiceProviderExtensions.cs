using System;

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

            var resolvedService = serviceProvider.GetService(attribute.ConditionEvaluatorType);

            System.Diagnostics.Debug.Assert(
               resolvedService != null,
               $"The type {attribute.ConditionEvaluatorType} was not registered in the container.");

            var conditionEvaluator = resolvedService as IConditionEvaluator;

            System.Diagnostics.Debug.Assert(
                resolvedService is IConditionEvaluator,
                $"The resolved type should always be of type {nameof(IConditionEvaluator)}");

            return conditionEvaluator;
        }

        #endregion
    }
}

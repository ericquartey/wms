using System;
using System.Linq;

namespace Ferretto.VW.MAS.DataModels
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class ErrorConditionAttribute : Attribute
    {
        #region Constructors

        public ErrorConditionAttribute(Type conditionEvaluatorType)
        {
            if (conditionEvaluatorType is null)
            {
                throw new ArgumentNullException(nameof(conditionEvaluatorType));
            }

            if (!conditionEvaluatorType.IsInterface)
            {
                throw new ArgumentException($"The specified type '{conditionEvaluatorType}' is not an interface.");
            }

            if (!conditionEvaluatorType.GetInterfaces().Any(i => i == typeof(IConditionEvaluator)))
            {
                throw new ArgumentException($"The specified type '{conditionEvaluatorType}' does not inherit from the interface '{nameof(IConditionEvaluator)}'.");
            }

            this.ConditionEvaluatorType = conditionEvaluatorType;
        }

        #endregion

        #region Properties

        public Type ConditionEvaluatorType { get; }

        #endregion
    }
}

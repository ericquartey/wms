using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Ferretto.Common.Utils.Expressions
{
    public static class ExpressionExtensions
    {
        #region Fields

        private static readonly System.Text.RegularExpressions.Regex BinaryExpressionRegex =
            new System.Text.RegularExpressions.Regex(
                @"(?<operator>[^(]+)\((?<left>[^,]+),(?<right>[^)]+)\)",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        private static readonly System.Text.RegularExpressions.Regex BinaryNestedExpressionRegex =
            new System.Text.RegularExpressions.Regex(
                @"^(?<operator>\w+)\((?<left>(\w+)\((.+)\)),(?<right>(\w+)\((.+)\))\)$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        #endregion

        #region Methods

        public static IExpression AsIExpression(this string stringExpression)
        {
            if (stringExpression == null)
            {
                return null;
            }

            var match = BinaryNestedExpressionRegex.Match(stringExpression);
            if (match.Success)
            {
                var operatorName = match.Groups["operator"].Value;
                return new BinaryExpression(operatorName)
                {
                    LeftExpression = AsIExpression(match.Groups["left"].Value),
                    RightExpression = AsIExpression(match.Groups["right"].Value)
                };
            }
            else
            {
                match = BinaryExpressionRegex.Match(stringExpression);
                if (match.Success)
                {
                    var operatorName = match.Groups["operator"].Value;
                    return new BinaryExpression(operatorName)
                    {
                        LeftExpression = AsIExpression(match.Groups["left"].Value),
                        RightExpression = AsIExpression(match.Groups["right"].Value)
                    };
                }
                else
                {
                    return new ValueExpression(stringExpression);
                }
            }
        }

        public static Expression<Func<TModel, bool>> BuildLambdaExpression<TModel>(this IExpression where)
        {
            if (where == null)
            {
                return null;
            }

            var lambdaInParameter = Expression.Parameter(typeof(TModel), typeof(TModel).Name.ToLower());
            var lambdaBody = where?.GetLambdaBody<TModel>(lambdaInParameter);

            return (Expression<Func<TModel, bool>>)Expression.Lambda(lambdaBody, lambdaInParameter);
        }

        public static bool ContainsOnlyTypeProperties<TDataModel>(
            this IExpression expression)
        {
            if (expression == null)
            {
                return true;
            }

            switch (expression)
            {
                case UnaryExpression v:
                    return ContainsOnlyTypeProperties<TDataModel>(v.Expression);

                case BinaryExpression v:
                    return ContainsOnlyTypeProperties<TDataModel>(v.LeftExpression) &&
                           ContainsOnlyTypeProperties<TDataModel>(v.RightExpression);

                case ValueExpression v:
                    var propertyName = v.Value;

                    return typeof(TDataModel).GetProperty(propertyName) != null;

                default:
                    return false;
            }
        }

        public static Expression GetLambdaBody<TInParameter>(
            this IExpression expression,
            ParameterExpression inParameter)
        {
            if (expression is BinaryExpression binaryExpression)
            {
                switch (binaryExpression.OperatorName)
                {
                    case string op when op.Equals(nameof(Expression.And), StringComparison.OrdinalIgnoreCase):
                        return GetLambdaAndExpression<TInParameter>(inParameter, binaryExpression);

                    case string op when op.Equals(nameof(Expression.Or), StringComparison.OrdinalIgnoreCase):
                        return GetLambdaOrExpression<TInParameter>(inParameter, binaryExpression);

                    case string op
                        when op.Equals(nameof(Expression.Equal), StringComparison.OrdinalIgnoreCase):
                        return GetLambdaEqualityExpression<TInParameter>(inParameter, binaryExpression);

                    default:
                        throw new NotSupportedException(
                            $"The specified operator '{binaryExpression.OperatorName}' is not supported");
                }
            }
            else if (expression is ValueExpression valueExpression)
            {
                var propertyInfo = typeof(TInParameter).GetProperty(
                    valueExpression.Value,
                    BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);

                if (propertyInfo == null)
                {
                    return Expression.Constant(valueExpression.Value);
                }

                return Expression.Property(inParameter, valueExpression.Value);
            }

            return null;
        }

        private static Expression GetLambdaAndExpression<TInParameter>(
            ParameterExpression inParameter,
            BinaryExpression binaryExpression)
        {
            return Expression.And(
                binaryExpression.LeftExpression.GetLambdaBody<TInParameter>(inParameter),
                binaryExpression.RightExpression.GetLambdaBody<TInParameter>(inParameter));
        }

        private static Expression GetLambdaEqualityExpression<TInParameter>(
            ParameterExpression inParameter,
            BinaryExpression binaryExpression)
        {
            var leftOperandType = binaryExpression.LeftExpression.GetOperandType<TInParameter>();

            Expression rightOperand = null;
            if (leftOperandType.IsEnum && binaryExpression.RightExpression is ValueExpression rightValueExpression)
            {
                var enumValue = System.Enum.Parse(leftOperandType, rightValueExpression.Value);
                rightOperand = Expression.Constant(enumValue);
            }
            else if (leftOperandType == typeof(int) &&
                     binaryExpression.RightExpression is ValueExpression rightIntegerValueExpression)
            {
                var intValue = int.Parse(rightIntegerValueExpression.Value);
                rightOperand = Expression.Constant(intValue);
            }
            else
            {
                rightOperand = binaryExpression.RightExpression.GetLambdaBody<TInParameter>(inParameter);
            }

            return Expression.Equal(
                binaryExpression.LeftExpression.GetLambdaBody<TInParameter>(inParameter),
                rightOperand);
        }

        private static Expression GetLambdaOrExpression<TInParameter>(
            ParameterExpression inParameter,
            BinaryExpression binaryExpression)
        {
            return Expression.Or(
                binaryExpression.LeftExpression.GetLambdaBody<TInParameter>(inParameter),
                binaryExpression.RightExpression.GetLambdaBody<TInParameter>(inParameter));
        }

        private static System.Type GetOperandType<TInParameter>(this IExpression expression)
        {
            if (expression is ValueExpression valueExpression)
            {
                var propertyInfo = typeof(TInParameter).GetProperty(
                    valueExpression.Value,
                    BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);

                if (propertyInfo != null)
                {
                    return propertyInfo.PropertyType;
                }
            }

            return null;
        }

        #endregion
    }
}

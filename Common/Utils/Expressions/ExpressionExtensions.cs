using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Ferretto.Common.Utils.Expressions
{
    public static class ExpressionExtensions
    {
        #region Fields

        private static readonly System.Text.RegularExpressions.Regex UnaryExpressionRegex =
            new System.Text.RegularExpressions.Regex(
                @"^(?<operator>\w+)\((?<operand>[^,]*)\)$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        private static readonly System.Text.RegularExpressions.Regex UnaryNestedExpressionRegex =
            new System.Text.RegularExpressions.Regex(
                @"^(?<operator>\w+)\((?<operand>(\w+)\((.+)\))\)$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        private static readonly System.Text.RegularExpressions.Regex BinaryExpressionRegex =
            new System.Text.RegularExpressions.Regex(
                @"^(?<operator>\w+)\((?<left>[^,]*),(?<right>[^)]*)\)$",
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

            const string operator_group_name = "operator";
            var match = BinaryNestedExpressionRegex.Match(stringExpression);
            if (match.Success)
            {
                return new BinaryExpression(match.Groups[operator_group_name].Value)
                {
                    LeftExpression = AsIExpression(match.Groups["left"].Value),
                    RightExpression = AsIExpression(match.Groups["right"].Value)
                };
            }

            match = BinaryExpressionRegex.Match(stringExpression);
            if (match.Success)
            {
                return new BinaryExpression(match.Groups[operator_group_name].Value)
                {
                    LeftExpression = AsIExpression(match.Groups["left"].Value),
                    RightExpression = AsIExpression(match.Groups["right"].Value)
                };
            }

            match = UnaryNestedExpressionRegex.Match(stringExpression);
            if (match.Success)
            {
                return new UnaryExpression(match.Groups[operator_group_name].Value)
                {
                    Expression = AsIExpression(match.Groups["operand"].Value),
                };
            }

            match = UnaryExpressionRegex.Match(stringExpression);
            if (match.Success)
            {
                return new UnaryExpression(match.Groups[operator_group_name].Value)
                {
                    Expression = AsIExpression(match.Groups["operand"].Value),
                };
            }

            return new ValueExpression(stringExpression);
        }

        public static Expression<Func<TModel, bool>> BuildLambdaExpression<TModel>(this IExpression where)
        {
            if (where == null)
            {
                return null;
            }

            var lambdaInParameter = Expression.Parameter(typeof(TModel), typeof(TModel).Name.ToLower());
            var lambdaBody = where.GetLambdaBody<TModel>(lambdaInParameter);

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

        public static string ToQueryString(this IEnumerable<SortOption> sortOptions)
        {
            if (sortOptions == null)
            {
                return string.Empty;
            }

            return string.Join(",", sortOptions.Select(s => $"{s.PropertyName} {s.Direction}"));
        }

        private static Expression GetLambdaAndExpression<TInParameter>(
            ParameterExpression inParameter,
            BinaryExpression binaryExpression)
        {
            return Expression.And(
                binaryExpression.LeftExpression.GetLambdaBody<TInParameter>(inParameter),
                binaryExpression.RightExpression.GetLambdaBody<TInParameter>(inParameter));
        }

        private static Expression GetLambdaBody<TInParameter>(
            this IExpression expression,
            ParameterExpression inParameter)
        {
            if (expression is BinaryExpression binaryExpression)
            {
                switch (binaryExpression.OperatorName)
                {
                    case string op
                        when op.Equals("And", StringComparison.OrdinalIgnoreCase):
                        return GetLambdaAndExpression<TInParameter>(inParameter, binaryExpression);

                    case string op
                        when op.Equals("Or", StringComparison.OrdinalIgnoreCase):
                        return GetLambdaOrExpression<TInParameter>(inParameter, binaryExpression);

                    case string op
                        when op.Equals("Equal", StringComparison.OrdinalIgnoreCase):
                        return GetLambdaEqualsExpression<TInParameter>(inParameter, binaryExpression);

                    case string op
                        when op.Equals("NotEqual", StringComparison.OrdinalIgnoreCase):
                        return GetLambdaNotEqualExpression<TInParameter>(inParameter, binaryExpression);

                    case string op
                        when op.Equals("Contains", StringComparison.OrdinalIgnoreCase):
                        return GetLambdaContainsExpression<TInParameter>(inParameter, binaryExpression);

                    case string op
                        when op.Equals("NotContains", StringComparison.OrdinalIgnoreCase):
                        return GetLambdaNotContainsExpression<TInParameter>(inParameter, binaryExpression);

                    case string op
                        when op.Equals("StartsWith", StringComparison.OrdinalIgnoreCase):
                        return GetLambdaStartsWithExpression<TInParameter>(inParameter, binaryExpression);

                    case string op
                        when op.Equals("EndsWith", StringComparison.OrdinalIgnoreCase):
                        return GetLambdaEndsWithExpression<TInParameter>(inParameter, binaryExpression);

                    default:
                        throw new NotSupportedException(
                            $"The specified binary operator '{binaryExpression.OperatorName}' is not supported");
                }
            }

            if (expression is UnaryExpression unaryExpression)
            {
                switch (unaryExpression.OperatorName)
                {
                    case string op
                        when op.Equals("Not", StringComparison.OrdinalIgnoreCase):
                        return GetLambdaNotExpression<TInParameter>(inParameter, unaryExpression);

                    default:
                        throw new NotSupportedException(
                            $"The specified unary operator '{unaryExpression.OperatorName}' is not supported");
                }
            }

            if (expression is ValueExpression valueExpression)
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

        private static Expression GetLambdaContainsExpression<TInParameter>(
            Expression inParameter,
            BinaryExpression binaryExpression)
        {
            if (!(binaryExpression.LeftExpression is ValueExpression propertyName))
            {
                throw new NotSupportedException("First operand of Contains operator must be a property name");
            }

            if (!(binaryExpression.RightExpression is ValueExpression containsToken))
            {
                throw new NotSupportedException("Second operand of Contains operator must be a property value");
            }

            if (typeof(TInParameter).GetProperty(propertyName.Value)?.PropertyType != typeof(string))
            {
                throw new NotSupportedException($"Property {propertyName} must be a string");
            }

            return Expression.GreaterThanOrEqual(
                Expression.Invoke(
                    (Expression<Func<string, int>>)(x => x.IndexOf(
                                                           containsToken.Value,
                                                           StringComparison.InvariantCultureIgnoreCase)),
                    Expression.Property(inParameter, propertyName.Value)),
                Expression.Constant(0));
        }

        private static Expression GetLambdaEndsWithExpression<TInParameter>(
            Expression inParameter,
            BinaryExpression binaryExpression)
        {
            if (!(binaryExpression.LeftExpression is ValueExpression propertyName))
            {
                throw new NotSupportedException("First operand of Contains operator must be a property name");
            }

            if (!(binaryExpression.RightExpression is ValueExpression containsToken))
            {
                throw new NotSupportedException("Second operand of Contains operator must be a property value");
            }

            if (typeof(TInParameter).GetProperty(propertyName.Value)?.PropertyType != typeof(string))
            {
                throw new NotSupportedException($"Property {propertyName} must be a string");
            }

            return Expression.Invoke(
                (Expression<Func<string, bool>>)(x => x.EndsWith(
                                                        containsToken.Value,
                                                        StringComparison.InvariantCultureIgnoreCase)),
                Expression.Property(inParameter, propertyName.Value));
        }

        private static Expression GetLambdaEqualsExpression<TInParameter>(
            ParameterExpression inParameter,
            BinaryExpression binaryExpression)
        {
            var leftOperator = GetOperandExpression<TInParameter>(
                inParameter,
                binaryExpression.LeftExpression,
                binaryExpression.RightExpression.GetOperandType<TInParameter>());
            var rightOperator = GetOperandExpression<TInParameter>(
                inParameter,
                binaryExpression.RightExpression,
                binaryExpression.LeftExpression.GetOperandType<TInParameter>());

            return Expression.Equal(leftOperator, rightOperator);
        }

        private static Expression GetLambdaNotContainsExpression<TInParameter>(
            Expression inParameter,
            BinaryExpression binaryExpression)
        {
            return Expression.Not(
                GetLambdaContainsExpression<TInParameter>(
                    inParameter,
                    binaryExpression));
        }

        private static Expression GetLambdaNotEqualExpression<TInParameter>(
            ParameterExpression inParameter,
            BinaryExpression binaryExpression)
        {
            return Expression.Not(
                GetLambdaEqualsExpression<TInParameter>(
                    inParameter,
                    binaryExpression));
        }

        private static Expression GetLambdaNotExpression<TInParameter>(
            ParameterExpression inParameter,
            UnaryExpression unaryExpression)
        {
            return Expression.Not(
                unaryExpression.Expression.GetLambdaBody<TInParameter>(inParameter));
        }

        private static Expression GetLambdaOrExpression<TInParameter>(
            ParameterExpression inParameter,
            BinaryExpression binaryExpression)
        {
            return Expression.Or(
                binaryExpression.LeftExpression.GetLambdaBody<TInParameter>(inParameter),
                binaryExpression.RightExpression.GetLambdaBody<TInParameter>(inParameter));
        }

        private static Expression GetLambdaStartsWithExpression<TInParameter>(
            Expression inParameter,
            BinaryExpression binaryExpression)
        {
            if (!(binaryExpression.LeftExpression is ValueExpression propertyName))
            {
                throw new NotSupportedException("First operand of Contains operator must be a property name");
            }

            if (!(binaryExpression.RightExpression is ValueExpression containsToken))
            {
                throw new NotSupportedException("Second operand of Contains operator must be a property value");
            }

            if (typeof(TInParameter).GetProperty(propertyName.Value)?.PropertyType != typeof(string))
            {
                throw new NotSupportedException($"Property {propertyName} must be a string");
            }

            return Expression.Invoke(
                (Expression<Func<string, bool>>)(x => x.StartsWith(
                                                        containsToken.Value,
                                                        StringComparison.InvariantCultureIgnoreCase)),
                Expression.Property(inParameter, propertyName.Value));
        }

        private static Expression GetOperandExpression<TInParameter>(
            ParameterExpression inParameter,
            IExpression binaryExpression,
            Type otherOperandType)
        {
            if (otherOperandType == null)
            {
                return binaryExpression.GetLambdaBody<TInParameter>(inParameter);
            }

            if (otherOperandType.IsEnum && binaryExpression is ValueExpression valueExpression)
            {
                var enumValue = Enum.Parse(otherOperandType, valueExpression.Value);
                return Expression.Constant(enumValue);
            }

            if (otherOperandType == typeof(int) &&
                binaryExpression is ValueExpression integerValueExpression)
            {
                var intValue = int.Parse(integerValueExpression.Value);
                return Expression.Constant(intValue);
            }

            return binaryExpression.GetLambdaBody<TInParameter>(inParameter);
        }

        private static Type GetOperandType<TInParameter>(this IExpression expression)
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

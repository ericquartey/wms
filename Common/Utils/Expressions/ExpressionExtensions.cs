using System.Linq.Expressions;

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

        #endregion Fields

        #region Methods

        public static IExpression AsIExpression(this string stringExpression)
        {
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

        public static Expression GetLambdaBody<T>(this IExpression expression, ParameterExpression inParameter)
        {
            if (expression is BinaryExpression binaryExpression)
            {
                switch (binaryExpression.OperatorName)
                {
                    case nameof(Expression.And):
                        return Expression.And(
                            binaryExpression.LeftExpression.GetLambdaBody<T>(inParameter),
                            binaryExpression.RightExpression.GetLambdaBody<T>(inParameter));

                    case nameof(Expression.Or):
                        return Expression.Or(
                            binaryExpression.LeftExpression.GetLambdaBody<T>(inParameter),
                            binaryExpression.RightExpression.GetLambdaBody<T>(inParameter));

                    case nameof(Expression.Equal):
                        {
                            var leftOperandType = binaryExpression.LeftExpression.GetOperandType<T>();

                            Expression rightOperand = null;
                            if (leftOperandType.IsEnum && binaryExpression.RightExpression is ValueExpression rightValueExpression)
                            {
                                var enumValue = System.Enum.Parse(leftOperandType, rightValueExpression.Value);
                                rightOperand = Expression.Constant(enumValue);
                            }
                            else
                            {
                                rightOperand = binaryExpression.RightExpression.GetLambdaBody<T>(inParameter);
                            }

                            return Expression.Equal(
                                binaryExpression.LeftExpression.GetLambdaBody<T>(inParameter),
                                rightOperand);
                        }
                }
            }
            else if (expression is ValueExpression valueExpression)
            {
                var propertyInfo = typeof(T).GetProperty(valueExpression.Value);

                if (propertyInfo == null)
                {
                    return Expression.Constant(valueExpression.Value);
                }

                return Expression.Property(inParameter, valueExpression.Value);
            }

            return null;
        }

        public static System.Type GetOperandType<T>(this IExpression expression)
        {
            if (expression is ValueExpression valueExpression)
            {
                var propertyInfo = typeof(T).GetProperty(valueExpression.Value);

                if (propertyInfo != null)
                {
                    return propertyInfo.PropertyType;
                }
            }

            return null;
        }

        #endregion Methods
    }
}

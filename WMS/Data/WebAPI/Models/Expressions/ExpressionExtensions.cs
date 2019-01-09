using System.Linq.Expressions;

namespace Ferretto.WMS.Data.WebAPI.Models.Expressions
{
    public static class ExpressionExtensions
    {
        #region Fields

        private static readonly System.Text.RegularExpressions.Regex binaryExpressionRegex =
               new System.Text.RegularExpressions.Regex(
           @"(?<operator>[^(]+)\((?<left>[^,]+),(?<right>[^)]+)\)",
           System.Text.RegularExpressions.RegexOptions.Compiled);

        #endregion Fields

        #region Methods

        public static IExpression BuildExpression(this string where)
        {
            var match = binaryExpressionRegex.Match(where);
            if (match.Success)
            {
                var operatorName = match.Groups["operator"].Value;
                return new BinaryExpression(operatorName)
                {
                    LeftExpression = BuildExpression(match.Groups["left"].Value),
                    RightExpression = BuildExpression(match.Groups["right"].Value)
                };
            }
            else
            {
                return new ValueExpression(where);
            }
        }

        public static Expression GetLambdaBody<T>(this IExpression expression, ParameterExpression inParameter)
        {
            if (expression is BinaryExpression binaryExpression)
            {
                if (binaryExpression.OperatorName == nameof(Expression.And))
                {
                    return Expression.And(
                        binaryExpression.LeftExpression.GetLambdaBody<T>(inParameter),
                        binaryExpression.RightExpression.GetLambdaBody<T>(inParameter));
                }
                else if (binaryExpression.OperatorName == nameof(Expression.Or))
                {
                    return Expression.Or(
                        binaryExpression.LeftExpression.GetLambdaBody<T>(inParameter),
                        binaryExpression.RightExpression.GetLambdaBody<T>(inParameter));
                }
                else if (binaryExpression.OperatorName == nameof(Expression.Equal))
                {
                    return Expression.Equal(
                        binaryExpression.LeftExpression.GetLambdaBody<T>(inParameter),
                        binaryExpression.RightExpression.GetLambdaBody<T>(inParameter));
                }
            }
            else if (expression is ValueExpression valueExpression)
            {
                var propertyInfo = typeof(T).GetProperty(valueExpression.Value);

                if (propertyInfo == null)
                {
                    return Expression.Constant(valueExpression.Value);
                }
                else
                {
                    return Expression.Property(inParameter, valueExpression.Value);
                }
            }

            return null;
        }

        #endregion Methods
    }
}

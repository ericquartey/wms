using System.Linq;
using DevExpress.Data.Filtering;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.Common.Controls
{
    public static class DevExpressExpressionExtensions
    {
        #region Methods

        public static IExpression BuildExpression(this CriteriaOperator filter)
        {
            if (filter is ConstantValue constantValue)
            {
                return new ValueExpression(constantValue.Value.ToString());
            }
            else if (filter is OperandProperty operandProperty)
            {
                return new ValueExpression(operandProperty.PropertyName);
            }
            else if (filter is BinaryOperator binaryOperator)
            {
                return new BinaryExpression(binaryOperator.OperatorType.ToString())
                {
                    LeftExpression = binaryOperator.LeftOperand.BuildExpression(),
                    RightExpression = binaryOperator.RightOperand.BuildExpression()
                };
            }
            else if (filter is GroupOperator groupOperator)
            {
                if (groupOperator.Operands.Count == 1)
                {
                    return groupOperator.Operands.Single().BuildExpression();
                }
                else
                {
                    return new BinaryExpression(groupOperator.OperatorType.ToString())
                    {
                        LeftExpression = groupOperator.Operands.First().BuildExpression(),
                        RightExpression = new GroupOperator(groupOperator.OperatorType, groupOperator.Operands.Skip(1)).BuildExpression()
                    };
                }
            }

            return null;
        }

        #endregion Methods
    }
}

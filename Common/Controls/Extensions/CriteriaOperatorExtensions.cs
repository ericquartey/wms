using System.Linq;
using DevExpress.Data.Filtering;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.Common.Controls.Extensions
{
    public static class CriteriaOperatorExtensions
    {
        #region Methods

        public static IExpression AsIExpression(this CriteriaOperator filter)
        {
            if (filter is ConstantValue constantValue)
            {
                return new ValueExpression(constantValue.Value.ToString());
            }

            if (filter is OperandValue operandValue)
            {
                return new ValueExpression(operandValue.Value?.ToString());
            }

            if (filter is OperandProperty operandProperty)
            {
                return new ValueExpression(operandProperty.PropertyName);
            }

            if (filter is BinaryOperator binaryOperator)
            {
                return new BinaryExpression(binaryOperator.OperatorType.ToString())
                {
                    LeftExpression = binaryOperator.LeftOperand.AsIExpression(),
                    RightExpression = binaryOperator.RightOperand.AsIExpression()
                };
            }

            if (filter is FunctionOperator functionOperator)
            {
                return new BinaryExpression(functionOperator.OperatorType.ToString())
                {
                    LeftExpression = functionOperator.Operands[0].AsIExpression(),
                    RightExpression = functionOperator.Operands[1].AsIExpression()
                };
            }

            if (filter is UnaryOperator unaryOperator)
            {
                return new UnaryExpression(unaryOperator.OperatorType.ToString())
                {
                    Expression = unaryOperator.Operand.AsIExpression(),
                };
            }

            if (filter is GroupOperator groupOperator)
            {
                if (groupOperator.Operands.Count == 1)
                {
                    return groupOperator.Operands.Single().AsIExpression();
                }

                return new BinaryExpression(groupOperator.OperatorType.ToString())
                {
                    LeftExpression = groupOperator.Operands.First().AsIExpression(),
                    RightExpression = new GroupOperator(groupOperator.OperatorType, groupOperator.Operands.Skip(1))
                        .AsIExpression()
                };
            }

            return null;
        }

        #endregion
    }
}

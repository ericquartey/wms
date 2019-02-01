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
            else if (filter is OperandValue operandValue)
            {
                return new ValueExpression(operandValue.Value.ToString());
            }
            else if (filter is OperandProperty operandProperty)
            {
                return new ValueExpression(operandProperty.PropertyName);
            }
            else if (filter is BinaryOperator binaryOperator)
            {
                return new BinaryExpression(binaryOperator.OperatorType.ToString())
                {
                    LeftExpression = binaryOperator.LeftOperand.AsIExpression(),
                    RightExpression = binaryOperator.RightOperand.AsIExpression()
                };
            }
            else if (filter is GroupOperator groupOperator)
            {
                if (groupOperator.Operands.Count == 1)
                {
                    return groupOperator.Operands.Single().AsIExpression();
                }
                else
                {
                    return new BinaryExpression(groupOperator.OperatorType.ToString())
                    {
                        LeftExpression = groupOperator.Operands.First().AsIExpression(),
                        RightExpression = new GroupOperator(groupOperator.OperatorType, groupOperator.Operands.Skip(1))
                            .AsIExpression()
                    };
                }
            }
            else
            {
                return null;
            }
        }

        #endregion Methods
    }
}

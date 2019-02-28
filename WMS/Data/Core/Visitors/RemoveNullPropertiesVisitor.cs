using DevExpress.Data.Filtering;
using DevExpress.Data.Filtering.Helpers;

namespace Ferretto.WMS.Data.Core.Visitors
{
    public class RemoveNullPropertiesVisitor : ClientCriteriaLazyPatcherBase.AggregatesCommonProcessingBase
    {
        #region Methods

        public static CriteriaOperator Patch(CriteriaOperator source)
        {
            return new RemoveNullPropertiesVisitor().Process(source);
        }

        public override CriteriaOperator Visit(AggregateOperand theOperand)
        {
            if (ReferenceEquals(theOperand, null))
            {
                return null;
            }

            var collectionProperty = this.Visit(theOperand.CollectionProperty);
            if (IsNull(collectionProperty))
            {
                return null;
            }

            var patched = base.Visit(theOperand);
            return ReferenceEquals(theOperand, patched) ? theOperand : null;
        }

        public override CriteriaOperator Visit(BetweenOperator theOperator)
        {
            theOperator = (BetweenOperator)base.Visit(theOperator);
            if (ReferenceEquals(theOperator, null))
            {
                return null;
            }

            if (IsNull(theOperator.BeginExpression) || IsNull(theOperator.EndExpression)
                                                    || IsNull(theOperator.TestExpression))
            {
                return null;
            }

            return theOperator;
        }

        public override CriteriaOperator Visit(BinaryOperator theOperator)
        {
            theOperator = (BinaryOperator)base.Visit(theOperator);
            if (ReferenceEquals(theOperator, null))
            {
                return null;
            }

            if (IsNull(theOperator.LeftOperand) || IsNull(theOperator.RightOperand))
            {
                return null;
            }

            return theOperator;
        }

        public override CriteriaOperator Visit(FunctionOperator theOperator)
        {
            var result = (FunctionOperator)base.Visit(theOperator);
            return !ReferenceEquals(theOperator, result) ? null : result;
        }

        public override CriteriaOperator Visit(InOperator theOperator)
        {
            if (ReferenceEquals(theOperator, null))
            {
                return null;
            }

            var result = (InOperator)base.Visit(theOperator);
            if (IsNull(result.LeftOperand))
            {
                return null;
            }

            if (ReferenceEquals(theOperator.Operands, result.Operands))
            {
                return theOperator;
            }

            var filteredOperands = RemoveEmptyOperands(result.Operands);
            if (filteredOperands.Count == 0)
            {
                return null;
            }

            if (filteredOperands.Count == 1)
            {
                return new BinaryOperator(theOperator.LeftOperand, filteredOperands[0], BinaryOperatorType.Equal);
            }

            return new InOperator(theOperator.LeftOperand, filteredOperands);
        }

        public override CriteriaOperator Visit(UnaryOperator theOperator)
        {
            theOperator = (UnaryOperator)base.Visit(theOperator);
            if (ReferenceEquals(theOperator, null))
            {
                return null;
            }

            return IsNull(theOperator.Operand) ? null : theOperator;
        }

        public override CriteriaOperator Visit(OperandValue theOperand)
        {
            return theOperand?.Value != null ? theOperand : null;
        }

        private static bool IsNull(CriteriaOperator theOperator)
        {
            return ReferenceEquals(theOperator, null);
        }

        private static CriteriaOperatorCollection RemoveEmptyOperands(CriteriaOperatorCollection source)
        {
            var result = new CriteriaOperatorCollection();
            foreach (var operand in source)
            {
                if (!IsNull(operand))
                {
                    result.Add(operand);
                }
            }

            return result;
        }

        #endregion
    }
}

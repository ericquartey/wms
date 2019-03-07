using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.Data.Filtering.Helpers;

namespace Ferretto.WMS.Data.Core.Visitors
{
    public class InsertNullPropagationVisitor : ClientCriteriaLazyPatcherBase.AggregatesCommonProcessingBase
    {
        #region Methods

        public static CriteriaOperator Patch(CriteriaOperator source)
        {
            return new InsertNullPropagationVisitor().Process(source);
        }

        public override CriteriaOperator Visit(FunctionOperator theOperator)
        {
            if (ReferenceEquals(theOperator, null))
            {
                return null;
            }

            var visitedOperator = (FunctionOperator)base.Visit(theOperator);
            var parameter = theOperator.Operands.FirstOrDefault();
            return AndNotNullCriteriaOperator(parameter, visitedOperator);
        }

        public override CriteriaOperator Visit(BinaryOperator theOperator)
        {
            var visitedOperator = (BinaryOperator)base.Visit(theOperator);

            CriteriaOperator modifiedCriteriaOperator = visitedOperator;
            if (visitedOperator.LeftOperand is FunctionOperator leftOperandFunction)
            {
                var parameter = leftOperandFunction.Operands.FirstOrDefault();
                modifiedCriteriaOperator = AndNotNullCriteriaOperator(parameter, modifiedCriteriaOperator);
            }

            if (visitedOperator.RightOperand is FunctionOperator rightOperandFunction)
            {
                var parameter = rightOperandFunction.Operands.FirstOrDefault();
                modifiedCriteriaOperator = AndNotNullCriteriaOperator(parameter, modifiedCriteriaOperator);
            }

            return modifiedCriteriaOperator;
        }

        private static CriteriaOperator AndNotNullCriteriaOperator(
            CriteriaOperator parameter,
            CriteriaOperator theOperator)
        {
            return CriteriaOperator.And(new NotOperator(new NullOperator(parameter)), theOperator);
        }

        #endregion
    }
}

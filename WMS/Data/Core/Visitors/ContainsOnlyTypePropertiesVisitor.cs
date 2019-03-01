using DevExpress.Data.Filtering;
using DevExpress.Data.Filtering.Helpers;

namespace Ferretto.WMS.Data.Core.Visitors
{
    public class ContainsOnlyTypePropertiesVisitor<TDataModel> : ClientCriteriaVisitorBase
    {
        #region Fields

        private bool containsOnlyTypeProperties = true;

        #endregion

        #region Methods

        public bool ProcessCriteria(CriteriaOperator theOperator)
        {
            this.Process(theOperator);
            return this.containsOnlyTypeProperties;
        }

        protected override CriteriaOperator Visit(OperandProperty theOperand)
        {
            if (ReferenceEquals(theOperand, null))
            {
                return null;
            }

            if (typeof(TDataModel).GetProperty(theOperand.PropertyName) == null)
            {
                this.containsOnlyTypeProperties = false;
            }

            return base.Visit(theOperand);
        }

        #endregion
    }
}

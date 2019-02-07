using DevExpress.Data.Filtering;

namespace Ferretto.Common.Controls
{
    public interface ICustomFilterViewModel
    {
        #region Properties

        CriteriaOperator CustomFilter { get; set; }

        bool IsFilterEditorVisible { get; }

        #endregion
    }
}

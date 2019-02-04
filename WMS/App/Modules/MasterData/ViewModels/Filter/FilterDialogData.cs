using DevExpress.Data.Filtering;
using DevExpress.Xpf.Core.FilteringUI;

namespace Ferretto.WMS.Modules.MasterData
{
    public class FilterDialogData
    {
        #region Properties

        public CriteriaOperator Filter { get; set; }

        public FilteringUIContext FilteringContext { get; set; }

        #endregion
    }
}

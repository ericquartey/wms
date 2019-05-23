using System.Collections.Generic;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemListOperation : BaseModel<int>, IPolicyItemList
    {
        #region Properties

        public ItemListStatus Status =>
            ItemList.GetStatus(
                this.TotalRowsCount,
                this.CompletedRowsCount,
                this.NewRowsCount,
                this.ExecutingRowsCount,
                this.WaitingRowsCount,
                this.IncompleteRowsCount,
                this.SuspendedRowsCount,
                this.ErrorRowsCount);

        public string Code { get; set; }

        public int CompletedRowsCount { get; set; }

        public int ErrorRowsCount { get; set; }

        public int ExecutingRowsCount { get; set; }

        public int IncompleteRowsCount { get; set; }

        public int NewRowsCount { get; set; }

        public IEnumerable<ItemListRowOperation> Rows { get; set; }

        public int SuspendedRowsCount { get; set; }

        public int TotalRowsCount { get; set; }

        public int WaitingRowsCount { get; set; }

        #endregion
    }
}

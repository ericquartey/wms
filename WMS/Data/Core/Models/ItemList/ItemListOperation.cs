using System.Collections.Generic;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(ItemList))]
    public class ItemListOperation : BaseModel<int>, IPolicyItemList
    {
        #region Properties

        public string Code { get; set; }

        [PositiveOrZero]
        public int CompletedRowsCount { get; set; }

        [PositiveOrZero]
        public int ErrorRowsCount { get; set; }

        [PositiveOrZero]
        public int ExecutingRowsCount { get; set; }

        [PositiveOrZero]
        public int IncompleteRowsCount { get; set; }

        [PositiveOrZero]
        public int NewRowsCount { get; set; }

        public ItemListType OperationType { get; set; }

        [PositiveOrZero]
        public int ReadyRowsCount { get; set; }

        public IEnumerable<ItemListRowOperation> Rows { get; set; }

        public ItemListStatus Status =>
           ItemList.GetStatus(
                this.TotalRowsCount,
                this.CompletedRowsCount,
                this.NewRowsCount,
                this.ExecutingRowsCount,
                this.WaitingRowsCount,
                this.IncompleteRowsCount,
                this.SuspendedRowsCount,
                this.ErrorRowsCount,
                this.ReadyRowsCount);

        [PositiveOrZero]
        public int SuspendedRowsCount { get; set; }

        [PositiveOrZero]
        public int TotalRowsCount { get; set; }

        [PositiveOrZero]
        public int WaitingRowsCount { get; set; }

        #endregion
    }
}

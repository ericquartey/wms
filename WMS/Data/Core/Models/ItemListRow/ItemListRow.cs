using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemListRow : BaseModel<int>
    {
        #region Properties

        public bool CanBeExecuted
        {
            get => this.ItemListRowStatus == ItemListRowStatus.Incomplete
                   || this.ItemListRowStatus == ItemListRowStatus.Suspended
                   || this.ItemListRowStatus == ItemListRowStatus.Waiting;
        }

        public string Code { get; set; }

        public DateTime CreationDate { get; set; }

        public int DispatchedQuantity { get; set; }

        public string ItemDescription { get; set; }

        public int ItemListId { get; set; }

        public ItemListRowStatus ItemListRowStatus { get; set; }

        public string ItemUnitMeasure { get; set; }

        public string MaterialStatusDescription { get; set; }

        public int RequiredQuantity { get; set; }

        public int RowPriority { get; set; }

        #endregion
    }
}

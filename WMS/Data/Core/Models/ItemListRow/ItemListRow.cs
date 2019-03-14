using System;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemListRow : BaseModel<int>,
        ICanDelete, ICanBeExecuted
    {
        #region Properties

        public bool CanBeExecuted
        {
            get => this.ItemListRowStatus == ItemListRowStatus.Incomplete
                   || this.ItemListRowStatus == ItemListRowStatus.Suspended
                   || this.ItemListRowStatus == ItemListRowStatus.Waiting;
        }

        public bool CanDelete
        {
            get => this.ItemListRowStatus == ItemListRowStatus.Waiting
                && !this.HasSchedulerRequestAssociated;
        }

        public string Code { get; set; }

        public DateTime CreationDate { get; set; }

        public int DispatchedQuantity { get; set; }

        public bool HasSchedulerRequestAssociated { get; set; }

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

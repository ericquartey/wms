using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemListRowDetails : BaseModel<int>
    {
        #region Properties

        public bool CanBeExecuted => this.ItemListRowStatus == ItemListRowStatus.Incomplete ||
                   this.ItemListRowStatus == ItemListRowStatus.Suspended ||
                   this.ItemListRowStatus == ItemListRowStatus.Waiting;

        public bool CanDelete
        {
            get => this.ItemListRowStatus == ItemListRowStatus.Waiting
                && !this.HasSchedulerRequestAssociated;
        }

        public string Code { get; set; }

        public DateTime? CompletionDate { get; set; }

        public DateTime CreationDate { get; set; }

        public int DispatchedQuantity { get; set; }

        public bool HasSchedulerRequestAssociated { get; set; }

        public string ItemDescription { get; set; }

        public int ItemId { get; set; }

        public string ItemListCode { get; set; }

        public string ItemListDescription { get; set; }

        public int ItemListId { get; set; }

        public ItemListRowStatus ItemListRowStatus { get; set; }

        public ItemListType ItemListType { get; set; }

        public string ItemUnitMeasure { get; set; }

        public DateTime? LastExecutionDate { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? PackageTypeId { get; set; }

        public string RegistrationNumber { get; set; }

        public int RequiredQuantity { get; set; }

        public int RowPriority { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion
    }
}

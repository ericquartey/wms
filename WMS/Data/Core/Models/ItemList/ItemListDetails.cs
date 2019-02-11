using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemListDetails : BaseModel<int>
    {
        #region Fields

        private int itemListItemsCount;

        private int priority;

        #endregion

        #region Properties

        public bool CanAddNewRow
        {
            get => this.ItemListStatus != ItemListStatus.Completed;
        }

        public bool CanBeExecuted
        {
            get => this.ItemListStatus == ItemListStatus.Incomplete
                   || this.ItemListStatus == ItemListStatus.Suspended
                   || this.ItemListStatus == ItemListStatus.Waiting;
        }

        public int ItemListRowsCount { get; set; }

        public string AreaName { get; set; }

        public string Code { get; set; }

        public DateTime CreationDate { get; set; }

        public string CustomerOrderCode { get; set; }

        public string CustomerOrderDescription { get; set; }

        public string Description { get; set; }

        public DateTime? ExecutionEndDate { get; set; }

        public DateTime? FirstExecutionDate { get; set; }

        public int ItemListItemsCount
        {
            get => this.itemListItemsCount;
            set => this.itemListItemsCount = CheckIfPositive(value);
        }

        public ItemListStatus ItemListStatus { get; set; }

        public ItemListType ItemListType { get; set; }

        public string ItemListTypeDescription { get; set; }

        public string Job { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public int Priority
        {
            get => this.priority;
            set => this.priority = CheckIfStrictlyPositive(value);
        }

        public bool ShipmentUnitAssociated { get; set; }

        public string ShipmentUnitCode { get; set; }

        public string ShipmentUnitDescription { get; set; }

        #endregion
    }
}

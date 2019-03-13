using System;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemDetails : BaseModel<int>,
        ICanDelete
    {
        #region Fields

        private int? averageWeight;

        private int? fifoTimePick;

        private int? fifoTimeStore;

        private int? height;

        private int? inventoryTolerance;

        private int? length;

        private int? pickTolerance;

        private int? reorderPoint;

        private int? reorderQuantity;

        private int? storeTolerance;

        private int totalAvailable;

        private int? width;

        #endregion

        #region Properties

        public string AbcClassId { get; set; }

        public int? AverageWeight
        {
            get => this.averageWeight;
            set => this.averageWeight = CheckIfStrictlyPositive(value);
        }

        public bool CanDelete
        {
            get => !this.HasCompartmentAssociated && !this.HasItemListRowAssociated
                && !this.HasMissionAssociated && !this.HasSchedulerRequestAssociated;
        }

        public string Code { get; set; }

        public int CompartmentsCount { get; set; }

        public DateTime CreationDate { get; set; }

        public string Description { get; set; }

        public int? FifoTimePick
        {
            get => this.fifoTimePick;
            set => this.fifoTimePick = CheckIfStrictlyPositive(value);
        }

        public int? FifoTimeStore
        {
            get => this.fifoTimeStore;
            set => this.fifoTimeStore = CheckIfStrictlyPositive(value);
        }

        public bool HasAreaAssociated { get; set; }

        public bool HasCompartmentAssociated { get; set; }

        public bool HasCompartmentTypeAssociated { get; set; }

        public bool HasItemListRowAssociated { get; set; }

        public bool HasMissionAssociated { get; set; }

        public bool HasSchedulerRequestAssociated { get; set; }

        public int? Height
        {
            get => this.height;
            set => this.height = CheckIfStrictlyPositive(value);
        }

        public string Image { get; set; }

        public string ImagePath { get; set; }

        public DateTime? InventoryDate { get; set; }

        public int? InventoryTolerance
        {
            get => this.inventoryTolerance;
            set => this.inventoryTolerance = CheckIfStrictlyPositive(value);
        }

        public int? ItemCategoryId { get; set; }

        public int ItemListRowsCount { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastStoreDate { get; set; }

        public int? Length
        {
            get => this.length;
            set => this.length = CheckIfStrictlyPositive(value);
        }

        public ItemManagementType ManagementType { get; set; }

        public string MeasureUnitDescription { get; set; }

        public string MeasureUnitId { get; set; }

        public int MissionsCount { get; set; }

        public string Note { get; set; }

        public int? PickTolerance
        {
            get => this.pickTolerance;
            set => this.pickTolerance = CheckIfStrictlyPositive(value);
        }

        public int? ReorderPoint
        {
            get => this.reorderPoint;
            set => this.reorderPoint = CheckIfStrictlyPositive(value);
        }

        public int? ReorderQuantity
        {
            get => this.reorderQuantity;
            set => this.reorderQuantity = CheckIfStrictlyPositive(value);
        }

        public int SchedulerRequestsCount { get; set; }

        public int? StoreTolerance
        {
            get => this.storeTolerance;
            set => this.storeTolerance = CheckIfStrictlyPositive(value);
        }

        public int TotalAvailable
        {
            get => this.totalAvailable;
            set => this.totalAvailable = CheckIfPositive(value);
        }

        public int? Width
        {
            get => this.width;
            set => this.width = CheckIfStrictlyPositive(value);
        }

        #endregion
    }
}

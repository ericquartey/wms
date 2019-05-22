using System;
using System.Collections.Generic;

namespace Ferretto.WMS.Data.Core.Models
{
    public class Item : BaseModel<int>, IItemPickPolicy, IItemDeletePolicy, IItemPutPolicy, IItemUpdatePolicy
    {
        #region Fields

        private int? averageWeight;

        private int? fifoTimePick;

        private int? fifoTimePut;

        private double? height;

        private int? inventoryTolerance;

        private double? length;

        private int? pickTolerance;

        private int? putTolerance;

        private int? reorderPoint;

        private int? reorderQuantity;

        private double totalAvailable;

        private double totalReservedForPick;

        private double totalReservedToPut;

        private double totalStock;

        private double? width;

        #endregion

        #region Properties

        public string AbcClassDescription { get; set; }

        public string AbcClassId { get; set; }

        public int? AverageWeight
        {
            get => this.averageWeight;
            set => this.averageWeight = CheckIfStrictlyPositive(value);
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

        public int? FifoTimePut
        {
            get => this.fifoTimePut;
            set => this.fifoTimePut = CheckIfStrictlyPositive(value);
        }

        public bool HasCompartmentTypes { get; set; }

        public double? Height
        {
            get => this.height;
            set => this.height = CheckIfStrictlyPositive(value);
        }

        public string Image { get; set; }

        public DateTime? InventoryDate { get; set; }

        public int? InventoryTolerance
        {
            get => this.inventoryTolerance;
            set => this.inventoryTolerance = CheckIfStrictlyPositive(value);
        }

        public string ItemCategoryDescription { get; set; }

        public int? ItemCategoryId { get; set; }

        public int ItemListRowsCount { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastPutDate { get; set; }

        public double? Length
        {
            get => this.length;
            set => this.length = CheckIfStrictlyPositive(value);
        }

        public IEnumerable<MachinePick> Machines { get; set; }

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

        public int? PutTolerance
        {
            get => this.putTolerance;
            set => this.putTolerance = CheckIfStrictlyPositive(value);
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

        public double TotalAvailable
        {
            get => this.totalAvailable;
            set => this.totalAvailable = CheckIfPositive(value);
        }

        public double TotalReservedForPick
        {
            get => this.totalReservedForPick;
            set => this.totalReservedForPick = CheckIfPositive(value);
        }

        public double TotalReservedToPut
        {
            get => this.totalReservedToPut;
            set => this.totalReservedToPut = CheckIfPositive(value);
        }

        public double TotalStock
        {
            get => this.totalStock;
            set => this.totalStock = CheckIfPositive(value);
        }

        public double? Width
        {
            get => this.width;
            set => this.width = CheckIfStrictlyPositive(value);
        }

        #endregion
    }
}

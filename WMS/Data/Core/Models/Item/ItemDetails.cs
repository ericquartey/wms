using System;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Performance",
        "CA1819: Properties should not return arrays",
        Justification = "Needed to upload image as byte[]")]
    public class ItemDetails : BaseModel<int>, IItemPickPolicy, IItemDeletePolicy, IItemPutPolicy, IItemUpdatePolicy
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

        private double? width;

        #endregion

        #region Properties

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

        public bool HasAssociatedAreas { get; set; }

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

        public byte[] UploadImageData { get; set; }

        public string UploadImageName { get; set; }

        public double? Width
        {
            get => this.width;
            set => this.width = CheckIfStrictlyPositive(value);
        }

        #endregion
    }
}

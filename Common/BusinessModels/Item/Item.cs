using System;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public sealed class Item : BusinessObject
    {
        #region Fields

        private int? averageWeight;
        private int? fifoTimePick;
        private int? fifoTimeStore;
        private int? height;
        private string image;
        private int? inventoryTolerance;
        private int? length;
        private int? pickTolerance;
        private int? reorderQuantity;
        private int? storeTolerance;
        private int totalAvailable;
        private int totalReservedForPick;
        private int totalReservedToStore;
        private int totalStock;
        private int? width;

        #endregion Fields

        #region Properties

        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClassDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemAverageWeight), ResourceType = typeof(BusinessObjects))]
        public int? AverageWeight
        {
            get => this.averageWeight;
            set => this.SetIfStrictlyPositive(ref this.averageWeight, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemCode), ResourceType = typeof(BusinessObjects))]
        public string Code { get; set; }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemDescription), ResourceType = typeof(BusinessObjects))]
        public string Description { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemFifoPickTime), ResourceType = typeof(BusinessObjects))]
        public int? FifoTimePick
        {
            get => this.fifoTimePick;
            set => this.SetIfStrictlyPositive(ref this.fifoTimePick, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemFifoStoreTime), ResourceType = typeof(BusinessObjects))]
        public int? FifoTimeStore
        {
            get => this.fifoTimeStore;
            set => this.SetIfStrictlyPositive(ref this.fifoTimeStore, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemHeight), ResourceType = typeof(BusinessObjects))]
        public int? Height
        {
            get => this.height;
            set => this.SetIfStrictlyPositive(ref this.height, value);
        }

        public string Image
        {
            get => this.image;
            set => this.SetProperty(ref this.image, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemLastInventoryDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? InventoryDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemInventoryTolerance), ResourceType = typeof(BusinessObjects))]
        public int? InventoryTolerance
        {
            get => this.inventoryTolerance;
            set => this.SetIfStrictlyPositive(ref this.inventoryTolerance, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemCategory), ResourceType = typeof(BusinessObjects))]
        public string ItemCategoryDescription { get; set; }

        [Display(Name = nameof(General.LastModificationDate), ResourceType = typeof(General))]
        public DateTime? LastModificationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemLastPickDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastPickDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemLastStoreDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastStoreDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemLength), ResourceType = typeof(BusinessObjects))]
        public int? Length
        {
            get => this.length;
            set => this.SetIfStrictlyPositive(ref this.length, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemManagementType), ResourceType = typeof(BusinessObjects))]
        public string ManagementTypeDescription { get; set; }

        [Display(Name = nameof(General.UnitOfMeasurement), ResourceType = typeof(General))]
        public string MeasureUnitDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemPickTolerance), ResourceType = typeof(BusinessObjects))]
        public int? PickTolerance
        {
            get => this.pickTolerance;
            set => this.SetIfStrictlyPositive(ref this.pickTolerance, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemReorderPoint), ResourceType = typeof(BusinessObjects))]
        public int? ReorderPoint { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemReorderQuantity), ResourceType = typeof(BusinessObjects))]
        public int? ReorderQuantity
        {
            get => this.reorderQuantity;
            set => this.SetIfStrictlyPositive(ref this.reorderQuantity, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemStoreTolerance), ResourceType = typeof(BusinessObjects))]
        public int? StoreTolerance
        {
            get => this.storeTolerance;
            set => this.SetIfStrictlyPositive(ref this.storeTolerance, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemAvailable), ResourceType = typeof(BusinessObjects))]
        public int TotalAvailable
        {
            get => this.totalAvailable;
            set => this.SetIfPositive(ref this.totalAvailable, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemReservedForPick), ResourceType = typeof(BusinessObjects))]
        public int TotalReservedForPick
        {
            get => this.totalReservedForPick;
            set => this.SetIfPositive(ref this.totalReservedForPick, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemReservedToStore), ResourceType = typeof(BusinessObjects))]
        public int TotalReservedToStore
        {
            get => this.totalReservedToStore;
            set => this.SetIfPositive(ref this.totalReservedToStore, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemStock), ResourceType = typeof(BusinessObjects))]
        public int TotalStock
        {
            get => this.totalStock;
            set => this.SetIfPositive(ref this.totalStock, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemWidth), ResourceType = typeof(BusinessObjects))]
        public int? Width
        {
            get => this.width;
            set => this.SetIfStrictlyPositive(ref this.width, value);
        }

        #endregion Properties
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public sealed class ItemDetails : BusinessObject
    {
        #region Fields

        private int? averageWeight;
        private int? height;
        private int? inventoryTolerance;
        private int? length;
        private int? pickTolerance;
        private int? reorderQuantity;
        private int? storeTolerance;
        private int? width;

        #endregion Fields

        #region Properties

        public IEnumerable<EnumerationString> AbcClassChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClassId { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemAverageWeight), ResourceType = typeof(BusinessObjects))]
        public int? AverageWeight
        {
            get => this.averageWeight;
            set => this.SetIfStrictlyPositive(ref this.averageWeight, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemCode), ResourceType = typeof(BusinessObjects))]
        public string Code { get; set; }

        public IEnumerable<Compartment> Compartments { get; set; }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemDescription), ResourceType = typeof(BusinessObjects))]
        public string Description { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemFifoPickTime), ResourceType = typeof(BusinessObjects))]
        public int? FifoTimePick { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemFifoStoreTime), ResourceType = typeof(BusinessObjects))]
        public int? FifoTimeStore { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemHeight), ResourceType = typeof(BusinessObjects))]
        public int? Height
        {
            get => this.height;
            set => this.SetIfStrictlyPositive(ref this.height, value);
        }

        public string Image { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemLastInventoryDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? InventoryDate { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemInventoryTolerance), ResourceType = typeof(BusinessObjects))]
        public int? InventoryTolerance
        {
            get => this.inventoryTolerance;
            set => this.SetIfStrictlyPositive(ref this.inventoryTolerance, value);
        }

        public IEnumerable<Enumeration> ItemCategoryChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemCategory), ResourceType = typeof(BusinessObjects))]
        public int? ItemCategoryId { get; set; }

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
        public ItemManagementType ManagementType { get; set; }

        public IEnumerable<(ItemManagementType Id, string Description)> ManagementTypeChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemManagementType), ResourceType = typeof(BusinessObjects))]
        public string ManagementTypeDescription => this.ManagementType.ToString();

        public IEnumerable<EnumerationString> MeasureUnitChoices { get; set; }

        [Display(Name = nameof(General.UnitOfMeasurement), ResourceType = typeof(General))]
        public string MeasureUnitDescription { get; set; }

        [Display(Name = nameof(General.UnitOfMeasurement), ResourceType = typeof(General))]
        public string MeasureUnitId { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemNotes), ResourceType = typeof(BusinessObjects))]
        public string Note { get; set; }

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
        public int TotalAvailable { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemWidth), ResourceType = typeof(BusinessObjects))]
        public int? Width
        {
            get => this.width;
            set => this.SetIfStrictlyPositive(ref this.width, value);
        }

        #endregion Properties
    }
}

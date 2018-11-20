using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public sealed class ItemDetails : BusinessObject
    {
        #region Fields

        private string abcClassId;
        private int? averageWeight;
        private string code;
        private DateTime creationDate;
        private string description;
        private int? fifoTimePick;
        private int? fifoTimeStore;
        private int? height;
        private string image;
        private DateTime? inventoryDate;
        private int? inventoryTolerance;
        private int? itemCategoryId;
        private Int32? itemManagementTypeId;
        private DateTime? lastModificationDate;
        private DateTime? lastPickDate;
        private DateTime? lastStoreDate;
        private int? length;
        private int managementType;
        private string measureUnitId;
        private string note;
        private int? pickTolerance;
        private int? reorderPoint;
        private int? reorderQuantity;
        private int? storeTolerance;
        private int totalAvailable;
        private int? width;

        #endregion Fields

        #region Properties

        public IEnumerable<EnumerationString> AbcClassChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClassId
        {
            get => this.abcClassId;
            set => this.SetProperty(ref this.abcClassId, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemAverageWeight), ResourceType = typeof(BusinessObjects))]
        public int? AverageWeight
        {
            get => this.averageWeight;
            set => this.SetIfStrictlyPositive(ref this.averageWeight, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemCode), ResourceType = typeof(BusinessObjects))]
        public string Code
        {
            get => this.code;
            set => this.SetProperty(ref this.code, value);
        }

        public IEnumerable<Compartment> Compartments { get; set; }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate
        {
            get => this.creationDate;
            set => this.SetProperty(ref this.creationDate, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemDescription), ResourceType = typeof(BusinessObjects))]
        public string Description
        {
            get => this.description;
            set => this.SetProperty(ref this.description, value);
        }

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
        public DateTime? InventoryDate
        {
            get => this.inventoryDate;
            set => this.SetProperty(ref this.inventoryDate, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemInventoryTolerance), ResourceType = typeof(BusinessObjects))]
        public int? InventoryTolerance
        {
            get => this.inventoryTolerance;
            set => this.SetIfStrictlyPositive(ref this.inventoryTolerance, value);
        }

        public IEnumerable<Enumeration> ItemCategoryChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemCategory), ResourceType = typeof(BusinessObjects))]
        public int? ItemCategoryId
        {
            get => this.itemCategoryId;
            set => this.SetProperty(ref this.itemCategoryId, value);
        }

        public IEnumerable<Enumeration> ManagementTypeChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemManagementType), ResourceType = typeof(BusinessObjects))]
        public string ManagementTypeDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemManagementType), ResourceType = typeof(BusinessObjects))]
        public int? ManagementTypeId
        {
            get => this.itemManagementTypeId;
            set => this.SetProperty(ref this.itemManagementTypeId, value);
        }

        [Display(Name = nameof(General.LastModificationDate), ResourceType = typeof(General))]
        public DateTime? LastModificationDate
        {
            get => this.lastModificationDate;
            set => this.SetProperty(ref this.lastModificationDate, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemLastPickDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastPickDate
        {
            get => this.lastPickDate;
            set => this.SetProperty(ref this.lastPickDate, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemLastStoreDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastStoreDate
        {
            get => this.lastStoreDate;
            set => this.SetProperty(ref this.lastStoreDate, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemLength), ResourceType = typeof(BusinessObjects))]
        public int? Length
        {
            get => this.length;
            set => this.SetIfStrictlyPositive(ref this.length, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemManagementType), ResourceType = typeof(BusinessObjects))]
        public int ManagementType
        {
            get => this.managementType;
            set => this.SetProperty(ref this.managementType, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemManagementType), ResourceType = typeof(BusinessObjects))]
        public int ManagementType { get; set; }

        public IEnumerable<Enumeration> ManagementTypeChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemManagementType), ResourceType = typeof(BusinessObjects))]
        public string ManagementTypeDescription => this.ManagementType.ToString();

        public IEnumerable<EnumerationString> MeasureUnitChoices { get; set; }

        [Display(Name = nameof(General.UnitOfMeasurement), ResourceType = typeof(General))]
        public string MeasureUnitDescription { get; set; }

        [Display(Name = nameof(General.UnitOfMeasurement), ResourceType = typeof(General))]
        public string MeasureUnitId
        {
            get => this.measureUnitId;
            set => this.SetProperty(ref this.measureUnitId, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemNotes), ResourceType = typeof(BusinessObjects))]
        public string Note
        {
            get => this.note;
            set => this.SetProperty(ref this.note, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemPickTolerance), ResourceType = typeof(BusinessObjects))]
        public int? PickTolerance
        {
            get => this.pickTolerance;
            set => this.SetIfStrictlyPositive(ref this.pickTolerance, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemReorderPoint), ResourceType = typeof(BusinessObjects))]
        public int? ReorderPoint
        {
            get => this.reorderPoint;
            set => this.SetIfStrictlyPositive(ref this.reorderPoint, value);
        }

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
            set => this.SetIfStrictlyPositive(ref this.totalAvailable, value);
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

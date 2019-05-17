using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils;

namespace Ferretto.WMS.App.Core.Models
{
    [Resource(nameof(Data.WebAPI.Contracts.Item))]
    public sealed class ItemDetails : BusinessObject
    {
        #region Fields

        private string abcClassId;

        private int? averageWeight;

        private string code;

        private DateTime creationDate;

        private string description;

        private int? fifoTimePick;

        private int? fifoTimePut;

        private double? height;

        private string image;

        private string imagePath;

        private DateTime? inventoryDate;

        private int? inventoryTolerance;

        private int? itemCategoryId;

        private DateTime? lastModificationDate;

        private DateTime? lastPickDate;

        private DateTime? lastPutDate;

        private double? length;

        private ItemManagementType? managementType;

        private string measureUnitId;

        private string note;

        private int? pickTolerance;

        private int? putTolerance;

        private int? reorderPoint;

        private int? reorderQuantity;

        private double totalAvailable;

        private double? width;

        #endregion

        #region Properties

        public IEnumerable<EnumerationString> AbcClassChoices { get; set; }

        [Required]
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
            set => this.SetProperty(ref this.averageWeight, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.ItemCode), ResourceType = typeof(BusinessObjects))]
        public string Code
        {
            get => this.code;
            set => this.SetProperty(ref this.code, value);
        }

        public IEnumerable<Compartment> Compartments { get; set; }

        public int CompartmentsCount { get; set; }

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
            set => this.SetProperty(ref this.fifoTimePick, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemFifoPutTime), ResourceType = typeof(BusinessObjects))]
        public int? FifoTimePut
        {
            get => this.fifoTimePut;
            set => this.SetProperty(ref this.fifoTimePut, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemHeight), ResourceType = typeof(BusinessObjects))]
        public double? Height
        {
            get => this.height;
            set => this.SetProperty(ref this.height, value);
        }

        public string Image
        {
            get => this.image;
            set => this.SetProperty(ref this.image, value);
        }

        public string ImagePath
        {
            get => this.imagePath;
            set => this.SetProperty(ref this.imagePath, value);
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
            set => this.SetProperty(ref this.inventoryTolerance, value);
        }

        public IEnumerable<Enumeration> ItemCategoryChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemCategory), ResourceType = typeof(BusinessObjects))]
        public int? ItemCategoryId
        {
            get => this.itemCategoryId;
            set => this.SetProperty(ref this.itemCategoryId, value);
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

        [Display(Name = nameof(BusinessObjects.ItemLastPutDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastPutDate
        {
            get => this.lastPutDate;
            set => this.SetProperty(ref this.lastPutDate, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemLength), ResourceType = typeof(BusinessObjects))]
        public double? Length
        {
            get => this.length;
            set => this.SetProperty(ref this.length, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.ItemManagementType), ResourceType = typeof(BusinessObjects))]
        public ItemManagementType? ManagementType
        {
            get => this.managementType;
            set => this.SetProperty(ref this.managementType, value);
        }

        public IEnumerable<Enumeration> ManagementTypeChoices { get; set; }

        public IEnumerable<EnumerationString> MeasureUnitChoices { get; set; }

        [Display(Name = nameof(General.MeasureUnit), ResourceType = typeof(General))]
        public string MeasureUnitDescription { get; set; }

        [Display(Name = nameof(General.MeasureUnit), ResourceType = typeof(General))]
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
            set => this.SetProperty(ref this.pickTolerance, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemPutTolerance), ResourceType = typeof(BusinessObjects))]
        public int? PutTolerance
        {
            get => this.putTolerance;
            set => this.SetProperty(ref this.putTolerance, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemReorderPoint), ResourceType = typeof(BusinessObjects))]
        public int? ReorderPoint
        {
            get => this.reorderPoint;
            set => this.SetProperty(ref this.reorderPoint, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemReorderQuantity), ResourceType = typeof(BusinessObjects))]
        public int? ReorderQuantity
        {
            get => this.reorderQuantity;
            set => this.SetProperty(ref this.reorderQuantity, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemAvailable), ResourceType = typeof(BusinessObjects))]
        public double TotalAvailable
        {
            get => this.totalAvailable;
            set => this.SetProperty(ref this.totalAvailable, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemWidth), ResourceType = typeof(BusinessObjects))]
        public double? Width
        {
            get => this.width;
            set => this.SetProperty(ref this.width, value);
        }

        #endregion

        #region Indexers

        public override string this[string columnName]
        {
            get
            {
                if (!this.IsValidationEnabled)
                {
                    return null;
                }

                var baseError = base[columnName];
                if (!string.IsNullOrEmpty(baseError))
                {
                    return baseError;
                }

                switch (columnName)
                {
                    case nameof(this.AverageWeight):

                        return this.GetErrorMessageIfNegativeOrZero(this.AverageWeight, columnName);

                    case nameof(this.FifoTimePick):

                        return this.GetErrorMessageIfNegativeOrZero(this.FifoTimePick, columnName);

                    case nameof(this.FifoTimePut):

                        return this.GetErrorMessageIfNegativeOrZero(this.FifoTimePut, columnName);

                    case nameof(this.Height):

                        return this.GetErrorMessageIfNegativeOrZero(this.Height, columnName);

                    case nameof(this.InventoryTolerance):

                        return this.GetErrorMessageIfNegativeOrZero(this.InventoryTolerance, columnName);

                    case nameof(this.Length):

                        return this.GetErrorMessageIfNegativeOrZero(this.Length, columnName);

                    case nameof(this.PickTolerance):

                        return this.GetErrorMessageIfNegativeOrZero(this.PickTolerance, columnName);

                    case nameof(this.ReorderPoint):

                        return this.GetErrorMessageIfNegativeOrZero(this.ReorderPoint, columnName);

                    case nameof(this.ReorderQuantity):

                        return this.GetErrorMessageIfNegativeOrZero(this.ReorderQuantity, columnName);

                    case nameof(this.PutTolerance):

                        return this.GetErrorMessageIfNegativeOrZero(this.PutTolerance, columnName);

                    case nameof(this.TotalAvailable):

                        return this.GetErrorMessageIfNegative(this.TotalAvailable, columnName);

                    case nameof(this.Width):

                        return this.GetErrorMessageIfNegativeOrZero(this.Width, columnName);
                }

                return null;
            }
        }

        #endregion
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public sealed class Item : BusinessObject
    {
        #region Fields

        private int? averageWeight;

        private int? fifoTimePick;

        private int? fifoTimePut;

        private double? height;

        private string image;

        private int? inventoryTolerance;

        private double? depth;

        private int? pickTolerance;

        private int? putTolerance;

        private int? reorderQuantity;

        private double totalAvailable;

        private double totalReservedForPick;

        private double totalReservedToPut;

        private double totalStock;

        private double? width;

        #endregion

        #region Properties

        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClassDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemAverageWeight), ResourceType = typeof(BusinessObjects))]
        public int? AverageWeight
        {
            get => this.averageWeight;
            set => this.SetProperty(ref this.averageWeight, value);
        }

        [Display(Name = nameof(BusinessObjects.Code), ResourceType = typeof(BusinessObjects))]
        public string Code { get; set; }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.Description), ResourceType = typeof(BusinessObjects))]
        public string Description { get; set; }

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

        [Display(Name = nameof(BusinessObjects.Height), ResourceType = typeof(BusinessObjects))]
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

        [Display(Name = nameof(BusinessObjects.LastInventoryDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? InventoryDate { get; set; }

        [Display(Name = nameof(BusinessObjects.InventoryTolerance), ResourceType = typeof(BusinessObjects))]
        public int? InventoryTolerance
        {
            get => this.inventoryTolerance;
            set => this.SetProperty(ref this.inventoryTolerance, value);
        }

        [Display(Name = nameof(BusinessObjects.Category), ResourceType = typeof(BusinessObjects))]
        public string ItemCategoryDescription { get; set; }

        [Display(Name = nameof(General.LastModificationDate), ResourceType = typeof(General))]
        public DateTime? LastModificationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.LastPickDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastPickDate { get; set; }

        [Display(Name = nameof(BusinessObjects.LastPutDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastPutDate { get; set; }

        [Display(Name = nameof(BusinessObjects.Depth), ResourceType = typeof(BusinessObjects))]
        public double? Depth
        {
            get => this.depth;
            set => this.SetProperty(ref this.depth, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemManagementType), ResourceType = typeof(BusinessObjects))]
        public string ManagementTypeDescription { get; set; }

        [Display(Name = nameof(General.MeasureUnit), ResourceType = typeof(General))]
        public string MeasureUnitDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.PickTolerance), ResourceType = typeof(BusinessObjects))]
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
        public int? ReorderPoint { get; set; }

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

        [Display(Name = nameof(BusinessObjects.ReservedForPick), ResourceType = typeof(BusinessObjects))]
        public double TotalReservedForPick
        {
            get => this.totalReservedForPick;
            set => this.SetProperty(ref this.totalReservedForPick, value);
        }

        [Display(Name = nameof(BusinessObjects.ReservedToPut), ResourceType = typeof(BusinessObjects))]
        public double TotalReservedToPut
        {
            get => this.totalReservedToPut;
            set => this.SetProperty(ref this.totalReservedToPut, value);
        }

        [Display(Name = nameof(BusinessObjects.Stock), ResourceType = typeof(BusinessObjects))]
        public double TotalStock
        {
            get => this.totalStock;
            set => this.SetProperty(ref this.totalStock, value);
        }

        [Display(Name = nameof(BusinessObjects.Width), ResourceType = typeof(BusinessObjects))]
        public double? Width
        {
            get => this.width;
            set => this.SetProperty(ref this.width, value);
        }

        #endregion
    }
}

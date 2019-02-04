using System;
using System.Collections.Generic;
using System.Linq;

namespace Ferretto.WMS.Data.WebAPI.Models
{
    public class Item : Model<int>
    {
        #region Fields

        private string abcClassDescription;

        private string abcClassId;

        private int? averageWeight;

        private int? fifoTimePick;

        private int? fifoTimeStore;

        private int? height;

        private int? inventoryTolerance;

        private string itemCategoryDescription;

        private int? itemCategoryId;

        private int? length;

        private int? pickTolerance;

        private int? reorderPoint;

        private int? reorderQuantity;

        private int? storeTolerance;

        private int totalAvailable;

        private int totalReservedForPick;

        private int totalReservedToStore;

        private int totalStock;

        private int? width;

        #endregion

        #region Properties

        public string AbcClassDescription
        {
            get => this.abcClassDescription;
            private set => this.abcClassDescription = value;
        }

        [Newtonsoft.Json.JsonIgnore]
        public IEnumerable<AbcClass> AbcClasses { get; set; }

        public string AbcClassId
        {
            get => this.abcClassId;
            set
            {
                this.abcClassId = value;
                this.ComputeAbcClassDescription();
            }
        }

        public int? AverageWeight
        {
            get => this.averageWeight;
            set => this.averageWeight = CheckIfStrictlyPositive(value);
        }

        public string Code { get; set; }

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

        public int? Height
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

        [Newtonsoft.Json.JsonIgnore]
        public IEnumerable<ItemCategory> ItemCategories { get; set; }

        public string ItemCategoryDescription
        {
            get => this.itemCategoryDescription;
            private set => this.itemCategoryDescription = value;
        }

        public int? ItemCategoryId
        {
            get => this.itemCategoryId;
            set
            {
                this.itemCategoryId = value;
                this.ComputeItemCategoryDescription();
            }
        }

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

        public int? StoreTolerance
        {
            get => this.storeTolerance;
            set => this.storeTolerance = CheckIfStrictlyPositive(value);
        }

        public int TotalAvailable
        {
            get => this.totalAvailable;
            private set => this.totalAvailable = CheckIfPositive(value);
        }

        public int TotalReservedForPick
        {
            get => this.totalReservedForPick;
            set
            {
                this.totalReservedForPick = CheckIfPositive(value);
                this.ComputeTotalAvailable();
            }
        }

        public int TotalReservedToStore
        {
            get => this.totalReservedToStore;
            set
            {
                this.totalReservedToStore = CheckIfPositive(value);
                this.ComputeTotalAvailable();
            }
        }

        public int TotalStock
        {
            get => this.totalStock;
            set
            {
                this.totalStock = CheckIfPositive(value);
                this.ComputeTotalAvailable();
            }
        }

        public int? Width
        {
            get => this.width;
            set => this.width = CheckIfStrictlyPositive(value);
        }

        #endregion

        #region Methods

        private void ComputeAbcClassDescription()
        {
            this.AbcClassDescription = this.AbcClasses?.SingleOrDefault(c => c.Id == this.AbcClassId)?.Description;
        }

        private void ComputeItemCategoryDescription()
        {
            this.ItemCategoryDescription = this.ItemCategories?.SingleOrDefault(c => c.Id == this.itemCategoryId)?.Description;
        }

        private void ComputeTotalAvailable()
        {
            this.TotalAvailable = this.TotalStock + this.TotalReservedToStore - this.TotalReservedForPick;
        }

        #endregion
    }
}

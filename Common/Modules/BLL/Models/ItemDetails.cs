using System;
using System.Collections.Generic;
using Ferretto.Common.Utils;

namespace Ferretto.Common.Modules.BLL.Models
{
    public class ItemDetails : BusinessObject, IEntity<int>
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

        public IEnumerable<AbcClass> AbcClassChoices { get; set; }

        public int? AverageWeight
        {
            get => this.averageWeight;
            set => SetIfStrictlyPositive(ref this.averageWeight, value);
        }

        public string ClassId { get; set; }
        public string Code { get; set; }
        public IEnumerable<Compartment> Compartments { get; set; }
        public DateTime CreationDate { get; set; }
        public string Description { get; set; }
        public int? FifoTimePick { get; set; }
        public int? FifoTimeStore { get; set; }

        public int? Height
        {
            get => this.height;
            set => SetIfStrictlyPositive(ref this.height, value);
        }

        public int Id { get; set; }
        public string Image { get; set; }
        public DateTime? InventoryDate { get; set; }

        public int? InventoryTolerance
        {
            get => this.inventoryTolerance;
            set => SetIfStrictlyPositive(ref this.inventoryTolerance, value);
        }

        public IEnumerable<ItemManagementType> ItemManagementTypeChoices { get; set; }
        public int? ItemManagementTypeId { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public DateTime? LastPickDate { get; set; }
        public DateTime? LastStoreDate { get; set; }

        public int? Length
        {
            get => this.length;
            set => SetIfStrictlyPositive(ref this.length, value);
        }

        public IEnumerable<MeasureUnit> MeasureUnitChoices { get; set; }
        public string MeasureUnitId { get; set; }
        public string Note { get; set; }

        public int? PickTolerance
        {
            get => this.pickTolerance;
            set => SetIfStrictlyPositive(ref this.pickTolerance, value);
        }

        public int? ReorderPoint { get; set; }

        public int? ReorderQuantity
        {
            get => this.reorderQuantity;
            set => SetIfStrictlyPositive(ref this.reorderQuantity, value);
        }

        public int? StoreTolerance
        {
            get => this.storeTolerance;
            set => SetIfStrictlyPositive(ref this.storeTolerance, value);
        }

        public int? Width
        {
            get => this.width;
            set => SetIfStrictlyPositive(ref this.width, value);
        }

        #endregion Properties
    }
}

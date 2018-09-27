using System;
using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Articolo
    public class Item
    {
        #region Properties

        public virtual AbcClass AbcClass { get; set; }
        public string AbcClassId { get; set; }
        public int? AverageWeight { get; set; }
        public string Code { get; set; }
        public IEnumerable<Compartment> Compartments { get; set; }
        public DateTime CreationDate { get; set; }
        public string Description { get; set; }
        public int? FifoTimePick { get; set; }
        public int? FifoTimeStore { get; set; }
        public int? Height { get; set; }
        public int Id { get; set; }
        public string Image { get; set; }
        public DateTime? InventoryDate { get; set; }
        public int? InventoryTolerance { get; set; }
        public IEnumerable<ItemArea> ItemAreas { get; set; }
        public IEnumerable<ItemListRow> ItemListRows { get; set; }
        public virtual ItemManagementType ItemManagementType { get; set; }
        public int? ItemManagementTypeId { get; set; }
        public IEnumerable<ItemCompartmentType> ItemsCompartmentTypes { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public DateTime? LastPickDate { get; set; }
        public DateTime? LastStoreDate { get; set; }
        public int? Length { get; set; }
        public virtual MeasureUnit MeasureUnit { get; set; }
        public string MeasureUnitId { get; set; }
        public IEnumerable<Mission> Missions { get; set; }
        public string Note { get; set; }
        public int? PickTolerance { get; set; }
        public int? ReorderPoint { get; set; }
        public int? ReorderQuantity { get; set; }
        public int? StoreTolerance { get; set; }
        public int? Width { get; set; }

        #endregion Properties
    }
}

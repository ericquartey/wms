using System;
using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Articolo
    public partial class Item
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public AbcClass? Class { get; set; }
        public int? MeasureUnitId { get; set; }
        public int? Width { get; set; }
        public int? Length { get; set; }
        public int? Height { get; set; }
        public int? ItemManagementTypeId { get; set; }
        public int? FifoTimeStore { get; set; }
        public int? FifoTimePick { get; set; }
        public int? ReorderPoint { get; set; }
        public int? ReorderQuantity { get; set; }
        public int? AverageWeight { get; set; }
        public int? PickTolerance { get; set; }
        public int? StoreTolerance { get; set; }
        public int? InventoryTolerance { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public DateTime? InventoryDate { get; set; }
        public DateTime? LastPickDate { get; set; }
        public DateTime? LastStoreDate { get; set; }
        public byte[] Image { get; set; }
        public string Note { get; set; }

        public MeasureUnit MeasureUnit { get; set; }
        public ItemManagementType ItemManagementType { get; set; }

        public List<Compartment> Compartments { get; set; }
        public List<ItemArea> ItemAreas { get; set; }
        public List<ItemListRow> ItemListRows { get; set; }
    }
}

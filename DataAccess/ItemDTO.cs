using System;
using System.Collections.Generic;
using System.Linq;

namespace Ferretto.Common.DataAccess
{
    public class ItemDTO
    {
        #region Properties

        public string AbcClassDescription { get; set; }

        public string AbcClassId { get; set; }

        public int? AverageWeight { get; set; }

        public string Code { get; set; }
        public DateTime CreationDate { get; set; }
        public string Description { get; set; }

        public int? FifoTimePick { get; set; }

        public int? FifoTimeStore { get; set; }

        public int? Height { get; set; }

        public int Id { get; set; }

        public DateTime? InventoryDate { get; set; }

        public int? InventoryTolerance { get; set; }

        public string ItemManagementTypeDescription { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public DateTime? LastPickDate { get; set; }
        public DateTime? LastStoreDate { get; set; }

        public int? Length { get; set; }

        public string MeasureUnitDescription { get; set; }

        public int? PickTolerance { get; set; }

        public int? ReorderPoint { get; set; }

        public int? ReorderQuantity { get; set; }

        public int? StoreTolerance { get; set; }

        public int TotalAvailable { get; set; }
        public int TotalReservedForPick { get; set; }
        public int TotalReservedToStore { get; set; }
        public int TotalStock { get; set; }

        public int? Width { get; set; }

        #endregion Properties
    }
}

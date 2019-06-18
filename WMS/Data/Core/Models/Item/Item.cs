using System;
using System.Collections.Generic;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Item))]
    public class Item : BaseModel<int>, IItemPickPolicy, IItemDeletePolicy, IItemPutPolicy, IItemUpdatePolicy
    {
        #region Properties

        public string AbcClassDescription { get; set; }

        public string AbcClassId { get; set; }

        [Positive]
        public int? AverageWeight { get; set; }

        public string Code { get; set; }

        public int CompartmentsCount { get; set; }

        public DateTime CreationDate { get; set; }

        public string Description { get; set; }

        [Positive]
        public int? FifoTimePick { get; set; }

        [Positive]
        public int? FifoTimePut { get; set; }

        public bool HasAssociatedAreas { get; set; }

        public bool HasCompartmentTypes { get; set; }

        [Positive]
        public double? Height { get; set; }

        public string Image { get; set; }

        public DateTime? InventoryDate { get; set; }

        [Positive]
        public int? InventoryTolerance { get; set; }

        public string ItemCategoryDescription { get; set; }

        public int? ItemCategoryId { get; set; }

        public int ItemListRowsCount { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastPutDate { get; set; }

        [Positive]
        public double? Length { get; set; }

        public IEnumerable<MachinePick> Machines { get; set; }

        public ItemManagementType ManagementType { get; set; }

        public string MeasureUnitDescription { get; set; }

        public string MeasureUnitId { get; set; }

        public int MissionsCount { get; set; }

        public string Note { get; set; }

        [Positive]
        public int? PickTolerance { get; set; }

        [Positive]
        public int? PutTolerance { get; set; }

        [Positive]
        public int? ReorderPoint { get; set; }

        [Positive]
        public int? ReorderQuantity { get; set; }

        public int SchedulerRequestsCount { get; set; }

        [PositiveOrZero]
        public double TotalAvailable { get; set; }

        [PositiveOrZero]
        public double TotalReservedForPick { get; set; }

        [PositiveOrZero]
        public double TotalReservedToPut { get; set; }

        [PositiveOrZero]
        public double TotalStock { get; set; }

        [Positive]
        public double? Width { get; set; }

        #endregion
    }
}

using System;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Models
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Performance",
        "CA1819: Properties should not return arrays",
        Justification = "Needed to upload image as byte[]")]
    [Resource(nameof(Item))]
    public class ItemDetails : BaseModel<int>, IItemPickPolicy, IItemDeletePolicy, IItemPutPolicy, IItemUpdatePolicy
    {
        #region Properties

        public string AbcClassId { get; set; }

        [Positive]
        public int? AverageWeight { get; set; }

        [Unique]
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

        public int? ItemCategoryId { get; set; }

        public int ItemListRowsCount { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastPutDate { get; set; }

        [Positive]
        public double? Depth { get; set; }

        public Enums.ItemManagementType ManagementType { get; set; }

        public string MeasureUnitDescription { get; set; }

        public string MeasureUnitId { get; set; }

        public int MissionOperationsCount { get; set; }

        public string Note { get; set; }

        [Positive]
        public int? PickTolerance { get; set; }

        [Positive]
        public int? PutTolerance { get; set; }

        [Positive]
        public int? ReorderPoint { get; set; }

        [Positive]
        public int? ReorderQuantity { get; set; }

        [PositiveOrZero]
        public int SchedulerRequestsCount { get; set; }

        [PositiveOrZero]
        public double TotalAvailable { get; set; }

        public byte[] UploadImageData { get; set; }

        public string UploadImageName { get; set; }

        [Positive]
        public double? Width { get; set; }

        #endregion
    }
}

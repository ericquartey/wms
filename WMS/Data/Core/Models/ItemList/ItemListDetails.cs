using System;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;
using Newtonsoft.Json;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(ItemList))]
    public class ItemListDetails : BaseModel<int>, IItemListPolicy, IItemListDeletePolicy
    {
        #region Properties

        public string AreaName { get; set; }

        [Unique]
        public string Code { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int CompletedRowsCount { get; internal set; }

        public DateTime CreationDate { get; set; }

        public string CustomerOrderCode { get; set; }

        public string CustomerOrderDescription { get; set; }

        public string Description { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int ErrorRowsCount { get; internal set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int ExecutingRowsCount { get; internal set; }

        public DateTime? ExecutionEndDate { get; set; }

        public DateTime? FirstExecutionDate { get; set; }

        [JsonIgnore]
        public bool HasActiveRows { get; internal set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int IncompleteRowsCount { get; internal set; }

        [PositiveOrZero]
        public int ItemListRowsCount { get; set; }

        public ItemListType ItemListType { get; set; }

        public string ItemListTypeDescription { get; set; }

        public string Job { get; set; }

        public DateTime? LastModificationDate { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int NewRowsCount { get; internal set; }

        [Positive]
        public int? Priority { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int ReadyRowsCount { get; internal set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int RowsCount { get; set; }

        public bool ShipmentUnitAssociated { get; set; }

        public string ShipmentUnitCode { get; set; }

        public string ShipmentUnitDescription { get; set; }

        public ItemListStatus Status => ItemList.GetStatus(
           this.RowsCount,
           this.CompletedRowsCount,
           this.NewRowsCount,
           this.ExecutingRowsCount,
           this.WaitingRowsCount,
           this.IncompleteRowsCount,
           this.SuspendedRowsCount,
           this.ErrorRowsCount,
           this.ReadyRowsCount);

        [JsonIgnore]
        [PositiveOrZero]
        public int SuspendedRowsCount { get; internal set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int WaitingRowsCount { get; internal set; }

        #endregion
    }
}

﻿using System;
using Newtonsoft.Json;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemListDetails : BaseModel<int>, IPolicyItemList, IItemListDeletePolicy
    {
        #region Fields

        private int? priority;

        #endregion

        #region Properties

        public string AreaName { get; set; }

        public string Code { get; set; }

        [JsonIgnore]
        public int CompletedRowsCount { get; internal set; }

        public DateTime CreationDate { get; set; }

        public string CustomerOrderCode { get; set; }

        public string CustomerOrderDescription { get; set; }

        public string Description { get; set; }

        [JsonIgnore]
        public int ExecutingRowsCount { get; internal set; }

        public DateTime? ExecutionEndDate { get; set; }

        public DateTime? FirstExecutionDate { get; set; }

        [JsonIgnore]
        public bool HasActiveRows { get; internal set; }

        [JsonIgnore]
        public int IncompleteRowsCount { get; internal set; }

        public int ItemListRowsCount { get; set; }

        public ItemListType ItemListType { get; set; }

        public string ItemListTypeDescription { get; set; }

        public string Job { get; set; }

        public DateTime? LastModificationDate { get; set; }

        [JsonIgnore]
        public int NewRowsCount { get; internal set; }

        public int? Priority
        {
            get => this.priority;
            set => this.priority = CheckIfStrictlyPositive(value);
        }

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
           this.SuspendedRowsCount);

        [JsonIgnore]
        public int SuspendedRowsCount { get; internal set; }

        [JsonIgnore]
        public int WaitingRowsCount { get; internal set; }

        #endregion
    }
}

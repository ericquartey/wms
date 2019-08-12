using System;
using System.Collections.Generic;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;
using Newtonsoft.Json;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(ItemList))]
    public class ItemList : BaseModel<int>, IItemListPolicy, IItemListDeletePolicy
    {
        #region Properties

        [Unique]
        public string Code { get; set; }

        [PositiveOrZero]
        public int CompletedRowsCount { get; set; }

        public DateTime CreationDate { get; set; }

        public string Description { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int ErrorRowsCount { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int ExecutingRowsCount { get; set; }

        [JsonIgnore]
        public bool HasActiveRows { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int IncompleteRowsCount { get; set; }

        [PositiveOrZero]
        public int ItemListRowsCount { get; set; }

        public Enums.ItemListType ItemListType { get; set; }

        public IEnumerable<Machine> Machines { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int NewRowsCount { get; set; }

        [PositiveOrZero]
        public int? Priority { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int ReadyRowsCount { get; set; }

        public string ShipmentUnitCode { get; set; }

        public string ShipmentUnitDescription { get; set; }

        public Enums.ItemListStatus Status => GetStatus(
            this.ItemListRowsCount,
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
        public int SuspendedRowsCount { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int WaitingRowsCount { get; set; }

        #endregion

        #region Methods

        internal static Enums.ItemListStatus GetStatus(
            int rowCount,
            int completedRowsCount,
            int newRowsCount,
            int executingRowsCount,
            int waitingRowsCount,
            int incompleteRowsCount,
            int suspendedRowsCount,
            int errorRowsCount,
            int readyRowsCount)
        {
            if (rowCount == 0 || rowCount == newRowsCount)
            {
                return Enums.ItemListStatus.New;
            }

            if (rowCount == completedRowsCount)
            {
                return Enums.ItemListStatus.Completed;
            }

            if (waitingRowsCount == rowCount)
            {
                return Enums.ItemListStatus.Waiting;
            }

            if (readyRowsCount == rowCount)
            {
                return Enums.ItemListStatus.Ready;
            }

            if (errorRowsCount > 0)
            {
                return Enums.ItemListStatus.Error;
            }

            if (waitingRowsCount > 0 || readyRowsCount > 0 || executingRowsCount > 0)
            {
                return Enums.ItemListStatus.Executing;
            }

            if (incompleteRowsCount > 0)
            {
                return Enums.ItemListStatus.Incomplete;
            }

            if (suspendedRowsCount > 0)
            {
                return Enums.ItemListStatus.Suspended;
            }

            // we can arrive here only with mixed New + Complete rows status
            return Enums.ItemListStatus.New;
        }

        #endregion
    }
}

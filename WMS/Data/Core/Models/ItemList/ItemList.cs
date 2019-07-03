using System;
using System.Collections.Generic;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;
using Newtonsoft.Json;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(ItemList))]
    public class ItemList : BaseModel<int>, IItemListPolicy, IItemListDeletePolicy
    {
        #region Properties

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

        public ItemListType ItemListType { get; set; }

        public IEnumerable<Machine> Machines { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int NewRowsCount { get; set; }

        [PositiveOrZero]
        public int? Priority { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int ReadyRowsCount { get; set; }

        public ItemListStatus Status => GetStatus(
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

        internal static ItemListStatus GetStatus(
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
                return ItemListStatus.New;
            }

            if (rowCount == completedRowsCount)
            {
                return ItemListStatus.Completed;
            }

            if (waitingRowsCount == rowCount)
            {
                return ItemListStatus.Waiting;
            }

            if (readyRowsCount == rowCount)
            {
                return ItemListStatus.Ready;
            }

            if (errorRowsCount > 0)
            {
                return ItemListStatus.Error;
            }

            if (waitingRowsCount > 0 || readyRowsCount > 0 || executingRowsCount > 0)
            {
                return ItemListStatus.Executing;
            }

            if (incompleteRowsCount > 0)
            {
                return ItemListStatus.Incomplete;
            }

            if (suspendedRowsCount > 0)
            {
                return ItemListStatus.Suspended;
            }

            // we can arrive here only with mixed New + Complete rows status
            return ItemListStatus.New;
        }

        #endregion
    }
}

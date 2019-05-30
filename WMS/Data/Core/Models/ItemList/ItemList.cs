using System;
using System.Collections.Generic;
using Ferretto.WMS.Data.Core.Interfaces;
using Newtonsoft.Json;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemList : BaseModel<int>, IPolicyItemList, IItemListDeletePolicy
    {
        #region Fields

        private int itemListRowsCount;

        private int? priority;

        #endregion

        #region Properties

        public string Code { get; set; }

        public int CompletedRowsCount { get; set; }

        public DateTime CreationDate { get; set; }

        public string Description { get; set; }

        [JsonIgnore]
        public int ErrorRowsCount { get; set; }

        [JsonIgnore]
        public int ExecutingRowsCount { get; set; }

        [JsonIgnore]
        public bool HasActiveRows { get; set; }

        [JsonIgnore]
        public int IncompleteRowsCount { get; set; }

        public int ItemListRowsCount
        {
            get => this.itemListRowsCount;
            set => this.itemListRowsCount = CheckIfPositive(value);
        }

        public ItemListType ItemListType { get; set; }

        public IEnumerable<Machine> Machines { get; set; }

        [JsonIgnore]
        public int NewRowsCount { get; set; }

        public int? Priority
        {
            get => this.priority;
            set => this.priority = CheckIfPositive(value);
        }

        [JsonIgnore]
        public int ReadyRowsCount { get; set; }

        public ItemListStatus Status => GetStatus(
            this.itemListRowsCount,
            this.CompletedRowsCount,
            this.NewRowsCount,
            this.ExecutingRowsCount,
            this.WaitingRowsCount,
            this.IncompleteRowsCount,
            this.SuspendedRowsCount,
            this.ErrorRowsCount,
            this.ReadyRowsCount);

        [JsonIgnore]
        public int SuspendedRowsCount { get; set; }

        [JsonIgnore]
        public int WaitingRowsCount { get; set; }

        #endregion

        #region Methods

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S107:Methods should not have too many parameters",
            Justification = "This method need to consider all this counters")]
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

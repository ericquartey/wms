using System;
using System.Collections.Generic;
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

        public int CompletedRowsCount { get; internal set; }

        public DateTime CreationDate { get; set; }

        public string Description { get; set; }

        [JsonIgnore]
        public int ExecutingRowsCount { get; internal set; }

        [JsonIgnore]
        public bool HasActiveRows { get; internal set; }

        [JsonIgnore]
        public int IncompleteRowsCount { get; internal set; }

        public int ItemListRowsCount
        {
            get => this.itemListRowsCount;
            set => this.itemListRowsCount = CheckIfPositive(value);
        }

        public ItemListType ItemListType { get; set; }

        public IEnumerable<Machine> Machines { get; set; }

        [JsonIgnore]
        public int NewRowsCount { get; internal set; }

        public int? Priority
        {
            get => this.priority;
            set => this.priority = CheckIfPositive(value);
        }

        public ItemListStatus Status => GetStatus(
            this.itemListRowsCount,
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

        #region Methods

        internal static ItemListStatus GetStatus(
            int rowCount,
            int completedRowsCount,
            int newRowsCount,
            int executingRowsCount,
            int waitingRowsCount,
            int incompleteRowsCount,
            int suspendedRowsCount)
        {
            if (rowCount == completedRowsCount)
            {
                return ItemListStatus.Completed;
            }

            if (rowCount == newRowsCount)
            {
                return ItemListStatus.New;
            }

            if (executingRowsCount > 0)
            {
                return ItemListStatus.Executing;
            }

            if (waitingRowsCount > 0)
            {
                return ItemListStatus.Waiting;
            }

            if (incompleteRowsCount > 0)
            {
                return ItemListStatus.Incomplete;
            }

            if (suspendedRowsCount > 0)
            {
                return ItemListStatus.Suspended;
            }

            throw new InvalidOperationException("Unable to determine list status.");
        }

        #endregion
    }
}

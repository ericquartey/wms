using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemListOperation : Model<int>
    {
        #region Properties

        public string Code { get; set; }

        public int CompletedRowsCount { get; set; }

        public int ExecutingRowsCount { get; set; }

        public int IncompleteRowsCount { get; set; }

        public int NewRowsCount { get; set; }

        public IEnumerable<ItemListRowOperation> Rows { get; set; }

        public int SuspendedRowsCount { get; set; }

        public int TotalRowsCount { get; set; }

        public int WaitingRowsCount { get; set; }

        #endregion

        #region Methods

        public ItemListStatus GetStatus() => GetStatus(
            this.TotalRowsCount,
            this.CompletedRowsCount,
            this.NewRowsCount,
            this.ExecutingRowsCount,
            this.WaitingRowsCount,
            this.IncompleteRowsCount,
            this.SuspendedRowsCount);

        internal static ItemListStatus GetStatus(
            int rowCount,
            int completedRowsCount,
            int newRowsCount,
            int executingRowsCount,
            int waitingRowsCount,
            int incompleteRowsCount,
            int suspendedRowsCount)
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

            if (incompleteRowsCount > 0)
            {
                return ItemListStatus.Incomplete;
            }

            if (suspendedRowsCount > 0)
            {
                return ItemListStatus.Suspended;
            }

            return ItemListStatus.Executing;
        }

        #endregion
    }
}

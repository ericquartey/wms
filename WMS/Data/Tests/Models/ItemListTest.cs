using Ferretto.WMS.Data.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public class ItemListTest
    {
        #region Methods

        [TestMethod]
        public void AllCompletedRowsStatus()
        {
            var list = new ItemList
            {
                ItemListRowsCount = 3,
                CompletedRowsCount = 3,
                NewRowsCount = 0,
                ExecutingRowsCount = 0,
                WaitingRowsCount = 0,
                IncompleteRowsCount = 0,
                SuspendedRowsCount = 0,
                ErrorRowsCount = 0,
                ReadyRowsCount = 0,
            };
            Assert.AreEqual(
                ItemListStatus.Completed,
                list.Status,
                "ItemList status should be Completed");
        }

        [TestMethod]
        public void AllNewRowsStatus()
        {
            var list = new ItemList
            {
                ItemListRowsCount = 3,
                CompletedRowsCount = 0,
                NewRowsCount = 3,
                ExecutingRowsCount = 0,
                WaitingRowsCount = 0,
                IncompleteRowsCount = 0,
                SuspendedRowsCount = 0,
                ErrorRowsCount = 0,
                ReadyRowsCount = 0,
            };
            Assert.AreEqual(
                ItemListStatus.New,
                list.Status,
                "ItemList status should be New");
        }

        [TestMethod]
        public void AllReadyRowsStatus()
        {
            var list = new ItemList
            {
                ItemListRowsCount = 3,
                CompletedRowsCount = 0,
                NewRowsCount = 0,
                ExecutingRowsCount = 0,
                WaitingRowsCount = 0,
                IncompleteRowsCount = 0,
                SuspendedRowsCount = 0,
                ErrorRowsCount = 0,
                ReadyRowsCount = 3,
            };
            Assert.AreEqual(
                ItemListStatus.Ready,
                list.Status,
                "ItemList status should be Ready");
        }

        [TestMethod]
        public void AllWaitingRowsStatus()
        {
            var list = new ItemList
            {
                ItemListRowsCount = 3,
                CompletedRowsCount = 0,
                NewRowsCount = 0,
                ExecutingRowsCount = 0,
                WaitingRowsCount = 3,
                IncompleteRowsCount = 0,
                SuspendedRowsCount = 0,
                ErrorRowsCount = 0,
                ReadyRowsCount = 0,
            };
            Assert.AreEqual(
                ItemListStatus.Waiting,
                list.Status,
                "ItemList status should be Waiting");
        }

        [TestMethod]
        public void MixedCompleteAndNewRowsStatus()
        {
            var list = new ItemList
            {
                ItemListRowsCount = 2,
                CompletedRowsCount = 1,
                NewRowsCount = 1,
                ExecutingRowsCount = 0,
                WaitingRowsCount = 0,
                IncompleteRowsCount = 0,
                SuspendedRowsCount = 0,
                ErrorRowsCount = 0,
                ReadyRowsCount = 0,
            };
            Assert.AreEqual(
                ItemListStatus.New,
                list.Status,
                "ItemList status should be New");
        }

        [TestMethod]
        public void NoRowsStatus()
        {
            var list = new ItemList
            {
                ItemListRowsCount = 0,
                CompletedRowsCount = 0,
                NewRowsCount = 0,
                ExecutingRowsCount = 0,
                WaitingRowsCount = 0,
                IncompleteRowsCount = 0,
                SuspendedRowsCount = 0,
                ErrorRowsCount = 0,
                ReadyRowsCount = 0,
            };
            Assert.AreEqual(
                ItemListStatus.New,
                list.Status,
                "ItemList status should be New");
        }

        [TestMethod]
        public void OneErrorRowsStatus()
        {
            var list = new ItemList
            {
                ItemListRowsCount = 7,
                CompletedRowsCount = 1,
                NewRowsCount = 0,
                ExecutingRowsCount = 1,
                WaitingRowsCount = 1,
                IncompleteRowsCount = 1,
                SuspendedRowsCount = 1,
                ErrorRowsCount = 1,
                ReadyRowsCount = 1,
            };
            Assert.AreEqual(
                ItemListStatus.Error,
                list.Status,
                "ItemList status should be Error");
        }

        [TestMethod]
        public void OneExecutingRowsStatus()
        {
            var list = new ItemList
            {
                ItemListRowsCount = 6,
                CompletedRowsCount = 1,
                NewRowsCount = 0,
                ExecutingRowsCount = 1,
                WaitingRowsCount = 1,
                IncompleteRowsCount = 1,
                SuspendedRowsCount = 1,
                ErrorRowsCount = 0,
                ReadyRowsCount = 1,
            };
            Assert.AreEqual(
                ItemListStatus.Executing,
                list.Status,
                "ItemList status should be Executing");
        }

        [TestMethod]
        public void OneIncompleteRowsStatus()
        {
            var list = new ItemList
            {
                ItemListRowsCount = 3,
                CompletedRowsCount = 1,
                NewRowsCount = 0,
                ExecutingRowsCount = 0,
                WaitingRowsCount = 0,
                IncompleteRowsCount = 1,
                SuspendedRowsCount = 1,
                ErrorRowsCount = 0,
                ReadyRowsCount = 0,
            };
            Assert.AreEqual(
                ItemListStatus.Incomplete,
                list.Status,
                "ItemList status should be Incomplete");
        }

        [TestMethod]
        public void OneSuspendedRowsStatus()
        {
            var list = new ItemList
            {
                ItemListRowsCount = 2,
                CompletedRowsCount = 1,
                NewRowsCount = 0,
                ExecutingRowsCount = 0,
                WaitingRowsCount = 0,
                IncompleteRowsCount = 0,
                SuspendedRowsCount = 1,
                ErrorRowsCount = 0,
                ReadyRowsCount = 0,
            };
            Assert.AreEqual(
                ItemListStatus.Suspended,
                list.Status,
                "ItemList status should be Suspended");
        }

        #endregion
    }
}

using System.Collections.Generic;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public class ItemTest
    {
        #region Methods

        [TestMethod]
        public void NegativeFields()
        {
            var item = new Item();

            Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => item.AverageWeight = -1);
            Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => item.FifoTimePick = -1);
            Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => item.FifoTimeStore = -1);
            Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => item.Height = -1);
            Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => item.InventoryTolerance = -1);
            Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => item.Length = -1);
            Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => item.PickTolerance = -1);
            Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => item.ReorderQuantity = -1);
            Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => item.StoreTolerance = -1);
            Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => item.TotalReservedForPick = -1);
            Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => item.TotalReservedToStore = -1);
            Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => item.TotalStock = -1);
            Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => item.Width = -1);
        }

        #endregion Methods
    }
}

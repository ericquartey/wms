using System.Collections.Generic;
using Ferretto.WMS.Data.WebAPI.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public class ItemTest
    {
        #region Methods

        [TestMethod]
        public void AbcClass_NotSpecified()
        {
            var item = new Item
            {
                AbcClasses = null,
                AbcClassId = "A"
            };

            Assert.IsNull(item.AbcClassDescription);
        }

        [TestMethod]
        public void AbcClass_Specified()
        {
            var item = new Item
            {
                AbcClasses = new List<AbcClass>
                {
                    new AbcClass { Id = "A", Description = "A Class" },
                    new AbcClass { Id = "B", Description = "B Class" },
                    new AbcClass { Id = "C", Description = "C Class" }
                },
                AbcClassId = "C"
            };

            Assert.AreEqual("C Class", item.AbcClassDescription);

            item.AbcClassId = "B";

            Assert.AreEqual("B Class", item.AbcClassDescription);
        }

        [TestMethod]
        public void ItemCategory_NotSpecified()
        {
            var item = new Item
            {
                ItemCategories = null,
                ItemCategoryId = 1
            };

            Assert.IsNull(item.ItemCategoryDescription);
        }

        [TestMethod]
        public void ItemCategory_Specified()
        {
            var item = new Item
            {
                ItemCategories = new List<ItemCategory>
                {
                    new ItemCategory { Id = 1, Description = "Category 1" },
                    new ItemCategory { Id = 2, Description = "Category 2" },
                    new ItemCategory { Id = 3, Description = "Category 3" }
                },
                ItemCategoryId = 3
            };

            Assert.AreEqual("Category 3", item.ItemCategoryDescription);

            item.ItemCategoryId = 1;

            Assert.AreEqual("Category 1", item.ItemCategoryDescription);
        }

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

        [TestMethod]
        public void TotalAvailable_NoReserved()
        {
            var item = new Item
            {
                TotalStock = 10,
                TotalReservedForPick = 0,
                TotalReservedToStore = 0
            };

            Assert.AreEqual(10, item.TotalAvailable);
        }

        [TestMethod]
        public void TotalAvailable_ReservedForPick()
        {
            var item = new Item
            {
                TotalStock = 10,
                TotalReservedForPick = 1,
                TotalReservedToStore = 0
            };

            Assert.AreEqual(9, item.TotalAvailable);
        }

        [TestMethod]
        public void TotalAvailable_ReservedForPickAndStore()
        {
            var item = new Item
            {
                TotalStock = 10,
                TotalReservedForPick = 9,
                TotalReservedToStore = 1
            };

            Assert.AreEqual(2, item.TotalAvailable);
        }

        [TestMethod]
        public void TotalAvailable_ReservedForStore()
        {
            var item = new Item
            {
                TotalStock = 10,
                TotalReservedForPick = 0,
                TotalReservedToStore = 1
            };

            Assert.AreEqual(11, item.TotalAvailable);
        }

        #endregion
    }
}

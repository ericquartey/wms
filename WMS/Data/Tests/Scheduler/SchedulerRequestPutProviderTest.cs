using System;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Scheduler.Tests
{
    [TestClass]
    public class SchedulerRequestPutProviderTest : BaseWarehouseTest
    {
        #region Methods

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN an item with type FIFO \
            AND three compartments with different FIFO start time \
            WHEN a new put request for that item is made, without specific selection parameters (eg. Sub1, Lot, etc.) \
            AND there are not enough compartments with FIFO start time that is newer than the item's FIFO time \
            THEN the request is rejected \
            WHEN a new put request for that item is made, without specific selection parameters (eg. Sub1, Lot, etc.) \
            AND there are enough compartments with FIFO start time that is newer than the item's FIFO time \
            THEN the request is accepted")]
        [DataRow(5, true)]
        [DataRow(2, true)]
        [DataRow(1, false)]
        public async Task FullyQualifyPutItemByFifoBase(int fifoTime, bool expectedSuccess)
        {
            #region Arrange

            var item1 = new Common.DataModels.Item
            {
                Id = 10,
                FifoTimePut = fifoTime,
                ManagementType = Common.DataModels.ItemManagementType.FIFO
            };
            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = compartmentType1.Id,
                ItemId = item1.Id,
                MaxCapacity = 10
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = compartmentType1.Id,
                Stock = 3,
                FifoStartDate = DateTime.Today.AddDays(-3),
            };
            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = compartmentType1.Id,
                Stock = 5,
                FifoStartDate = DateTime.Today.AddDays(-2),
            };
            var compartment3 = new Common.DataModels.Compartment
            {
                Id = 3,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = compartmentType1.Id,
                Stock = 5,
                FifoStartDate = DateTime.Today.AddDays(-1),
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.Compartments.Add(compartment3);
                context.SaveChanges();
            }

            var schedulerRequestPutProvider = this.GetService<ISchedulerRequestPutProvider>();

            var itemPutOptions = new ItemOptions
            {
                AreaId = 1,
                RequestedQuantity = 5,
            };

            #endregion

            #region Act

            var result = await schedulerRequestPutProvider.FullyQualifyPutRequestAsync(item1.Id, itemPutOptions);

            #endregion

            #region Assert

            Assert.AreEqual(expectedSuccess, result.Success);

            #endregion
        }

        [TestMethod]
        [DataRow(1, true)]
        [DataRow(3, false)]
        public async Task FullyQualifyPutItemByFifoWithDifferentCompartmentAges(int compartmentAge, bool expectedSuccess)
        {
            #region Arrange

            var item1 = new Common.DataModels.Item
            {
                Id = 10,
                FifoTimePut = 2,
                ManagementType = Common.DataModels.ItemManagementType.FIFO
            };
            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = compartmentType1.Id,
                ItemId = item1.Id,
                MaxCapacity = 10
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = compartmentType1.Id,
                Stock = 1,
                FifoStartDate = DateTime.Today.AddDays(-1 * compartmentAge),
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.Compartments.Add(compartment1);
                context.SaveChanges();
            }

            var schedulerRequestPutProvider = this.GetService<ISchedulerRequestPutProvider>();

            var itemPutOptions = new ItemOptions
            {
                AreaId = 1,
                RequestedQuantity = 5,
            };

            #endregion

            #region Act

            var result = await schedulerRequestPutProvider.FullyQualifyPutRequestAsync(item1.Id, itemPutOptions);

            #endregion

            #region Assert

            Assert.AreEqual(expectedSuccess, result.Success);

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN an item with FIFO management type \
            WHEN a new put request for that item is made, with specific selection parameters (i.e. Sub1 and/or Sub2), \
            AND the sum of the available space in the selected compartment set is greater than the requested quantity, \
            THEN the request is accepted \
            WHEN a new put request for that item is made, with specific selection parameters (i.e. Sub1 and/or Sub2), \
            AND the sum of the available space in the selected compartment set is lower than the requested quantity, \
            THEN the request is rejected")]
        [DataRow(5, "S1", "S2", true)]
        [DataRow(5, null, "S2", true)]
        [DataRow(7, "S1", "S2", false)]
        public async Task FullyQualifyPutItemByFifoAdvanced(int requestedQuantity, string s1, string s2, bool expectedSuccess)
        {
            #region Arrange

            var item1 = new Common.DataModels.Item
            {
                Id = 10,
                FifoTimePut = 3,
                ManagementType = Common.DataModels.ItemManagementType.FIFO
            };
            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = compartmentType1.Id,
                ItemId = item1.Id,
                MaxCapacity = 10
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = compartmentType1.Id,
                Stock = 3,
                Sub1 = "S1",
                Sub2 = "S2",
                FifoStartDate = DateTime.Today.AddDays(-3),
            };
            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = compartmentType1.Id,
                Stock = 5,
                Sub1 = "S1",
                Sub2 = "S2",
                FifoStartDate = DateTime.Today.AddDays(-2),
            };
            var compartment3 = new Common.DataModels.Compartment
            {
                Id = 3,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = compartmentType1.Id,
                Stock = 7,
                Sub2 = "S2",
                FifoStartDate = DateTime.Today.AddDays(-1),
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.Compartments.Add(compartment3);
                context.SaveChanges();
            }

            var schedulerRequestPutProvider = this.GetService<ISchedulerRequestPutProvider>();

            var itemPutOptions1 = new ItemOptions
            {
                AreaId = 1,
                RequestedQuantity = requestedQuantity,
                Sub1 = s1,
                Sub2 = s2,
            };

            #endregion

            #region Act

            var result = await schedulerRequestPutProvider.FullyQualifyPutRequestAsync(
                item1.Id, itemPutOptions1);

            #endregion

            #region Assert

            Assert.AreEqual(expectedSuccess, result.Success);

            #endregion
        }

        [TestMethod]
        public async Task FullyQualifyPutItemByVolumeAcceptPairedCompartment()
        {
            #region Arrange

            var otherItem = new Common.DataModels.Item
            {
                Id = 10,
                ManagementType = Common.DataModels.ItemManagementType.Volume
            };
            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                CompartmentTypeId = compartmentType1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                ItemId = this.ItemVolume.Id,
                IsItemPairingFixed = true,
                Stock = 0,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = compartmentType1.Id,
                ItemId = this.ItemVolume.Id,
                MaxCapacity = 10
            };
            var itemCompartmentType2 = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = compartmentType1.Id,
                ItemId = otherItem.Id,
                MaxCapacity = 10
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(otherItem);
                context.CompartmentTypes.Add(compartmentType1);
                context.Compartments.Add(compartment1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType2);

                context.SaveChanges();
            }

            var schedulerRequestPutProvider = this.GetService<ISchedulerRequestPutProvider>();

            var itemPutOptions1 = new ItemOptions
            {
                AreaId = 1,
                RequestedQuantity = 5,
            };

            #endregion

            #region Act

            var result = await schedulerRequestPutProvider.FullyQualifyPutRequestAsync(
                this.ItemVolume.Id, itemPutOptions1);

            #endregion

            #region Assert

            Assert.IsTrue(
                result.Success,
                "This request should be accepted because the compartment is already paired with the same item");

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN an item with type Volume \
            AND three compartments (1 full, 1 empty and 1 half full) \
            WHEN a new put request for that item is made \
            AND the requested quantity is higher than the available space \
            THEN the request is rejected \
            WHEN a new put request for that item is made \
            AND the requested quantity is lower than the available space \
            THEN the request is accepted")]
        [DataRow(5, true)]
        [DataRow(15, true)]
        [DataRow(25, false)]
        public async Task FullyQualifyPutItemByVolumeBase(int requestedQuantity, bool expectedSuccess)
        {
            #region Arrange

            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 0,
                CompartmentTypeId = compartmentType1.Id,
            };
            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 5,
                CompartmentTypeId = compartmentType1.Id,
            };
            var compartment3 = new Common.DataModels.Compartment
            {
                Id = 3,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
                CompartmentTypeId = compartmentType1.Id,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = compartmentType1.Id,
                ItemId = this.ItemVolume.Id,
                MaxCapacity = 10
            };

            using (var context = this.CreateContext())
            {
                context.CompartmentTypes.Add(compartmentType1);
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.Compartments.Add(compartment3);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);

                context.SaveChanges();
            }

            var schedulerRequestPutProvider = this.GetService<ISchedulerRequestPutProvider>();

            var itemPutOptions1 = new ItemOptions
            {
                AreaId = 1,
                RequestedQuantity = requestedQuantity,
            };

            #endregion

            #region Act

            var result = await schedulerRequestPutProvider.FullyQualifyPutRequestAsync(
                this.ItemVolume.Id, itemPutOptions1);

            #endregion

            #region Assert

            Assert.AreEqual(expectedSuccess, result.Success);
            if (result.Success)
            {
                Assert.AreEqual(requestedQuantity, result.Entity.RequestedQuantity);
            }

            #endregion
        }

        [TestMethod]
        public async Task FullyQualifyPutItemByVolumeAdvanced()
        {
            #region Arrange

            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 1,
                CompartmentTypeId = compartmentType1.Id,
            };
            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 1,
                Sub1 = "Sub1",
                Sub2 = "Sub2",
                CompartmentTypeId = compartmentType1.Id,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = compartmentType1.Id,
                ItemId = this.ItemVolume.Id,
                MaxCapacity = 10
            };

            using (var context = this.CreateContext())
            {
                context.CompartmentTypes.Add(compartmentType1);
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);

                context.SaveChanges();
            }

            var schedulerRequestPutProvider = this.GetService<ISchedulerRequestPutProvider>();

            var itemPutOptions1 = new ItemOptions
            {
                AreaId = 1,
                RequestedQuantity = 5,
                Sub1 = "Sub1",
                Sub2 = "Sub2",
            };

            #endregion

            #region Act

            var result = await schedulerRequestPutProvider.FullyQualifyPutRequestAsync(
                this.ItemVolume.Id, itemPutOptions1);

            #endregion

            #region Assert

            Assert.IsTrue(
                result.Success,
                "This request should be accepted because we have enough free space");

            Assert.AreEqual(
                itemPutOptions1.Sub1,
                result.Entity.Sub1,
                "Selected advanced parameters should be same as requested");
            Assert.AreEqual(
                itemPutOptions1.Sub2,
                result.Entity.Sub2,
                "Selected advanced parameters should be same as requested");

            #endregion
        }

        [TestMethod]
        public async Task FullyQualifyPutItemByVolumeDenyPairedCompartment()
        {
            #region Arrange

            var otherItem = new Common.DataModels.Item
            {
                Id = 10,
                ManagementType = Common.DataModels.ItemManagementType.Volume
            };
            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                CompartmentTypeId = compartmentType1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                ItemId = otherItem.Id,
                IsItemPairingFixed = true,
                Stock = 0,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = compartmentType1.Id,
                ItemId = this.ItemVolume.Id,
                MaxCapacity = 10
            };
            var itemCompartmentType2 = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = compartmentType1.Id,
                ItemId = otherItem.Id,
                MaxCapacity = 10
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(otherItem);
                context.CompartmentTypes.Add(compartmentType1);
                context.Compartments.Add(compartment1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType2);

                context.SaveChanges();
            }

            var schedulerRequestPutProvider = this.GetService<ISchedulerRequestPutProvider>();

            var itemPutOptions1 = new ItemOptions
            {
                AreaId = 1,
                RequestedQuantity = 5,
            };

            #endregion

            #region Act

            var result = await schedulerRequestPutProvider.FullyQualifyPutRequestAsync(
                this.ItemVolume.Id, itemPutOptions1);

            #endregion

            #region Assert

            Assert.IsFalse(
                result.Success,
                "This request should be rejected because the compartment is already paired with another item");

            #endregion
        }

        [TestMethod]
        [DataRow(15, false)]
        [DataRow(5, true)]
        public async Task FullyQualifyPutItemByVolumeOnPaired(int requestedQuantity, bool expectedSuccess)
        {
            #region Arrange

            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                CompartmentTypeId = compartmentType1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                ItemId = this.ItemVolume.Id,
                IsItemPairingFixed = true,
                Stock = 0,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = compartmentType1.Id,
                ItemId = this.ItemVolume.Id,
                MaxCapacity = 10
            };

            using (var context = this.CreateContext())
            {
                context.CompartmentTypes.Add(compartmentType1);
                context.Compartments.Add(compartment1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);

                context.SaveChanges();
            }

            var schedulerRequestPutProvider = this.GetService<ISchedulerRequestPutProvider>();

            var itemPutOptions1 = new ItemOptions
            {
                AreaId = 1,
                RequestedQuantity = requestedQuantity,
            };

            #endregion

            #region Act

            var result = await schedulerRequestPutProvider.FullyQualifyPutRequestAsync(
                this.ItemVolume.Id, itemPutOptions1);

            #endregion

            #region Assert

            Assert.AreEqual(expectedSuccess, result.Success);

            #endregion
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Scheduler.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.Tests.Scheduler
{
    [TestClass]
    public class RequestForPutItemTest : BaseWarehouseTest
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
        [DataRow(5)]
        [DataRow(2)]
        [DataRow(1)]
        public async Task FullyQualifyPutItemByFifo(int fifoTime)
        {
            #region Arrange

            var item1 = new Common.DataModels.Item
            {
                Id = 10,
                FifoTimePut = fifoTime,
                ManagementType = Common.DataModels.ItemManagementType.FIFO
            };
            var itemComparmentType1 = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = 1,
                ItemId = item1.Id,
                MaxCapacity = 10
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = itemComparmentType1.CompartmentTypeId,
                Stock = 3,
                FifoStartDate = DateTime.Today.AddDays(-3),
            };
            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = itemComparmentType1.CompartmentTypeId,
                Stock = 5,
                FifoStartDate = DateTime.Today.AddDays(-2),
            };
            var compartment3 = new Common.DataModels.Compartment
            {
                Id = 3,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = itemComparmentType1.CompartmentTypeId,
                Stock = 5,
                FifoStartDate = DateTime.Today.AddDays(-1),
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.ItemsCompartmentTypes.Add(itemComparmentType1);
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

            var result = await schedulerRequestPutProvider.FullyQualifyPutRequestAsync(item1.Id, itemPutOptions, null);
            var itemSchedulerRequest = result.Entity;

            #endregion

            #region Assert

            if (result.Success)
            {
                Assert.IsNotNull(itemSchedulerRequest);
            }
            else
            {
                Assert.IsNull(itemSchedulerRequest);
            }

            switch (fifoTime)
            {
                case 1:

                    Assert.IsNull(itemSchedulerRequest);
                    break;

                case 2:
                case 5:
                    Assert.IsNotNull(itemSchedulerRequest);
                    break;
            }

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
        [DataRow(5)]
        [DataRow(15)]
        [DataRow(25)]
        public async Task FullyQualifyPutItemByVolume(int requestedQuantity)
        {
            #region Arrange

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 0,
                CompartmentTypeId = 1,
            };
            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 5,
                CompartmentTypeId = 1,
            };
            var compartment3 = new Common.DataModels.Compartment
            {
                Id = 3,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
                CompartmentTypeId = 1,
            };
            var itemComparmentType1 = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = 1,
                ItemId = this.ItemVolume.Id,
                MaxCapacity = 10
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.Compartments.Add(compartment3);
                context.ItemsCompartmentTypes.Add(itemComparmentType1);

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

            // Test Put One Item -> One Shot
            var result = await schedulerRequestPutProvider.FullyQualifyPutRequestAsync(
                this.ItemVolume.Id, itemPutOptions1, null);

            #endregion

            #region Assert

            var itemSchedulerRequest = result.Entity;

            if (result.Success)
            {
                // test RequestedQuantity is minor or major maxcapacity of associated compartments
                var stockTotal = compartment1.Stock + compartment2.Stock + compartment3.Stock;
                var maxCapacityTotal = itemComparmentType1.MaxCapacity * 3;
                var availableSpace = maxCapacityTotal - stockTotal;

                if (requestedQuantity <= availableSpace)
                {
                    Assert.IsNotNull(itemSchedulerRequest);
                    Assert.AreEqual(itemPutOptions1.RequestedQuantity, itemSchedulerRequest.RequestedQuantity);
                }
                else
                {
                    // if full, it return failed
                    Assert.IsNull(itemSchedulerRequest);
                }
            }
            else
            {
                // if full, it return failed
                Assert.IsNull(itemSchedulerRequest);
            }

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
        [DataRow(5, "S1", "S2")]
        [DataRow(5, null, "S2")]
        [DataRow(7, "S1", "S2")]
        public async Task FullyQualifyPutItemWithUserInput(int requestedQuantity, string s1, string s2)
        {
            #region Arrange

            var item1 = new Common.DataModels.Item
            {
                Id = 10,
                FifoTimePut = 3,
                ManagementType = Common.DataModels.ItemManagementType.FIFO
            };
            var itemComparmentType1 = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = 1,
                ItemId = item1.Id,
                MaxCapacity = 10
            };
            var sub1 = "S1";
            var sub2 = "S2";

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = itemComparmentType1.CompartmentTypeId,
                Stock = 3,
                Sub1 = sub1,
                Sub2 = sub2,
                FifoStartDate = DateTime.Today.AddDays(-3),
            };
            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = itemComparmentType1.CompartmentTypeId,
                Stock = 5,
                Sub1 = sub1,
                Sub2 = sub2,
                FifoStartDate = DateTime.Today.AddDays(-2),
            };
            var compartment3 = new Common.DataModels.Compartment
            {
                Id = 3,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = itemComparmentType1.CompartmentTypeId,
                Stock = 7,
                Sub2 = sub2,
                FifoStartDate = DateTime.Today.AddDays(-1),
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.ItemsCompartmentTypes.Add(itemComparmentType1);
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
                Sub1 = sub1,
                Sub2 = sub2,
            };

            #endregion

            #region Act

            var result = await schedulerRequestPutProvider.FullyQualifyPutRequestAsync(
                item1.Id, itemPutOptions1, null);
            var itemSchedulerRequest = result.Entity;

            #endregion

            #region Assert

            if (result.Success)
            {
                Assert.IsNotNull(itemSchedulerRequest);
            }
            else
            {
                Assert.IsNull(itemSchedulerRequest);
            }

            if (requestedQuantity == 5)
            {
                Assert.IsNotNull(itemSchedulerRequest);
            }
            else if (requestedQuantity == 7)
            {
                Assert.IsNull(itemSchedulerRequest);
            }

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

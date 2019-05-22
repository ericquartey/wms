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
            @"GIVEN a item with type Volume to put in loading unit \
               WHEN a new request for that item is made, without specific selection parameters (eg. Sub1, Lot, etc.) \
               THEN the new request should be applied the criteria by Volume to find right compartments")]
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
                LoadingUnitId = this.LoadingUnit1.Id,
                CompartmentTypeId = itemComparmentType1.CompartmentTypeId,
                Stock = 5,
                FifoStartDate = DateTime.Today.AddDays(-3),
            };
            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1.Id,
                CompartmentTypeId = itemComparmentType1.CompartmentTypeId,
                Stock = 5,
                FifoStartDate = DateTime.Today.AddDays(-2),
            };
            var compartment3 = new Common.DataModels.Compartment
            {
                Id = 3,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1.Id,
                CompartmentTypeId = itemComparmentType1.CompartmentTypeId,
                Stock = 5,
                FifoStartDate = DateTime.Today.AddDays(-1),
            };

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act

                context.Items.Add(item1);
                context.ItemsCompartmentTypes.Add(itemComparmentType1);
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.Compartments.Add(compartment3);
                context.SaveChanges();

                var schedulerRequestPutProvider = this.GetService<ISchedulerRequestPutProvider>();

                var itemPutOptions = new ItemOptions
                {
                    AreaId = 1,
                    RequestedQuantity = 1,
                };
                var itemSchedulerRequest = await schedulerRequestPutProvider.FullyQualifyPutRequestAsync(item1.Id, itemPutOptions, null);

                #endregion

                #region Assert

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
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN a item with type FIFO to put in loading unit \
               WHEN a new request for that item is made, without specific selection parameters (eg. Sub1, Lot, etc.) \
               THEN the new request should be applied the criteria by FIFO and then by Volume to find right compartments")]
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
                LoadingUnitId = this.LoadingUnit1.Id,
                Stock = 0,
                CompartmentTypeId = 1,
            };
            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1.Id,
                Stock = 5,
                CompartmentTypeId = 1,
            };
            var compartment3 = new Common.DataModels.Compartment
            {
                Id = 3,
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1.Id,
                Stock = 10,
                CompartmentTypeId = 1,
            };
            var itemComparmentType1 = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = 1,
                ItemId = this.ItemVolume.Id,
                MaxCapacity = 10
            };

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act

                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.Compartments.Add(compartment3);
                context.ItemsCompartmentTypes.Add(itemComparmentType1);

                context.SaveChanges();

                var schedulerRequestPutProvider = this.GetService<ISchedulerRequestPutProvider>();

                var itemPutOptions1 = new ItemOptions
                {
                    AreaId = 1,
                    RequestedQuantity = requestedQuantity,
                };

                // Test Put One Item -> One Shot
                var itemSchedulerRequest = await schedulerRequestPutProvider.FullyQualifyPutRequestAsync(
                    this.ItemVolume.Id, itemPutOptions1, null);

                // test RequestedQuantity is minor or major maxcapacity of associated compartments
                var stockTotal = compartment1.Stock + compartment2.Stock + compartment3.Stock;
                var maxCapacityTotal = itemComparmentType1.MaxCapacity * 3;
                var availableSpace = maxCapacityTotal - stockTotal;

                #endregion

                #region Assert

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

                #endregion
            }
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN a item to put in loading unit \
               WHEN a new request for that item is made, with specific selection parameters (eg. Sub1, Lot, etc.) \
               THEN the new request should be elaborated and the filters are be applied.")]
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
                LoadingUnitId = this.LoadingUnit1.Id,
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
                LoadingUnitId = this.LoadingUnit1.Id,
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
                LoadingUnitId = this.LoadingUnit1.Id,
                CompartmentTypeId = itemComparmentType1.CompartmentTypeId,
                Stock = 7,
                Sub2 = sub2,
                FifoStartDate = DateTime.Today.AddDays(-1),
            };

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act

                context.Items.Add(item1);
                context.ItemsCompartmentTypes.Add(itemComparmentType1);
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.Compartments.Add(compartment3);
                context.SaveChanges();

                var schedulerRequestPutProvider = this.GetService<ISchedulerRequestPutProvider>();
                var itemPutOptions1 = new ItemOptions
                {
                    AreaId = 1,
                    RequestedQuantity = requestedQuantity,
                    Sub1 = sub1,
                    Sub2 = sub2,
                };
                var itemSchedulerRequest = await schedulerRequestPutProvider.FullyQualifyPutRequestAsync(
                    item1.Id, itemPutOptions1, null);

                #endregion

                #region Assert

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
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        #endregion
    }
}

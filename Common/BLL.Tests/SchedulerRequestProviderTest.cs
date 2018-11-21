using System.Threading.Tasks;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.DataModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.Common.BLL.Tests
{
    [TestClass]
    public class SchedulerRequestProviderTest
    {
        #region Fields

        private Aisle aisle1;
        private Area area1;
        private Cell cell1;
        private Item item1;
        private Item itemFifo;
        private Item itemVolume;
        private LoadingUnit loadingUnit1;

        #endregion Fields

        #region Methods

        [TestCleanup]
        public void Cleanup()
        {
            using (var context = this.CreateContext())
            {
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        [TestProperty("Description",
          @"GIVEN two compartments with same Sub1, but in different areas \
                AND a requests allocated to the first area, so that there is no availability on that area \
               WHEN a new request for the first area is made \
               THEN the new request should be accepted")]
        public async Task CompartmentsInDifferentAreasTest()
        {
            #region Arrange

            var sub1 = "S1";

            var compartment1 = new Compartment
            {
                Id = 1,
                Code = "Compartment #1",
                ItemId = this.item1.Id,
                Sub1 = sub1,
                LoadingUnitId = this.loadingUnit1.Id,
                Stock = 10,
            };

            var compartment2 = new Compartment
            {
                Id = 2,
                Code = "Compartment #2",
                ItemId = this.item1.Id,
                Sub1 = sub1,
                LoadingUnitId = 2,
                Stock = 10,
            };

            var request1 = new SchedulerRequest
            {
                Id = 1,
                ItemId = this.item1.Id,
                Sub1 = sub1,
                RequestedQuantity = compartment1.Stock,
                OperationType = OperationType.Withdrawal
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.SchedulerRequests.Add(request1);

                context.SaveChanges();
            }

            #endregion Arrange

            using (var context = this.CreateContext())
            {
                #region Act

                var provider = new SchedulerRequestProvider(context);

                var schedulerRequest = new BusinessModels.SchedulerRequest
                {
                    ItemId = this.item1.Id,
                    AreaId = this.area1.Id,
                    RequestedQuantity = 1,
                    Type = BusinessModels.OperationType.Withdrawal
                };

                var acceptedRequest = await provider.FullyQualifyWithdrawalRequest(schedulerRequest);

                #endregion Act

                #region Assert

                Assert.IsNull(acceptedRequest);

                #endregion Assert
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            this.area1 = new Area { Id = 1, Name = "Area #1" };
            this.aisle1 = new Aisle { Id = 1, AreaId = this.area1.Id, Name = "Aisle #1" };
            this.cell1 = new Cell { Id = 1, AisleId = this.aisle1.Id };
            this.loadingUnit1 = new LoadingUnit { Id = 1, Code = "Loading Unit #1", CellId = this.cell1.Id };
            this.item1 = new Item { Id = 1, Code = "Item #1" };
            this.itemFifo = new Item { Id = 2, Code = "Item #2", ManagementType = ItemManagementType.FIFO };
            this.itemVolume = new Item { Id = 3, Code = "Item #3", ManagementType = ItemManagementType.Volume };

            using (var context = this.CreateContext())
            {
                context.Areas.Add(this.area1);
                context.Aisles.Add(this.aisle1);
                context.Cells.Add(this.cell1);
                context.LoadingUnits.Add(this.loadingUnit1);
                context.Items.Add(this.item1);
                context.Items.Add(this.itemFifo);
                context.Items.Add(this.itemVolume);

                context.SaveChanges();
            }
        }

        [TestMethod]
        [TestProperty("Description",
            @"GIVEN a compartment with Sub1 \
                AND no other requests are present \
               WHEN a new request for no particular Sub1 is made \
               THEN the new request should be accepted")]
        public async Task SingleCompartmentWithNoRequestsTest()
        {
            #region Arrange

            var compartment1 = new Compartment
            {
                Id = 1,
                Code = "Compartment #1",
                ItemId = this.item1.Id,
                Sub1 = "S1",
                LoadingUnitId = this.loadingUnit1.Id,
                Stock = 10,
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);

                context.SaveChanges();
            }

            #endregion Arrange

            using (var context = this.CreateContext())
            {
                #region Act

                var provider = new SchedulerRequestProvider(context);

                var schedulerRequest = new BusinessModels.SchedulerRequest
                {
                    ItemId = this.item1.Id,
                    AreaId = this.area1.Id,
                    RequestedQuantity = 1,
                    Type = BusinessModels.OperationType.Withdrawal
                };

                var acceptedRequest = await provider.FullyQualifyWithdrawalRequest(schedulerRequest);

                #endregion Act

                #region Assert

                Assert.IsNotNull(acceptedRequest);
                Assert.AreSame(compartment1.Sub1, acceptedRequest.Sub1);

                #endregion Assert
            }
        }

        [TestMethod]
        [TestProperty("Description",
            @"GIVEN a compartment with Sub1 \
                AND a request that uses part of the compartment's availability \
               WHEN a new request for the same Sub1 is made for the remaining availability \
               THEN the new request should be accepted")]
        public async Task SingleCompartmentWithOneRequestTest()
        {
            #region Arrange

            var sub1 = "S1";

            var compartment1 = new Compartment
            {
                Id = 1,
                Code = "Compartment #1",
                ItemId = this.item1.Id,
                Sub1 = sub1,
                LoadingUnitId = this.loadingUnit1.Id,
                Stock = 10,
            };

            var request1 = new SchedulerRequest
            {
                Id = 1,
                ItemId = this.item1.Id,
                Sub1 = sub1,
                RequestedQuantity = 3,
                OperationType = OperationType.Withdrawal
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.SchedulerRequests.Add(request1);

                context.SaveChanges();
            }

            #endregion Arrange

            using (var context = this.CreateContext())
            {
                #region Act

                var provider = new SchedulerRequestProvider(context);

                var schedulerRequest = new BusinessModels.SchedulerRequest
                {
                    ItemId = this.item1.Id,
                    AreaId = this.area1.Id,
                    RequestedQuantity = compartment1.Stock - request1.RequestedQuantity,
                    Type = BusinessModels.OperationType.Withdrawal
                };

                var acceptedRequest = await provider.FullyQualifyWithdrawalRequest(schedulerRequest);

                #endregion Act

                #region Assert

                Assert.IsNotNull(acceptedRequest);
                Assert.AreSame(compartment1.Sub1, acceptedRequest.Sub1);

                #endregion Assert
            }
        }

        [TestMethod]
        [TestProperty("Description",
            @"GIVEN a compartment with Sub1 \
                AND two requests allocated so that the compartment has no availability \
               WHEN a new request for the same Sub1 is made \
               THEN the new request should be rejected")]
        public async Task SingleCompartmentWithTwoRequestsAndNoAvailabilityTest()
        {
            #region Arrange

            var sub1 = "S1";

            var compartment1 = new Compartment
            {
                Id = 1,
                Code = "Compartment #1",
                ItemId = this.item1.Id,
                Sub1 = sub1,
                LoadingUnitId = this.loadingUnit1.Id,
                Stock = 10,
            };

            var request1 = new SchedulerRequest
            {
                Id = 1,
                ItemId = this.item1.Id,
                Sub1 = sub1,
                RequestedQuantity = compartment1.Stock / 2,
                OperationType = OperationType.Withdrawal
            };

            var request2 = new SchedulerRequest
            {
                Id = 2,
                ItemId = this.item1.Id,
                Sub1 = sub1,
                RequestedQuantity = compartment1.Stock - request1.RequestedQuantity,
                OperationType = OperationType.Withdrawal
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.SchedulerRequests.Add(request1);
                context.SchedulerRequests.Add(request2);

                context.SaveChanges();
            }

            #endregion Arrange

            using (var context = this.CreateContext())
            {
                #region Act

                var provider = new SchedulerRequestProvider(context);

                var schedulerRequest = new BusinessModels.SchedulerRequest
                {
                    ItemId = this.item1.Id,
                    AreaId = this.area1.Id,
                    RequestedQuantity = 1,
                    Type = BusinessModels.OperationType.Withdrawal
                };

                var acceptedRequest = await provider.FullyQualifyWithdrawalRequest(schedulerRequest);

                #endregion Act

                #region Assert

                Assert.IsNull(acceptedRequest);

                #endregion Assert
            }
        }

        [TestMethod]
        [TestProperty("Description",
            @"GIVEN two compartments with same Sub1 \
                AND two requests allocated so that there is still availability \
               WHEN a new request for the same Sub1 is made \
               THEN the new request should be accepted")]
        public async Task TwoCompartmentsWithTwoRequestsAndNoAvailabilityOnOneCompartmentTest()
        {
            #region Arrange

            var sub1 = "S1";

            var compartment1 = new Compartment
            {
                Id = 1,
                Code = "Compartment #1",
                ItemId = this.item1.Id,
                Sub1 = sub1,
                LoadingUnitId = this.loadingUnit1.Id,
                Stock = 10,
            };

            var compartment2 = new Compartment
            {
                Id = 2,
                Code = "Compartment #2",
                ItemId = this.item1.Id,
                Sub1 = sub1,
                LoadingUnitId = this.loadingUnit1.Id,
                Stock = 10,
            };

            var request1 = new SchedulerRequest
            {
                Id = 1,
                ItemId = this.item1.Id,
                Sub1 = sub1,
                RequestedQuantity = compartment1.Stock,
                OperationType = OperationType.Withdrawal
            };

            var request2 = new SchedulerRequest
            {
                Id = 2,
                ItemId = this.item1.Id,
                Sub1 = sub1,
                RequestedQuantity = compartment2.Stock / 2,
                OperationType = OperationType.Withdrawal
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.SchedulerRequests.Add(request1);
                context.SchedulerRequests.Add(request2);

                context.SaveChanges();
            }

            #endregion Arrange

            using (var context = this.CreateContext())
            {
                #region Act

                var provider = new SchedulerRequestProvider(context);

                var schedulerRequest = new BusinessModels.SchedulerRequest
                {
                    ItemId = this.item1.Id,
                    AreaId = this.area1.Id,
                    RequestedQuantity = compartment2.Stock - request2.RequestedQuantity,
                    Type = BusinessModels.OperationType.Withdrawal
                };

                var acceptedRequest = await provider.FullyQualifyWithdrawalRequest(schedulerRequest);

                #endregion Act

                #region Assert

                Assert.IsNotNull(acceptedRequest);
                Assert.AreSame(compartment2.Sub1, acceptedRequest.Sub1);

                #endregion Assert
            }
        }

        private DatabaseContext CreateContext()
        {
            return new DatabaseContext(
                new DbContextOptionsBuilder<DatabaseContext>()
                    .UseInMemoryDatabase(databaseName: "test_database")
                    .Options
                );
        }

        #endregion Methods
    }
}

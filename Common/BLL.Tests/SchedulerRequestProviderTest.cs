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

        private const int OtherBayId = 1000;
        private const int OtherLoadingUnitId = 1000;
        private Aisle aisle1;
        private Area area1;
        private Bay bay1;
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
            @"GIVEN a compartment in a specific area, associated to a specific item \
                AND a withdrawal request for the given item on a bay of the specified area \
               WHEN a new request for the same item and area is made \
               THEN the new request should be accepted")]
        public async Task CompartmentsInBay()
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
                    BayId = this.bay1.Id,
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
          @"GIVEN two compartments with same Sub1, but in different areas \
                AND a requests allocated to the first area, so that there is no availability on that area \
               WHEN a new request for the first area is made \
               THEN the new request should be rejected")]
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
                LoadingUnitId = OtherLoadingUnitId,
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

        [TestMethod]
        [TestProperty("Description",
         @"GIVEN a compartment in a specific area and aisle \
               WHEN a new request is made for another area that has no compatible compartments \
               THEN the new request should be rejected")]
        public async Task CompartmentsNotInBay()
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
                    BayId = OtherBayId,
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
            @"GIVEN a compartment associated to an item \
               WHEN a new request for that item is made, without specific selection parameters (eg. Sub1, Lot, etc.) \
               THEN the new request should be accepted and all the request parameters should be populated")]
        public async Task FullRequestQualificationTest()
        {
            #region Arrange

            var compartment1 = new Compartment
            {
                Id = 1,
                Code = "Compartment #1",
                ItemId = this.item1.Id,
                Sub1 = "S1",
                Sub2 = "S2",
                Lot = "Lot1",
                PackageTypeId = 1,
                RegistrationNumber = "RegistrationNumber1",
                MaterialStatusId = 1,
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
                    IsInstant = true,
                    Type = BusinessModels.OperationType.Withdrawal
                };

                var acceptedRequest = await provider.FullyQualifyWithdrawalRequest(schedulerRequest);

                #endregion Act

                #region Assert

                Assert.IsNotNull(acceptedRequest);
                Assert.IsTrue(acceptedRequest.IsInstant);
                Assert.AreSame(compartment1.Sub1, acceptedRequest.Sub1);
                Assert.AreSame(compartment1.Sub2, acceptedRequest.Sub2);
                Assert.AreSame(compartment1.Lot, acceptedRequest.Lot);
                Assert.AreEqual(compartment1.PackageTypeId.Value, acceptedRequest.PackageTypeId.Value);
                Assert.AreEqual(compartment1.MaterialStatusId.Value, acceptedRequest.MaterialStatusId.Value);
                Assert.AreSame(compartment1.RegistrationNumber, acceptedRequest.RegistrationNumber);

                #endregion Assert
            }
        }

        [TestMethod]
        [TestProperty("Description",
            @"GIVEN that a non-withdrawal request is created \
               WHEN the FullyQualifyWithdrawalRequest method is called with the invalid request\
               THEN the method should throw an exception")]
        public async Task FullyQualifyWithdrawalRequestWithInvalidArgumentTest()
        {
            #region Arrange

            var request = new BusinessModels.SchedulerRequest
            {
                Type = BusinessModels.OperationType.Insertion
            };

            #endregion Arrange

            using (var context = this.CreateContext())
            {
                #region Act + Assert

                var provider = new SchedulerRequestProvider(context);

                await Assert.ThrowsExceptionAsync<System.ArgumentException>(() => provider.FullyQualifyWithdrawalRequest(request));

                #endregion Act + Assert
            }
        }

        [TestMethod]
        [TestProperty("Description",
            @"GIVEN that a null request is created \
               WHEN the FullyQualifyWithdrawalRequest method is called with the null new request\
               THEN the method should throw an exception")]
        public async Task FullyQualifyWithdrawalRequestWithNullArgumentTest()
        {
            #region Arrange

            BusinessModels.SchedulerRequest request = null;

            #endregion Arrange

            using (var context = this.CreateContext())
            {
                #region Act + Assert

                var provider = new SchedulerRequestProvider(context);

                await Assert.ThrowsExceptionAsync<System.ArgumentNullException>(() => provider.FullyQualifyWithdrawalRequest(request));

                #endregion Act + Assert
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            this.area1 = new Area { Id = 1, Name = "Area #1" };
            this.aisle1 = new Aisle { Id = 1, AreaId = this.area1.Id, Name = "Aisle #1" };
            this.cell1 = new Cell { Id = 1, AisleId = this.aisle1.Id };
            this.loadingUnit1 = new LoadingUnit { Id = 1, Code = "Loading Unit #1", CellId = this.cell1.Id };
            this.bay1 = new Bay { Id = 1, Description = "Bay #1", AreaId = this.area1.Id };
            this.item1 = new Item { Id = 1, Code = "Item #1", ManagementType = ItemManagementType.FIFO };
            this.itemFifo = new Item { Id = 2, Code = "Item #2", ManagementType = ItemManagementType.FIFO };
            this.itemVolume = new Item { Id = 3, Code = "Item #3", ManagementType = ItemManagementType.Volume };

            using (var context = this.CreateContext())
            {
                context.Areas.Add(this.area1);
                context.Aisles.Add(this.aisle1);
                context.Bays.Add(this.bay1);
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
            @"GIVEN some compartments for a FIFO item with different Sub1's and different first store dates \
                AND no other requests are present \
               WHEN a new request for no particular Sub1 is made \
               THEN the new request should be accepted\
                AND the accepted request should select the Sub1's with oldest store date")]
        public async Task SingleCompartmentWithFifoTest()
        {
            #region Arrange

            var subX = "Sx";
            var subZ = "Sz";

            var compartments = new Compartment[]
            {
                new Compartment
                {
                    Id = 1,
                    Code = "Compartment #1",
                    ItemId = this.itemFifo.Id,
                    Sub1 = subX,
                    LoadingUnitId = this.loadingUnit1.Id,
                    Stock = 10,
                    FirstStoreDate = System.DateTime.Now.AddHours(-1)
                },
                new Compartment
                {
                    Id = 2,
                    Code = "Compartment #2",
                    ItemId = this.itemFifo.Id,
                    Sub1 = subX,
                    LoadingUnitId = this.loadingUnit1.Id,
                    Stock = 10,
                    FirstStoreDate = System.DateTime.Now.AddHours(-3)
                },
                new Compartment
                {
                    Id = 3,
                    Code = "Compartment #3",
                    ItemId = this.itemFifo.Id,
                    Sub1 = subZ,
                    LoadingUnitId = this.loadingUnit1.Id,
                    Stock = 10,
                    FirstStoreDate = System.DateTime.Now.AddHours(-2)
                },
                new Compartment
                {
                    Id = 4,
                    Code = "Compartment #4",
                    ItemId = this.itemFifo.Id,
                    Sub1 = subZ,
                    LoadingUnitId = this.loadingUnit1.Id,
                    Stock = 10,
                    FirstStoreDate = System.DateTime.Now.AddHours(-4)
                }
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.AddRange(compartments);

                context.SaveChanges();
            }

            #endregion Arrange

            using (var context = this.CreateContext())
            {
                #region Act

                var provider = new SchedulerRequestProvider(context);

                var schedulerRequest = new BusinessModels.SchedulerRequest
                {
                    ItemId = this.itemFifo.Id,
                    AreaId = this.area1.Id,
                    RequestedQuantity = 1,
                    Type = BusinessModels.OperationType.Withdrawal
                };

                var acceptedRequest = await provider.FullyQualifyWithdrawalRequest(schedulerRequest);

                #endregion Act

                #region Assert

                Assert.IsNotNull(acceptedRequest);
                Assert.AreSame(compartments[compartments.Length - 1].Sub1, acceptedRequest.Sub1);

                #endregion Assert
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
                AND two requests allocated so that the compartment has no availability \
               WHEN a new request is made \
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
                RequestedQuantity = 5,
                OperationType = OperationType.Withdrawal
            };

            var request2 = new SchedulerRequest
            {
                Id = 2,
                ItemId = this.item1.Id,
                Sub1 = sub1,
                RequestedQuantity = 5,
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
            @"GIVEN some compartments for a Volume item with different Sub1's and different stocks \
                AND no other requests are present \
               WHEN a new request for no particular Sub1 is made \
               THEN the new request should be accepted \
                AND the accepted request should select the Sub1's with less stock and oldest store date")]
        public async Task SingleCompartmentWithVolumeTest()
        {
            #region Arrange

            var subX = "Sx";
            var subZ = "Sz";
            var subY = "Sy";
            var now = System.DateTime.Now;

            var compartments = new Compartment[]
            {
                new Compartment
                {
                    Id = 1,
                    Code = "Compartment #1",
                    ItemId = this.itemVolume.Id,
                    Sub1 = subX,
                    LoadingUnitId = this.loadingUnit1.Id,
                    Stock = 2,
                    FirstStoreDate = now.AddHours(-1)
                },
                new Compartment
                {
                    Id = 2,
                    Code = "Compartment #2",
                    ItemId = this.itemVolume.Id,
                    Sub1 = subX,
                    LoadingUnitId = this.loadingUnit1.Id,
                    Stock = 2,
                    FirstStoreDate = now.AddHours(-3)
                },
                new Compartment
                {
                    Id = 3,
                    Code = "Compartment #3",
                    ItemId = this.itemVolume.Id,
                    Sub1 = subZ,
                    LoadingUnitId = this.loadingUnit1.Id,
                    Stock = 2,
                    FirstStoreDate = now.AddHours(-1)
                },
                new Compartment
                {
                    Id = 4,
                    Code = "Compartment #4",
                    ItemId = this.itemVolume.Id,
                    Sub1 = subY,
                    LoadingUnitId = this.loadingUnit1.Id,
                    Stock = 1,
                    FirstStoreDate = now.AddHours(-1)
                },
                new Compartment
                {
                    Id = 5,
                    Code = "Compartment #5",
                    ItemId = this.itemVolume.Id,
                    Sub1 = subY,
                    LoadingUnitId = this.loadingUnit1.Id,
                    Stock = 1,
                    FirstStoreDate = now.AddHours(-2)
                },
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.AddRange(compartments);

                context.SaveChanges();
            }

            #endregion Arrange

            using (var context = this.CreateContext())
            {
                #region Act

                var provider = new SchedulerRequestProvider(context);

                var schedulerRequest = new BusinessModels.SchedulerRequest
                {
                    ItemId = this.itemVolume.Id,
                    AreaId = this.area1.Id,
                    RequestedQuantity = 1,
                    Type = BusinessModels.OperationType.Withdrawal
                };

                var acceptedRequest = await provider.FullyQualifyWithdrawalRequest(schedulerRequest);

                #endregion Act

                #region Assert

                Assert.IsNotNull(acceptedRequest);
                Assert.AreSame(compartments[compartments.Length - 1].Sub1, acceptedRequest.Sub1);

                #endregion Assert
            }
        }

        [TestMethod]
        [TestProperty("Description",
            @"GIVEN two compartments with different Sub1's \
                AND two requests allocated on the two Sub1's \
               WHEN a new request is made that has no specific Sub1, \
                    but has as requested quantity the sum of all the remainder availability across different subs \
               THEN the new request should be rejected")]
        public async Task TwoCompartmentsWithDifferentSubsAndNoAvailability()
        {
            #region Arrange

            var subX = "SX";
            var subZ = "SZ";

            var compartment1 = new Compartment
            {
                Id = 1,
                Code = "Compartment #1",
                ItemId = this.item1.Id,
                Sub1 = subX,
                LoadingUnitId = this.loadingUnit1.Id,
                Stock = 10,
            };

            var compartment2 = new Compartment
            {
                Id = 2,
                Code = "Compartment #2",
                ItemId = this.item1.Id,
                Sub1 = subZ,
                LoadingUnitId = this.loadingUnit1.Id,
                Stock = 10,
            };

            var request1 = new SchedulerRequest
            {
                Id = 1,
                ItemId = this.item1.Id,
                Sub1 = subX,
                RequestedQuantity = 9,
                OperationType = OperationType.Withdrawal
            };

            var request2 = new SchedulerRequest
            {
                Id = 2,
                ItemId = this.item1.Id,
                Sub1 = subZ,
                RequestedQuantity = 9,
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
                    RequestedQuantity = 2,
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

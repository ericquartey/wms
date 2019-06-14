using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.WebAPI.Scheduler.Tests
{
    public partial class SchedulerRequestPickProviderTest
    {
        #region Fields

        private const int OtherBayId = 1000;

        private const int OtherLoadingUnitId = 1000;

        #endregion

        #region Methods

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN two compartments with same Sub1, but in different areas \
                AND a requests allocated to the first area, so that there is no availability on that area \
               WHEN a new request for the first area is made \
               THEN the new request should be rejected")]
        public async Task FullyQualifyPickRequestAsync_CompartmentsInDifferentAreas()
        {
            #region Arrange

            const string sub1 = "S1";

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.Item1.Id,
                Sub1 = sub1,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
            };

            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = this.Item1.Id,
                Sub1 = sub1,
                LoadingUnitId = OtherLoadingUnitId,
                Stock = 10,
            };

            var request1 = new Common.DataModels.SchedulerRequest
            {
                Id = 1,
                ItemId = this.Item1.Id,
                Sub1 = sub1,
                RequestedQuantity = compartment1.Stock,
                ReservedQuantity = 0,
                OperationType = Common.DataModels.OperationType.Withdrawal,
                Status = Common.DataModels.SchedulerRequestStatus.New,
                Type = Common.DataModels.SchedulerRequestType.Item
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.SchedulerRequests.Add(request1);
                context.SaveChanges();
            }

            var schedulerRequestPickProvider = this.GetService<ISchedulerRequestPickProvider>();

            var options = new ItemOptions
            {
                AreaId = this.Area1.Id,
                RequestedQuantity = 1
            };

            #endregion

            #region Act

            var result = await schedulerRequestPickProvider.FullyQualifyPickRequestAsync(this.Item1.Id, options);

            #endregion

            #region Assert

            Assert.IsFalse(result.Success);

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN a compartment associated to an item \
               WHEN a new request for that item is made, without specific selection parameters (eg. Sub1, Lot, etc.) \
               THEN the new request should be accepted and all the request parameters should be populated")]
        public async Task FullyQualifyPickRequestAsync_FullRequestQualification()
        {
            #region Arrange

            var schedulerRequestPickProvider = this.GetService<ISchedulerRequestPickProvider>();

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.Item1.Id,
                Sub1 = "S1",
                Sub2 = "S2",
                Lot = "Lot1",
                PackageTypeId = 1,
                RegistrationNumber = "RegistrationNumber1",
                MaterialStatusId = 1,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var options = new ItemOptions
            {
                AreaId = this.Area1.Id,
                RequestedQuantity = 1,
                RunImmediately = true
            };

            var result = await schedulerRequestPickProvider.FullyQualifyPickRequestAsync(this.Item1.Id, options);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success);
            var acceptedRequests = result.Entity;
            Assert.IsNotNull(acceptedRequests);
            Assert.AreEqual(1, acceptedRequests.Count());

            var acceptedRequest = acceptedRequests.Single();
            Assert.IsTrue(acceptedRequest.IsInstant);
            Assert.AreSame(compartment1.Sub1, acceptedRequest.Sub1);
            Assert.AreSame(compartment1.Sub2, acceptedRequest.Sub2);
            Assert.AreSame(compartment1.Lot, acceptedRequest.Lot);
            Assert.AreEqual(compartment1.PackageTypeId.Value, acceptedRequest.PackageTypeId.Value);
            Assert.AreEqual(compartment1.MaterialStatusId.Value, acceptedRequest.MaterialStatusId.Value);
            Assert.AreSame(compartment1.RegistrationNumber, acceptedRequest.RegistrationNumber);

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN a compartment in a specific area and aisle \
                WHEN a new request is made for another area that has no compatible compartments \
                THEN the new request should be rejected")]
        public async Task FullyQualifyPickRequestAsync_NoCompartments()
        {
            #region Arrange

            var schedulerRequestPickProvider = this.GetService<ISchedulerRequestPickProvider>();

            const string sub1 = "S1";

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.Item1.Id,
                Sub1 = sub1,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.SaveChanges();
            }

            var options = new ItemOptions
            {
                AreaId = this.Area1.Id,
                BayId = OtherBayId,
                RequestedQuantity = 1
            };

            #endregion

            #region Act

            var result = await schedulerRequestPickProvider.FullyQualifyPickRequestAsync(this.Item1.Id, options);

            #endregion

            #region Assert

            Assert.IsFalse(result.Success);

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN a compartment in a specific area, associated to a specific item \
                AND a pick request for the given item on a bay of the specified area \
               WHEN a new request for the same item and area is made \
               THEN the new request should be accepted")]
        public async Task FullyQualifyPickRequestAsync_Nominal()
        {
            #region Arrange

            const string sub1 = "S1";

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.Item1.Id,
                Sub1 = sub1,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.SaveChanges();
            }

            var schedulerRequestPickProvider = this.GetService<ISchedulerRequestPickProvider>();

            var options = new ItemOptions
            {
                AreaId = this.Area1.Id,
                BayId = this.Bay1Aisle1.Id,
                RequestedQuantity = 1,
                RunImmediately = true
            };

            #endregion

            #region Act

            var result = await schedulerRequestPickProvider.FullyQualifyPickRequestAsync(this.Item1.Id, options);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success);
            var acceptedRequests = result.Entity;
            Assert.IsNotNull(acceptedRequests);
            Assert.AreEqual(1, acceptedRequests.Count());

            var acceptedRequest = acceptedRequests.Single();
            Assert.AreSame(compartment1.Sub1, acceptedRequest.Sub1);

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN two compartments with different Sub1's \
               WHEN a new request is made that has no specific Sub1, \
               THEN the new request should be accepted on the two Sub1's")]
        public async Task FullyQualifyPickRequestAsync_RequestOnMultipleSets()
        {
            #region Arrange

            var schedulerRequestPickProvider = this.GetService<ISchedulerRequestPickProvider>();

            const string subX = "SX";
            const string subZ = "SZ";

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.Item1.Id,
                Sub1 = subX,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
            };

            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = this.Item1.Id,
                Sub1 = subZ,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var options = new ItemOptions
            {
                AreaId = this.Area1.Id,
                RequestedQuantity = 20,
                RunImmediately = true
            };

            var result = await schedulerRequestPickProvider.FullyQualifyPickRequestAsync(this.Item1.Id, options);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success, result.Description);
            Assert.AreEqual(2, result.Entity.Count());
            Assert.IsTrue(result.Entity.Any(r => r.Sub1 == subX));
            Assert.IsTrue(result.Entity.Any(r => r.Sub1 == subZ));

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN some compartments for a FIFO item with different Sub1's and different first put dates \
                AND no other requests are present \
               WHEN a new request for no particular Sub1 is made \
               THEN the new request should be accepted\
                AND the accepted request should select the Sub1's with oldest put date")]
        public async Task FullyQualifyPickRequestAsync_SingleFifoCompartment()
        {
            #region Arrange

            const string subX = "Sx";
            const string subZ = "Sz";

            var compartments = new[]
            {
                new Common.DataModels.Compartment
                {
                    Id = 1,
                    ItemId = this.ItemFifo.Id,
                    Sub1 = subX,
                    LoadingUnitId = this.LoadingUnit1Cell1.Id,
                    Stock = 10,
                    FifoStartDate = System.DateTime.Now.AddDays(-0.5)
                },
                new Common.DataModels.Compartment
                {
                    Id = 2,
                    ItemId = this.ItemFifo.Id,
                    Sub1 = subX,
                    LoadingUnitId = this.LoadingUnit1Cell1.Id,
                    Stock = 10,
                    FifoStartDate = System.DateTime.Now.AddDays(-3)
                },
                new Common.DataModels.Compartment
                {
                    Id = 3,
                    ItemId = this.ItemFifo.Id,
                    Sub1 = subZ,
                    LoadingUnitId = this.LoadingUnit1Cell1.Id,
                    Stock = 10,
                    FifoStartDate = System.DateTime.Now.AddDays(-2)
                },
                new Common.DataModels.Compartment
                {
                    Id = 4,
                    ItemId = this.ItemFifo.Id,
                    Sub1 = subZ,
                    LoadingUnitId = this.LoadingUnit1Cell1.Id,
                    Stock = 10,
                    FifoStartDate = System.DateTime.Now.AddDays(-4)
                }
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.AddRange(compartments);
                context.SaveChanges();
            }

            var schedulerRequestPickProvider = this.GetService<ISchedulerRequestPickProvider>();

            var options = new ItemOptions
            {
                AreaId = this.Area1.Id,
                RequestedQuantity = 1,
                RunImmediately = true
            };

            #endregion

            #region Act

            var result = await schedulerRequestPickProvider.FullyQualifyPickRequestAsync(this.ItemFifo.Id, options);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success);
            var acceptedRequests = result.Entity;
            Assert.IsNotNull(acceptedRequests);
            Assert.AreEqual(1, acceptedRequests.Count());

            Assert.AreSame(
                compartments.OrderBy(c => c.FifoStartDate).First().Sub1,
                acceptedRequests.Single().Sub1);

            #endregion
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN a compartment with Sub1 \
                AND no other requests are present \
               WHEN a new request for no particular Sub1 is made \
               THEN the new request should be accepted")]
        public async Task SingleCompartmentWithNoRequestsTest()
        {
            #region Arrange

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.Item1.Id,
                Sub1 = "S1",
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var schedulerRequestPickProvider = this.GetService<ISchedulerRequestPickProvider>();

            var options = new ItemOptions
            {
                AreaId = this.Area1.Id,
                RequestedQuantity = 1,
                RunImmediately = true
            };

            var result = await schedulerRequestPickProvider.FullyQualifyPickRequestAsync(this.Item1.Id, options);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success);
            var acceptedRequests = result.Entity;
            Assert.IsNotNull(acceptedRequests);
            Assert.AreEqual(1, acceptedRequests.Count());

            var acceptedRequest = acceptedRequests.Single();
            Assert.AreSame(compartment1.Sub1, acceptedRequest.Sub1);

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN a compartment with Sub1 \
                AND a request that uses part of the compartment's availability \
               WHEN a new request for the same Sub1 is made for the remaining availability \
               THEN the new request should be accepted")]
        public async Task SingleCompartmentWithOneRequestTest()
        {
            #region Arrange

            const string sub1 = "S1";

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.Item1.Id,
                Sub1 = sub1,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
            };

            var request1 = new Common.DataModels.SchedulerRequest
            {
                Id = 1,
                ItemId = this.Item1.Id,
                Sub1 = sub1,
                RequestedQuantity = 3,
                ReservedQuantity = 0,
                Type = Common.DataModels.SchedulerRequestType.Item,
                Status = Common.DataModels.SchedulerRequestStatus.New,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.SchedulerRequests.Add(request1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var schedulerRequestPickProvider = this.GetService<ISchedulerRequestPickProvider>();

            var options = new ItemOptions
            {
                AreaId = this.Area1.Id,
                RequestedQuantity = 1,
                RunImmediately = true
            };

            var result = await schedulerRequestPickProvider.FullyQualifyPickRequestAsync(this.Item1.Id, options);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success);
            var acceptedRequests = result.Entity;
            Assert.IsNotNull(acceptedRequests);
            Assert.AreEqual(1, acceptedRequests.Count());

            var acceptedRequest = acceptedRequests.Single();
            Assert.AreSame(compartment1.Sub1, acceptedRequest.Sub1);

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN a compartment with Sub1 \
                AND two requests allocated so that the compartment has no availability \
               WHEN a new request is made \
               THEN the new request should be rejected")]
        public async Task SingleCompartmentWithTwoRequestsAndNoAvailabilityTest()
        {
            #region Arrange

            const string sub1 = "S1";

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.Item1.Id,
                Sub1 = sub1,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
            };

            var request1 = new Common.DataModels.SchedulerRequest
            {
                Id = 1,
                ItemId = this.Item1.Id,
                Sub1 = sub1,
                RequestedQuantity = 5,
                ReservedQuantity = 0,
                Type = Common.DataModels.SchedulerRequestType.Item,
                Status = Common.DataModels.SchedulerRequestStatus.New,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            var request2 = new Common.DataModels.SchedulerRequest
            {
                Id = 2,
                ItemId = this.Item1.Id,
                Sub1 = sub1,
                RequestedQuantity = 5,
                ReservedQuantity = 0,
                Type = Common.DataModels.SchedulerRequestType.Item,
                Status = Common.DataModels.SchedulerRequestStatus.New,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.SchedulerRequests.Add(request1);
                context.SchedulerRequests.Add(request2);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var schedulerRequestPickProvider = this.GetService<ISchedulerRequestPickProvider>();

            var options = new ItemOptions
            {
                AreaId = this.Area1.Id,
                RequestedQuantity = 1,
                RunImmediately = true
            };

            var result = await schedulerRequestPickProvider.FullyQualifyPickRequestAsync(this.Item1.Id, options);

            #endregion

            #region Assert

            Assert.IsFalse(result.Success);

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN some compartments for a Volume item with different Sub1's and different stocks \
                AND no other requests are present \
               WHEN a new request for no particular Sub1 is made \
               THEN the new request should be accepted \
                AND the accepted request should select the Sub1's with less stock")]
        public async Task SingleCompartmentWithVolumeTest()
        {
            #region Arrange

            const string subX = "Sx";
            const string subZ = "Sz";
            const string subY = "Sy";
            var now = System.DateTime.Now;

            var compartments = new[]
            {
                new Common.DataModels.Compartment
                {
                    Id = 1,
                    ItemId = this.ItemVolume.Id,
                    Sub1 = subX,
                    LoadingUnitId = this.LoadingUnit1Cell1.Id,
                    Stock = 2,
                    FifoStartDate = now.AddDays(-0.5)
                },
                new Common.DataModels.Compartment
                {
                    Id = 2,
                    ItemId = this.ItemVolume.Id,
                    Sub1 = subX,
                    LoadingUnitId = this.LoadingUnit1Cell1.Id,
                    Stock = 2,
                    FifoStartDate = now.AddDays(-3)
                },
                new Common.DataModels.Compartment
                {
                    Id = 3,
                    ItemId = this.ItemVolume.Id,
                    Sub1 = subZ,
                    LoadingUnitId = this.LoadingUnit1Cell1.Id,
                    Stock = 2,
                    FifoStartDate = now.AddDays(-0.5)
                },
                new Common.DataModels.Compartment
                {
                    Id = 4,
                    ItemId = this.ItemVolume.Id,
                    Sub1 = subY,
                    LoadingUnitId = this.LoadingUnit1Cell1.Id,
                    Stock = 1,
                    FifoStartDate = now.AddDays(-0.5)
                },
                new Common.DataModels.Compartment
                {
                    Id = 5,
                    ItemId = this.ItemVolume.Id,
                    Sub1 = subY,
                    LoadingUnitId = this.LoadingUnit1Cell1.Id,
                    Stock = 1,
                    FifoStartDate = now.AddDays(-2)
                },
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.AddRange(compartments);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var schedulerRequestPickProvider = this.GetService<ISchedulerRequestPickProvider>();

            var options = new ItemOptions
            {
                AreaId = this.Area1.Id,
                RequestedQuantity = 1,
                RunImmediately = true
            };

            var result = await schedulerRequestPickProvider.FullyQualifyPickRequestAsync(this.ItemVolume.Id, options);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success);

            var acceptedRequests = result.Entity;
            Assert.IsNotNull(acceptedRequests);
            Assert.AreEqual(1, acceptedRequests.Count());

            var acceptedRequest = acceptedRequests.Single();
            Assert.AreEqual(subY, acceptedRequest.Sub1);

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN two compartments with different Sub1's \
                AND two requests allocated on the two Sub1's \
               WHEN a new request is made that has no specific Sub1, \
                    but its requested quantity equals the sum of all the remainder availability across different subs \
               THEN the new request should be accepted")]
        public async Task TwoCompartmentsWithDifferentSubsAndWithAvailability()
        {
            #region Arrange

            var schedulerRequestPickProvider = this.GetService<ISchedulerRequestPickProvider>();

            const string subX = "SX";
            const string subZ = "SZ";

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.Item1.Id,
                Sub1 = subX,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
            };

            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = this.Item1.Id,
                Sub1 = subZ,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
            };

            var request1 = new Common.DataModels.SchedulerRequest
            {
                Id = 1,
                ItemId = this.Item1.Id,
                Sub1 = subX,
                RequestedQuantity = 9,
                ReservedQuantity = 0,
                Type = Common.DataModels.SchedulerRequestType.Item,
                Status = Common.DataModels.SchedulerRequestStatus.New,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            var request2 = new Common.DataModels.SchedulerRequest
            {
                Id = 2,
                ItemId = this.Item1.Id,
                Sub1 = subZ,
                RequestedQuantity = 9,
                ReservedQuantity = 0,
                Type = Common.DataModels.SchedulerRequestType.Item,
                Status = Common.DataModels.SchedulerRequestStatus.New,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.SchedulerRequests.Add(request1);
                context.SchedulerRequests.Add(request2);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var options = new ItemOptions
            {
                AreaId = this.Area1.Id,
                RequestedQuantity = 2,
                RunImmediately = true
            };

            var result = await schedulerRequestPickProvider.FullyQualifyPickRequestAsync(this.Item1.Id, options);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success, result.Description);

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN two compartments with same Sub1 \
                AND two requests allocated so that there is still availability \
               WHEN a new request for the same Sub1 is made \
               THEN the new request should be accepted")]
        public async Task TwoCompartmentsWithTwoRequestsAndNoAvailabilityOnOneCompartmentTest()
        {
            #region Arrange

            const string sub1 = "S1";

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.Item1.Id,
                Sub1 = sub1,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
            };

            var compartment2 = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = this.Item1.Id,
                Sub1 = sub1,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
            };

            var request1 = new Common.DataModels.SchedulerRequest
            {
                Id = 1,
                ItemId = this.Item1.Id,
                Sub1 = sub1,
                RequestedQuantity = compartment1.Stock,
                ReservedQuantity = 0,
                Status = Common.DataModels.SchedulerRequestStatus.New,
                Type = Common.DataModels.SchedulerRequestType.Item,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            var request2 = new Common.DataModels.SchedulerRequest
            {
                Id = 2,
                ItemId = this.Item1.Id,
                Sub1 = sub1,
                RequestedQuantity = compartment2.Stock / 2,
                ReservedQuantity = 0,
                Type = Common.DataModels.SchedulerRequestType.Item,
                OperationType = Common.DataModels.OperationType.Withdrawal
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.SchedulerRequests.Add(request1);
                context.SchedulerRequests.Add(request2);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var schedulerRequestPickProvider = this.GetService<ISchedulerRequestPickProvider>();

            var options = new ItemOptions
            {
                AreaId = this.Area1.Id,
                RequestedQuantity = compartment2.Stock - request2.RequestedQuantity.Value,
                RunImmediately = true
            };

            var result = await schedulerRequestPickProvider.FullyQualifyPickRequestAsync(this.Item1.Id, options);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success);
            var acceptedRequests = result.Entity;
            Assert.IsNotNull(acceptedRequests);
            Assert.AreEqual(1, acceptedRequests.Count());

            var acceptedRequest = acceptedRequests.Single();
            Assert.AreSame(compartment2.Sub1, acceptedRequest.Sub1);

            #endregion
        }

        #endregion
    }
}

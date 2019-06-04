using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.DataModels;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.Tests.Scheduler
{
    [TestClass]
    public class CompartmentOperationProviderTest : BaseDataContextTest
    {
        #region Fields

        private Common.DataModels.Bay bayA;

        private Common.DataModels.Bay bayB;

        private Common.DataModels.Compartment compartmentInMachineA;

        private Common.DataModels.Compartment compartmentInMachineB;

        private ICompartmentOperationProvider provider;

        #endregion

        #region Methods

        [TestCleanup]
        public void Cleanup()
        {
            this.CleanupDatabase();
        }

        [TestMethod]
        [TestCategory("Nominal Case")]
        [TestCategory("Unit")]
        public async Task GetByIdForStockUpdateAsync_Nominal()
        {
            #region Arrange

            var compartmentId = this.compartmentInMachineA.Id;

            #endregion

            #region Act

            var updatedCompartment = await this.provider.GetByIdForStockUpdateAsync(compartmentId);

            #endregion

            #region Assert

            Assert.IsNotNull(updatedCompartment);
            Assert.AreEqual(this.compartmentInMachineA.Id, updatedCompartment.Id);
            Assert.AreEqual(this.compartmentInMachineA.IsItemPairingFixed, updatedCompartment.IsItemPairingFixed);
            Assert.AreEqual(this.compartmentInMachineA.ItemId, updatedCompartment.ItemId);
            Assert.AreEqual(this.compartmentInMachineA.LastPickDate, updatedCompartment.LastPickDate);
            Assert.AreEqual(this.compartmentInMachineA.LoadingUnitId, updatedCompartment.LoadingUnitId);
            Assert.AreEqual(this.compartmentInMachineA.ReservedForPick, updatedCompartment.ReservedForPick);
            Assert.AreEqual(this.compartmentInMachineA.Stock, updatedCompartment.Stock);

            #endregion
        }

        [TestMethod]
        [TestCategory("Error case")]
        [TestCategory("Unit")]
        public async Task GetByIdForStockUpdateAsync_NotFound()
        {
            #region Arrange

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 100
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);

                context.SaveChanges();
            }

            #endregion

            #region Act

            var wrongCompartmentId = 10;

            var updatedCompartment = await this.provider.GetByIdForStockUpdateAsync(wrongCompartmentId);

            #endregion

            #region Assert

            Assert.IsNull(updatedCompartment);

            #endregion
        }

        [TestMethod]
        [TestCategory("Nominal Case")]
        [TestCategory("Unit")]
        public void GetCompartmentIsInBayFunction_Vertimag()
        {
            IQueryable<Compartment> compartments;
            using (var context = this.CreateContext())
            {
                #region Arrange

                compartments = context.Compartments
                    .Include(c => c.LoadingUnit)
                    .ThenInclude(l => l.Cell)
                    .ThenInclude(c => c.Aisle)
                    .ThenInclude(a => a.Machines)
                    .ThenInclude(m => m.Bays);

                #endregion

                #region Act

                var compartmentsInBayFunction = this.provider.GetCompartmentIsInBayFunction(this.bayA.Id);

                #endregion

                #region Assert

                Assert.IsNotNull(compartmentsInBayFunction);

                var filteredCompartments = compartments.Where(compartmentsInBayFunction);

                Assert.AreEqual(filteredCompartments.Single().Id, this.compartmentInMachineA.Id);

                #endregion
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetCompartmentIsInBayFunction_Vertimag_NoBay()
        {
            #region Arrange

            var compartments = new List<Common.DataModels.Compartment>
            {
                this.compartmentInMachineA,
                this.compartmentInMachineB
            }.AsQueryable();

            int? bayId = null;

            #endregion

            #region Act

            var compartmentsInBayFunction = this.provider.GetCompartmentIsInBayFunction(bayId);

            #endregion

            #region Assert

            Assert.IsNotNull(compartmentsInBayFunction);

            var filteredCompartments = compartments.Where(compartmentsInBayFunction);

            Assert.AreEqual(2, filteredCompartments.Count());
            Assert.IsTrue(filteredCompartments.Contains(this.compartmentInMachineA));
            Assert.IsTrue(filteredCompartments.Contains(this.compartmentInMachineB));

            #endregion
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();

            this.provider = this.GetService<ICompartmentOperationProvider>();
        }

        public void InitializeDatabase()
        {
            var area = new Area { Id = 1 };

            var aisleA = new Aisle { Id = 1, AreaId = area.Id };

            var aisleB = new Aisle { Id = 2, AreaId = area.Id };

            var machineA = new Machine { Id = 1, AisleId = aisleA.Id };

            var machineB = new Machine { Id = 2, AisleId = aisleB.Id };

            this.bayA = new Bay { Id = 1, AreaId = area.Id, MachineId = machineA.Id };

            this.bayB = new Bay { Id = 2, AreaId = area.Id, MachineId = machineB.Id };

            var cellA = new Cell { Id = 1, AisleId = aisleA.Id };

            var cellB = new Cell { Id = 2, AisleId = aisleB.Id };

            var loadingUnitA = new LoadingUnit { Id = 1, CellId = cellA.Id };

            var loadingUnitB = new LoadingUnit { Id = 2, CellId = cellB.Id };

            var compartmentType = new CompartmentType { Id = 1, Height = 1, Width = 1 };

            var itemCompartmentType = new ItemCompartmentType
            {
                CompartmentTypeId = compartmentType.Id,
                ItemId = 1,
                MaxCapacity = 10
            };

            this.compartmentInMachineA = new Compartment
            {
                Id = 1,
                LoadingUnitId = loadingUnitA.Id,
                IsItemPairingFixed = true,
                ItemId = itemCompartmentType.ItemId,
                LastPickDate = System.DateTime.Now.AddDays(-1),
                ReservedForPick = 5,
                ReservedToPut = 6,
                Stock = 47,
                CompartmentTypeId = compartmentType.Id
            };

            this.compartmentInMachineB = new Common.DataModels.Compartment
            {
                Id = 2,
                ItemId = itemCompartmentType.ItemId,
                LoadingUnitId = loadingUnitB.Id,
                CompartmentTypeId = compartmentType.Id
            };

            using (var context = this.CreateContext())
            {
                context.Areas.Add(area);
                context.Bays.Add(this.bayA);
                context.Bays.Add(this.bayB);
                context.Aisles.Add(aisleA);
                context.Aisles.Add(aisleB);
                context.Cells.Add(cellA);
                context.Cells.Add(cellB);
                context.Machines.Add(machineA);
                context.Machines.Add(machineB);
                context.LoadingUnits.Add(loadingUnitA);
                context.LoadingUnits.Add(loadingUnitB);
                context.CompartmentTypes.Add(compartmentType);
                context.ItemsCompartmentTypes.Add(itemCompartmentType);
                context.Compartments.Add(this.compartmentInMachineA);
                context.Compartments.Add(this.compartmentInMachineB);

                context.SaveChanges();
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(Core.Models.ItemManagementType.FIFO, Core.Models.OperationType.Insertion, 2, 3, 4, 1)]
        [DataRow(Core.Models.ItemManagementType.FIFO, Core.Models.OperationType.Withdrawal, 3, 2, 4, 1)]
        [DataRow(Core.Models.ItemManagementType.Volume, Core.Models.OperationType.Insertion, 4, 1, 2, 3)]
        [DataRow(Core.Models.ItemManagementType.Volume, Core.Models.OperationType.Withdrawal, 3, 1, 2, 4)]
        public void OrderCompartmentsByManagementType_Compartments(
            Core.Models.ItemManagementType managementType,
            Core.Models.OperationType operationType,
            int firstCompartmentId,
            int secondCompartmentId,
            int thirdCompartmentId,
            int fourthCompartmentId)
        {
            #region Arrange

            var now = DateTime.Now;
            var compartments = new List<Core.Models.CandidateCompartment>
            {
               new Core.Models.CandidateCompartment
               {
                   Id = 1,
                   FifoStartDate = now.AddDays(-1),
                   Stock = 90,
                   MaxCapacity = 200
               },
               new Core.Models.CandidateCompartment
               {
                   Id = 2,
                   FifoStartDate = now.AddDays(-3),
                   Stock = 100,
                   MaxCapacity = 400
               },
               new Core.Models.CandidateCompartment
               {
                   Id = 3,
                   FifoStartDate = now.AddDays(-3),
                   Stock = 50,
                   MaxCapacity = 400
               },
               new Core.Models.CandidateCompartment
               {
                   Id = 4,
                   FifoStartDate = now.AddDays(-2),
                   Stock = 400,
                   MaxCapacity = 500
               }
            }.AsQueryable();

            #endregion

            #region Act

            var orderedCompartments = this.provider.OrderCompartmentsByManagementType(compartments, managementType, operationType);

            #endregion

            #region Assert

            Assert.AreEqual(compartments.Count(), orderedCompartments.Count());
            Assert.AreEqual(firstCompartmentId, orderedCompartments.ElementAt(0).Id);
            Assert.AreEqual(secondCompartmentId, orderedCompartments.ElementAt(1).Id);
            Assert.AreEqual(thirdCompartmentId, orderedCompartments.ElementAt(2).Id);
            Assert.AreEqual(fourthCompartmentId, orderedCompartments.ElementAt(3).Id);

            #endregion
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(Core.Models.ItemManagementType.FIFO, Core.Models.OperationType.Insertion, 3, 2, 4, 1)]
        [DataRow(Core.Models.ItemManagementType.FIFO, Core.Models.OperationType.Withdrawal, 3, 2, 4, 1)]
        [DataRow(Core.Models.ItemManagementType.Volume, Core.Models.OperationType.Insertion, 4, 3, 2, 1)]
        [DataRow(Core.Models.ItemManagementType.Volume, Core.Models.OperationType.Withdrawal, 4, 1, 3, 2)]
        public void OrderCompartmentsByManagementType_CompartmentSets(
           Core.Models.ItemManagementType managementType,
           Core.Models.OperationType operationType,
           int firstCompartmentSize,
           int secondCompartmentSize,
           int thirdCompartmentSize,
           int fourthCompartmentSize)
        {
            #region Arrange

            var now = DateTime.Now;
            var compartments = new List<Core.Models.CompartmentSet>
            {
               new Core.Models.CompartmentSet
               {
                   FifoStartDate = now.AddDays(-2),
                   Availability = 100,
                   RemainingCapacity = 300,
                   Size = 1
               },
               new Core.Models.CompartmentSet
               {
                   FifoStartDate = now.AddDays(-3),
                   Availability = 300,
                   RemainingCapacity = 500,
                   Size = 2
               },
               new Core.Models.CompartmentSet
               {
                   FifoStartDate = now.AddDays(-3),
                   Availability = 303,
                   RemainingCapacity = 500,
                   Size = 3
               },
               new Core.Models.CompartmentSet
               {
                   FifoStartDate = now.AddDays(-2),
                   Availability = 300,
                   RemainingCapacity = 500,
                   Size = 4
               }
            }.AsQueryable();

            #endregion

            #region Act

            var orderedCompartments = this.provider.OrderCompartmentsByManagementType(compartments, managementType, operationType);

            #endregion

            #region Assert

            Assert.AreEqual(compartments.Count(), orderedCompartments.Count());
            Assert.AreEqual(firstCompartmentSize, orderedCompartments.ElementAt(0).Size);
            Assert.AreEqual(secondCompartmentSize, orderedCompartments.ElementAt(1).Size);
            Assert.AreEqual(thirdCompartmentSize, orderedCompartments.ElementAt(2).Size);
            Assert.AreEqual(fourthCompartmentSize, orderedCompartments.ElementAt(3).Size);

            #endregion
        }

        protected override void AddServices(IServiceCollection services)
        {
            services.AddSchedulerServiceProvider<ICompartmentOperationProvider>();
        }

        #endregion
    }
}

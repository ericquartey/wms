using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.WebAPI.Scheduler.Tests
{
    public partial class MissionExecutionProviderTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Nominal Case")]
        [DataRow(50, "S1", "S2", 3, 4, 1)]
        [DataRow(0, null, null, null, null, null)]
        public async Task AbortItemAsync_NominalPut(
            int inStock,
            string outSub1,
            string outSub2,
            int? outMaterialStatusId,
            int? outPackageTypeId,
            int? outItemId)
        {
            #region Arrange

            var missionProvider = this.GetService<IMissionOperationProvider>();

            var compartmentProvider = this.GetService<ICompartmentProvider>();

            var itemCompartmentType = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = this.CompartmentType.Id,
                ItemId = this.Item1.Id,
                MaxCapacity = 100
            };

            var compartment = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = this.CompartmentType.Id,
                ItemId = this.Item1.Id,
                ReservedToPut = 10,
                Stock = inStock,
                Sub1 = "S1",
                Sub2 = "S2",
                MaterialStatusId = 3,
                PackageTypeId = 4
            };

            var mission = new Common.DataModels.Mission
            {
                Id = GetNewId(),
                LoadingUnitId = this.LoadingUnit1Cell1.Id
            };

            var missionOperation = new Common.DataModels.MissionOperation
            {
                Id = GetNewId(),
                MissionId = mission.Id,
                CompartmentId = compartment.Id,
                ItemId = this.Item1.Id,
                Status = Common.DataModels.MissionOperationStatus.Executing,
                Type = Common.DataModels.MissionOperationType.Put,
                RequestedQuantity = 10,
                Sub1 = compartment.Sub1,
                Sub2 = compartment.Sub2,
                MaterialStatusId = compartment.MaterialStatusId,
                PackageTypeId = compartment.PackageTypeId
            };

            using (var context = this.CreateContext())
            {
                context.ItemsCompartmentTypes.Add(itemCompartmentType);
                context.Compartments.Add(compartment);
                context.MissionOperations.Add(missionOperation);
                context.Missions.Add(mission);

                context.SaveChanges();
            }

            #endregion

            #region Act

            var result = await missionProvider.AbortAsync(missionOperation.Id);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success, result.Description);

            Assert.AreEqual(
                MissionOperationStatus.Incomplete,
                result.Entity.Status,
                $"The status of the mission should be '{MissionOperationStatus.Incomplete}'.");

            var updatedCompartment = await compartmentProvider.GetByIdAsync(compartment.Id);

            Assert.IsNotNull(updatedCompartment);

            Assert.AreEqual(
                compartment.Stock,
                updatedCompartment.Stock);

            Assert.AreEqual(
                outSub1,
                updatedCompartment.Sub1);

            Assert.AreEqual(
                outSub2,
                updatedCompartment.Sub2);

            Assert.AreEqual(
                outPackageTypeId,
                updatedCompartment.PackageTypeId);

            Assert.AreEqual(
                outMaterialStatusId,
                updatedCompartment.MaterialStatusId);

            Assert.AreEqual(
                0,
                updatedCompartment.ReservedToPut);

            Assert.AreEqual(
                outItemId == null ? null : compartment.ItemId,
                updatedCompartment.ItemId);

            #endregion
        }

        [TestProperty(
                     "Description",
             @"GIVEN an executing pick mission on a compartment \
                AND the compartment has stock of 0 \
                AND the mission has a quantity of 10 \
               WHEN the mission is completed \
               THEN the remaining compartment's stock is 10 \
                AND the mission's item information should be copied in the compartment")]
        [TestMethod]
        [TestCategory("Nominal Case")]
        public async Task CompleteItemAsync_PutInEmptyCompartment()
        {
            #region Arrange

            var operationProvider = this.GetService<IMissionOperationProvider>();

            var compartmentProvider = this.GetService<ICompartmentProvider>();

            var itemCompartmentType = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = this.CompartmentType.Id,
                ItemId = this.Item1.Id,
                MaxCapacity = 100
            };

            var emptyCompartment = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                CompartmentTypeId = this.CompartmentType.Id,
                ItemId = this.Item1.Id,
                ReservedToPut = 10,
                Sub1 = "S1",
                Sub2 = "S2",
                MaterialStatusId = 3,
                PackageTypeId = 4
            };

            var missionOperation = new Common.DataModels.MissionOperation
            {
                Id = GetNewId(),
                CompartmentId = emptyCompartment.Id,
                ItemId = this.Item1.Id,
                Status = Common.DataModels.MissionOperationStatus.Executing,
                Type = Common.DataModels.MissionOperationType.Put,
                RequestedQuantity = 10,
                Sub1 = emptyCompartment.Sub1,
                Sub2 = emptyCompartment.Sub2,
                MaterialStatusId = emptyCompartment.MaterialStatusId,
                PackageTypeId = emptyCompartment.PackageTypeId
            };

            var mission = new Common.DataModels.Mission
            {
                Id = GetNewId(),
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Operations = new[] { missionOperation }
            };

            using (var context = this.CreateContext())
            {
                context.ItemsCompartmentTypes.Add(itemCompartmentType);
                context.Compartments.Add(emptyCompartment);
                context.MissionOperations.Add(missionOperation);
                context.Missions.Add(mission);

                context.SaveChanges();
            }

            #endregion

            #region Act

            var result = await operationProvider.CompleteAsync(
                missionOperation.Id,
                missionOperation.RequestedQuantity);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success, result.Description);

            Assert.AreEqual(
                MissionOperationStatus.Completed,
                result.Entity.Status,
                $"The status of the mission should be '{MissionOperationStatus.Completed}'.");

            var updatedCompartment = await compartmentProvider.GetByIdAsync(emptyCompartment.Id);

            Assert.IsNotNull(updatedCompartment);

            Assert.AreEqual(
                missionOperation.RequestedQuantity,
                updatedCompartment.Stock);

            Assert.AreEqual(
                0,
                updatedCompartment.ReservedToPut);

            Assert.AreEqual(
                missionOperation.ItemId,
                updatedCompartment.ItemId,
                $"The mission's item information should be copied in the compartment.");

            #endregion
        }

        #endregion
    }
}

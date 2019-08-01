using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.WebAPI.Scheduler.Tests
{
    public partial class MissionExecutionProviderTest
    {
        #region Methods

        [TestProperty(
             "Description",
             @"GIVEN a pick mission on a compartment \
                AND the mission is executing \
                AND the compartment has stock of 10 items \
                AND the mission requires 10 items \
                AND the compartment-item pairing is [fixed/free] \
               WHEN the mission is completed \
               THEN the remaining compartment's stock is 0
                AND the compartment-item pairing is [maintained/lifted]")]
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CompleteItemAsync_PickAndEmptyCompartment(bool isPairingFixed)
        {
            #region Arrange

            var missionOperationProvider = this.GetService<IMissionOperationProvider>();

            var compartmentOperationProvider = this.GetService<ICompartmentOperationProvider>();

            var itemCompartmentType = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = this.CompartmentType.Id,
                ItemId = this.Item1.Id,
                MaxCapacity = 100,
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                ItemId = itemCompartmentType.ItemId,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
                ReservedForPick = 10,
                IsItemPairingFixed = isPairingFixed,
                CompartmentTypeId = this.CompartmentType.Id,
            };

            var missionOperation1 = new Common.DataModels.MissionOperation
            {
                Id = GetNewId(),
                Status = Common.DataModels.MissionOperationStatus.Executing,
                CompartmentId = compartment1.Id,
                ItemId = itemCompartmentType.ItemId,
                Type = Common.DataModels.MissionOperationType.Pick,
                RequestedQuantity = 10,
            };

            var mission = new Common.DataModels.Mission
            {
                Id = GetNewId(),
                Operations = new[] { missionOperation1 },
            };

            using (var context = this.CreateContext())
            {
                context.ItemsCompartmentTypes.Add(itemCompartmentType);
                context.Compartments.Add(compartment1);
                context.Missions.Add(mission);

                context.SaveChanges();
            }

            #endregion

            #region Act

            var result = await missionOperationProvider.CompleteAsync(missionOperation1.Id, missionOperation1.RequestedQuantity);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success);

            Assert.AreEqual(
                MissionOperationStatus.Completed,
                result.Entity.Status,
                $"The status of the mission should be '{MissionOperationStatus.Completed}'.");

            var updatedCompartment = await compartmentOperationProvider.GetByIdForStockUpdateAsync(compartment1.Id);

            Assert.IsNotNull(updatedCompartment);

            Assert.AreEqual(
                0,
                updatedCompartment.Stock,
                $"The stock of the compartment should be 0.");

            Assert.AreEqual(
                isPairingFixed,
                updatedCompartment.IsItemPairingFixed,
                $"Item pairing should not be changed.");

            if (isPairingFixed)
            {
                Assert.IsNotNull(
                    updatedCompartment.ItemId,
                    $"The item pairing should not be lifted.");
            }
            else
            {
                Assert.IsNull(
                    updatedCompartment.ItemId,
                    $"The item pairing should be lifted.");
            }

            #endregion
        }

        [TestMethod]
        [TestCategory("Nominal Case")]
        [TestProperty(
            "Description",
            @"GIVEN a pick mission on a compartment \
                AND the mission is executing \
                AND the compartment has stock of 10 items \
                AND the mission requires 7 items \
               WHEN the mission is completed \
               THEN the remaining compartment's stock is 3
                AND the compartment's reserved quantity for pick is reset
                AND the compartment-item pairing is preserved")]
        public async Task CompleteItemAsync_PickNominal()
        {
            #region Arrange

            var missionOperationProvider = this.GetService<IMissionOperationProvider>();
            var compartmentOperationProvider = this.GetService<ICompartmentOperationProvider>();
            var loadingUnitProvider = this.GetService<ILoadingUnitProvider>();
            var itemProvider = this.GetService<IItemProvider>();

            var item = this.Item1;

            var itemCompartmentType = new Common.DataModels.ItemCompartmentType
            {
                CompartmentTypeId = this.CompartmentType.Id,
                ItemId = item.Id,
                MaxCapacity = 100,
            };

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                ItemId = item.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 10,
                ReservedForPick = 7,
                LastPickDate = null,
                CompartmentTypeId = this.CompartmentType.Id,
            };

            var missionOperation1 = new Common.DataModels.MissionOperation
            {
                Id = GetNewId(),
                Status = Common.DataModels.MissionOperationStatus.Executing,
                CompartmentId = compartment1.Id,
                ItemId = item.Id,
                Type = Common.DataModels.MissionOperationType.Pick,
                RequestedQuantity = 7,
            };

            var mission = new Common.DataModels.Mission
            {
                Id = GetNewId(),
                Operations = new[] { missionOperation1 },
            };

            using (var context = this.CreateContext())
            {
                context.ItemsCompartmentTypes.Add(itemCompartmentType);
                context.Compartments.Add(compartment1);
                context.Missions.Add(mission);

                context.SaveChanges();
            }

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act

                var result = await missionOperationProvider.CompleteAsync(missionOperation1.Id, missionOperation1.RequestedQuantity);

                #endregion

                #region Assert

                Assert.IsTrue(result.Success);

                Assert.AreEqual(
                    MissionOperationStatus.Completed,
                    result.Entity.Status,
                    $"The status of the mission should be '{MissionOperationStatus.Completed}'.");

                var updatedCompartment = await compartmentOperationProvider.GetByIdForStockUpdateAsync(compartment1.Id);

                Assert.IsNotNull(updatedCompartment);

                Assert.AreEqual(
                  3,
                  updatedCompartment.Stock,
                  $"The stock of the compartment should be 3.");

                Assert.AreEqual(
                  0,
                  updatedCompartment.ReservedForPick,
                  $"The reserved quantity for pick should be 0.");

                Assert.IsNotNull(
                    updatedCompartment.ItemId,
                    $"The item pairing is not removed.");

                Assert.IsTrue(updatedCompartment.LastPickDate.HasValue);

                var updatedLoadingUnit = await loadingUnitProvider.GetByIdForExecutionAsync(compartment1.LoadingUnitId);

                Assert.IsNotNull(updatedLoadingUnit);

                Assert.IsTrue(updatedLoadingUnit.LastPickDate.HasValue);

                var updatedItem = await itemProvider.GetByIdAsync(compartment1.ItemId.Value);

                Assert.IsNotNull(updatedItem);

                Assert.IsTrue(updatedItem.LastPickDate.HasValue);

                #endregion
            }
        }

        #endregion
    }
}

using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Scheduler.Tests
{
    [TestClass]
    public class MissionProviderTest : BaseWarehouseTest
    {
        #region Methods

        [TestCleanup]
        public void Cleanup()
        {
            this.CleanupDatabase();
        }

        [TestMethod]
        [TestProperty(
            "Description",
           @"GIVEN a pick mission on a compartment \
                AND a the mission is executing \
                AND a the compartment has stock of 10 items \
                AND a the mission requires 7 items \
               WHEN the mission is completed \
               THEN the remaining compartment's stock is 3
                AND the compartment's reserved quantity for pick is reset
                AND the compartment-item pairing is preserved")]
        public async Task CompleteMission()
        {
            #region Arrange

            var missionProvider = this.ServiceProvider
                .GetService(typeof(IMissionSchedulerProvider)) as IMissionSchedulerProvider;

            var compartmentProvider = this.ServiceProvider
              .GetService(typeof(ICompartmentSchedulerProvider)) as ICompartmentSchedulerProvider;

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.Item1.Id,
                LoadingUnitId = this.LoadingUnit1.Id,
                Stock = 10,
                ReservedForPick = 7
            };

            var mission = new Common.DataModels.Mission
            {
                Id = 1,
                CompartmentId = compartment1.Id,
                ItemId = compartment1.ItemId,
                Status = Common.DataModels.MissionStatus.Executing,
                Type = Common.DataModels.MissionType.Pick,
                RequiredQuantity = 7
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Missions.Add(mission);

                context.SaveChanges();
            }

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act

                var result = await missionProvider.CompleteAsync(mission.Id);

                #endregion

                #region Assert

                Assert.IsTrue(result.Success);

                Assert.AreEqual(
                    MissionStatus.Completed,
                    result.Entity.Status,
                    $"The status of the mission should be '{MissionStatus.Completed}'.");

                var updatedCompartment = await compartmentProvider.GetByIdForStockUpdateAsync(compartment1.Id);

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

                #endregion
            }
        }

        [TestProperty(
            "Description",
            @"GIVEN a pick mission on a compartment \
                AND a the mission is executing \
                AND a the compartment has stock of 10 items \
                AND a the mission requires 10 items \
                AND a thecompartment-item pairing is [fixed/free] \
               WHEN the mission is completed \
               THEN the remaining compartment's stock is 0
                AND the compartment-item pairing is [maintained/lifted]")]
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CompleteMissionFreePairing(bool isPairingFixed)
        {
            #region Arrange

            var missionProvider = this.ServiceProvider
                .GetService(typeof(IMissionSchedulerProvider)) as IMissionSchedulerProvider;

            var compartmentProvider = this.ServiceProvider
              .GetService(typeof(ICompartmentSchedulerProvider)) as ICompartmentSchedulerProvider;

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = this.Item1.Id,
                LoadingUnitId = this.LoadingUnit1.Id,
                Stock = 10,
                ReservedForPick = 10,
                IsItemPairingFixed = isPairingFixed
            };

            var mission = new Common.DataModels.Mission
            {
                Id = 1,
                CompartmentId = compartment1.Id,
                ItemId = compartment1.ItemId,
                Status = Common.DataModels.MissionStatus.Executing,
                Type = Common.DataModels.MissionType.Pick,
                RequiredQuantity = 10
            };

            using (var context = this.CreateContext())
            {
                context.Compartments.Add(compartment1);
                context.Missions.Add(mission);

                context.SaveChanges();
            }

            #endregion

            using (var context = this.CreateContext())
            {
                #region Act

                var result = await missionProvider.CompleteAsync(mission.Id);

                #endregion

                #region Assert

                Assert.IsTrue(result.Success);

                Assert.AreEqual(
                    MissionStatus.Completed,
                    result.Entity.Status,
                    $"The status of the mission should be '{MissionStatus.Completed}'.");

                var updatedCompartment = await compartmentProvider.GetByIdForStockUpdateAsync(compartment1.Id);

                Assert.IsNotNull(updatedCompartment);

                Assert.AreEqual(
                  0,
                  updatedCompartment.Stock,
                  $"The stock of the compartment should be 0.");

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
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        #endregion
    }
}

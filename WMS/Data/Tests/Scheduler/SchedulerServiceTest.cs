using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.WebAPI.Scheduler.Tests
{
    [TestClass]
    public partial class SchedulerServiceTest : BaseWarehouseTest
    {
        #region Methods

        [TestCleanup]
        public void Cleanup()
        {
            this.CleanupDatabase();
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN two compartments in 2 different aisles (vertimag machines) \
                AND an item with volume as management type  \
                AND the compartment in the first aisle is fuller than the one in the second aisle \
               WHEN a immediate pick request is performed for the item on the first aisle \
               THEN the chosen compartment should be the one in the first aisle")]
        public async Task PickItemAsync_TwoCompartmentsInDifferentAislesTest()
        {
            #region Arrange

            var schedulerService = this.GetService<IItemSchedulerService>();

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 8,
            };

            var compartment2 = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit2Cell2.Id,
                Stock = 2,
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
                BayId = this.Bay1Aisle1.Id,
                RequestedQuantity = 2,
                RunImmediately = true
            };

            var result = await schedulerService.PickItemAsync(this.ItemVolume.Id, options);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success, result.Description);

            using (var context = this.CreateContext())
            {
                Assert.AreEqual(
                    1,
                    context.Missions.Count(),
                    "Only one mission should be generated.");

                Assert.AreEqual(
                    1,
                    context.Missions.Include(m => m.Operations).First().Operations.Count(),
                    "Only one mission operation should be generated.");

                Assert.AreEqual(
                    compartment1.Id,
                    context.Missions.First().Operations.First().CompartmentId,
                    "The chosen compartment should be the one in the first aisle");
            }

            #endregion
        }

        [TestMethod]
        [TestProperty(
            "Description",
            @"GIVEN two compartments in same aisle (vertimag machines) \
                AND an item with volume as management type  \
                AND one compartment is fuller than other \
               WHEN a immediate pick request is performed for the item on the aisle \
               THEN the chosen compartment should be the emptiest one")]
        public async Task PickItemAsync_TwoCompartmentsInSameAislesTest()
        {
            #region Arrange

            var schedulerService = this.GetService<IItemSchedulerService>();

            var compartment1 = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 8,
            };

            var compartment2 = new Common.DataModels.Compartment
            {
                Id = GetNewId(),
                ItemId = this.ItemVolume.Id,
                LoadingUnitId = this.LoadingUnit1Cell1.Id,
                Stock = 2,
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
                BayId = this.Bay1Aisle1.Id,
                RequestedQuantity = 2,
                RunImmediately = true
            };

            var result = await schedulerService.PickItemAsync(this.ItemVolume.Id, options);

            #endregion

            #region Assert

            Assert.IsTrue(result.Success, result.Description);

            using (var context = this.CreateContext())
            {
                Assert.AreEqual(
                    1,
                    context.Missions.Count(),
                    "Only one mission should be generated.");

                Assert.AreEqual(
                    1,
                    context.Missions.Include(m => m.Operations).First().Operations.Count(),
                    "Only one mission operation should be generated.");

                Assert.AreEqual(
                    compartment2.Id,
                    context.Missions.First().Operations.First().CompartmentId,
                    "The chosen compartment should be the one in the first aisle.");
            }

            #endregion
        }

        #endregion
    }
}

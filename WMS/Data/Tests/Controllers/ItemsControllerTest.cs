using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Providers;
using Ferretto.WMS.Data.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DataModels = Ferretto.Common.DataModels;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public class ItemsControllerTest : BaseControllerTest
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
        public async Task ItemAvailableButReserved()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);
                var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };
                var compartment1 = new DataModels.Compartment { Id = 1, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 10, ReservedToStore = 0 };

                context.Items.Add(item1);
                context.Compartments.Add(compartment1);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAreasWithAvailabilityAsync(item1.Id);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

                #endregion
            }
        }

        [TestMethod]
        public async Task ItemAvailableInTwoAreas()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);
                var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };
                var compartment1 = new DataModels.Compartment { Id = 1, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToStore = 0 };
                var compartment2 = new DataModels.Compartment { Id = 2, LoadingUnitId = this.LoadingUnit3.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToStore = 0 };

                context.Items.Add(item1);
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAreasWithAvailabilityAsync(item1.Id);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var resultAreas = (IEnumerable<Area>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(2, resultAreas.Count());

                #endregion
            }
        }

        [TestMethod]
        public async Task ItemAvailableTwoTimesInOneArea()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);
                var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };
                var compartment1 = new DataModels.Compartment { Id = 1, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToStore = 0 };
                var compartment2 = new DataModels.Compartment { Id = 2, LoadingUnitId = this.LoadingUnit2.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToStore = 0 };

                context.Items.Add(item1);
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAreasWithAvailabilityAsync(item1.Id);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var resultAreas = (IEnumerable<Area>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(1, resultAreas.Count());

                #endregion
            }
        }

        [TestMethod]
        public async Task ItemNotAvailable()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);
                var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };

                context.Items.Add(item1);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAreasWithAvailabilityAsync(item1.Id);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

                #endregion
            }
        }

        private static ItemsController MockController(DatabaseContext context)
        {
            return new ItemsController(
                new Mock<ILogger<ItemsController>>().Object,
                new ItemProvider(context),
                new AreaProvider(context));
        }

        #endregion
    }
}

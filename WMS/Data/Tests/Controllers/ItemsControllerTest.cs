using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataModels = Ferretto.Common.DataModels;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public class ItemsControllerTest : BaseControllerTest
    {
        #region Methods

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

                var controller = this.MockController();
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

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));

                #endregion
            }
        }

        [TestMethod]
        public async Task ItemAvailableInTwoAreas()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
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

                var controller = this.MockController();
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

                var controller = this.MockController();
                var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };

                context.Items.Add(item1);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAreasWithAvailabilityAsync(item1.Id);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<Area>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(0, result.Count());

                #endregion
            }
        }

        private ItemsController MockController()
        {
            return new ItemsController(
                this.ServiceProvider.GetService(typeof(IItemProvider)) as IItemProvider,
                this.ServiceProvider.GetService(typeof(IAreaProvider)) as IAreaProvider,
                this.ServiceProvider.GetService(typeof(ICompartmentProvider)) as ICompartmentProvider,
                this.ServiceProvider.GetService(typeof(Scheduler.Core.Interfaces.ISchedulerService)) as Scheduler.Core.Interfaces.ISchedulerService);
        }

        #endregion
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataModels = Ferretto.Common.DataModels;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    public partial class ItemsControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAreasWithAvailabilityAsync_ItemAvailableButReserved()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };
            var compartment1 = new DataModels.Compartment { Id = 1, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 10, ReservedToPut = 0 };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.Compartments.Add(compartment1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAreasWithAvailabilityAsync(item1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));

            #endregion
        }

        [TestMethod]
        public async Task GetAreasWithAvailabilityAsync_ItemAvailableInTwoAreas()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };
            var compartment1 = new DataModels.Compartment { Id = 1, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToPut = 0 };
            var compartment2 = new DataModels.Compartment { Id = 2, LoadingUnitId = this.LoadingUnit3.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToPut = 0 };
            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAreasWithAvailabilityAsync(item1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var resultAreas = (IEnumerable<Area>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(2, resultAreas.Count());

            #endregion
        }

        [TestMethod]
        public async Task GetAreasWithAvailabilityAsync_ItemAvailableTwoTimesInOneArea()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };
            var compartment1 = new DataModels.Compartment { Id = 1, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToPut = 0 };
            var compartment2 = new DataModels.Compartment { Id = 2, LoadingUnitId = this.LoadingUnit2.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToPut = 0 };
            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAreasWithAvailabilityAsync(item1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var resultAreas = (IEnumerable<Area>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(1, resultAreas.Count());

            #endregion
        }

        [TestMethod]
        public async Task GetAreasWithAvailabilityAsync_ItemNotAvailable()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };
            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAreasWithAvailabilityAsync(item1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (IEnumerable<Area>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(0, result.Count());

            #endregion
        }

        #endregion
    }
}

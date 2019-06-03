using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.Tests
{
    public partial class ItemsControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task CreateAllowedAreaAsync_CreateExisting()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new Common.DataModels.Item { Id = 1, Code = "Item #1" };
            var item1Area1 = new Common.DataModels.ItemArea { ItemId = item1.Id, AreaId = this.Area1.Id };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.ItemsAreas.Add(item1Area1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.CreateAllowedAreaAsync(item1.Id, this.Area1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(
                actionResult,
                typeof(UnprocessableEntityObjectResult),
                "Operation result should be Unprocessable");

            #endregion
        }

        [TestMethod]
        public async Task CreateAllowedAreaAsync_CreateNotExisting()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new Common.DataModels.Item { Id = 1, Code = "Item #1" };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.CreateAllowedAreaAsync(item1.Id, this.Area1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(
                actionResult,
                typeof(CreatedAtActionResult),
                "Operation result should be Ok");

            using (var context = this.CreateContext())
            {
                var dataModel =
                    context.ItemsAreas.FirstOrDefault(x => x.AreaId == this.Area1.Id && x.ItemId == item1.Id);
                Assert.IsNotNull(
                    dataModel,
                    "DB record should be present");
            }

            #endregion
        }

        [TestMethod]
        public async Task DeleteAllowedByItemIdAsync_DeleteExisting()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new Common.DataModels.Item { Id = 1, Code = "Item #1" };
            var item1Area1 = new Common.DataModels.ItemArea { ItemId = item1.Id, AreaId = this.Area1.Id };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.ItemsAreas.Add(item1Area1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.DeleteAllowedAreaAsync(item1Area1.ItemId, item1Area1.AreaId);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(
                actionResult,
                typeof(OkResult),
                "Operation result should be Ok");

            using (var context = this.CreateContext())
            {
                var dataModel =
                    context.ItemsAreas.FirstOrDefault(x =>
                        x.AreaId == item1Area1.AreaId && x.ItemId == item1Area1.ItemId);
                Assert.IsNull(
                    dataModel,
                    "DB record should not be present");
            }

            #endregion
        }

        [TestMethod]
        public async Task DeleteAllowedByItemIdAsync_DeleteNotExisting()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new Common.DataModels.Item { Id = 1, Code = "Item #1" };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.DeleteAllowedAreaAsync(item1.Id, this.Area1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(
                actionResult,
                typeof(NotFoundObjectResult),
                "Operation result should be Ok");

            #endregion
        }

        [TestMethod]
        public async Task GetAllowedAreasAsync_DeletePolicyAllowed()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new Common.DataModels.Item { Id = 1, Code = "Item #1" };
            var item1Area1 = new Common.DataModels.ItemArea { ItemId = item1.Id, AreaId = this.Area1.Id };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.ItemsAreas.Add(item1Area1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllowedAreasAsync(item1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(
                actionResult.Result,
                typeof(OkObjectResult),
                "Operation result should be Ok");
            var result = (IEnumerable<AllowedItemArea>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(
                1,
                result.Count(),
                "Wrong number of returned objects");
            Assert.IsTrue(
                result
                    .FirstOrDefault(x => x.Id == this.Area1.Id)
                    .Policies
                    .FirstOrDefault(x => x.Name == nameof(CrudPolicies.Delete))
                    .IsAllowed,
                "Delete operation for record should be allowed");

            #endregion
        }

        [TestMethod]
        public async Task GetAllowedAreasAsync_DeletePolicyNotAllowedStockNotAvailable()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new Common.DataModels.Item { Id = 1, Code = "Item #1" };
            var item1Area1 = new Common.DataModels.ItemArea { ItemId = item1.Id, AreaId = this.Area1.Id };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1.Id,
                Stock = 0,
                IsItemPairingFixed = true,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.ItemsAreas.Add(item1Area1);
                context.Compartments.Add(compartment1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllowedAreasAsync(item1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(
                actionResult.Result,
                typeof(OkObjectResult),
                "Operation result should be Ok");
            var result = (IEnumerable<AllowedItemArea>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(
                1,
                result.Count(),
                "Wrong number of returned objects");
            Assert.IsFalse(
                result
                    .FirstOrDefault(x => x.Id == this.Area1.Id)
                    .Policies
                    .FirstOrDefault(x => x.Name == nameof(CrudPolicies.Delete))
                    .IsAllowed,
                "Delete operation for record should be denied");

            #endregion
        }

        [TestMethod]
        public async Task GetAllowedAreasAsync_DeletePolicyNotAllowedStockAvailable()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new Common.DataModels.Item { Id = 1, Code = "Item #1" };
            var item1Area1 = new Common.DataModels.ItemArea { ItemId = item1.Id, AreaId = this.Area1.Id };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                ItemId = item1.Id,
                LoadingUnitId = this.LoadingUnit1.Id,
                Stock = 10,
                IsItemPairingFixed = false,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.ItemsAreas.Add(item1Area1);
                context.Compartments.Add(compartment1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllowedAreasAsync(item1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(
                actionResult.Result,
                typeof(OkObjectResult),
                "Operation result should be Ok");
            var result = (IEnumerable<AllowedItemArea>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(
                1,
                result.Count(),
                "Wrong number of returned objects");
            Assert.IsFalse(
                result
                    .FirstOrDefault(x => x.Id == this.Area1.Id)
                    .Policies
                    .FirstOrDefault(x => x.Name == nameof(CrudPolicies.Delete))
                    .IsAllowed,
                "Delete operation for record should be denied");

            #endregion
        }

        [TestMethod]
        public async Task GetAllowedAreasAsync_ItemNotPresent()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new Common.DataModels.Item { Id = 1, Code = "Item #1" };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllowedAreasAsync(999);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(
                actionResult.Result,
                typeof(OkObjectResult),
                "Operation result should be Ok");

            #endregion
        }

        [TestMethod]
        public async Task GetAllowedAreasAsync_ItemWithAssociations()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new Common.DataModels.Item { Id = 1, Code = "Item #1" };
            var item1Area1 = new Common.DataModels.ItemArea { ItemId = item1.Id, AreaId = this.Area1.Id };
            var item1Area2 = new Common.DataModels.ItemArea { ItemId = item1.Id, AreaId = this.Area2.Id };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.ItemsAreas.Add(item1Area1);
                context.ItemsAreas.Add(item1Area2);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllowedAreasAsync(item1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(
                actionResult.Result,
                typeof(OkObjectResult),
                "Operation result should be Ok");
            var result = (IEnumerable<AllowedItemArea>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(
                2,
                result.Count(),
                "Wrong number of returned objects");
            Assert.IsTrue(result.Any(x => x.Id == this.Area1.Id));
            Assert.IsTrue(result.Any(x => x.Id == this.Area2.Id));

            #endregion
        }

        [TestMethod]
        public async Task GetAllowedAreasAsync_ItemWithoutAssociations()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new Common.DataModels.Item { Id = 1, Code = "Item #1" };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllowedAreasAsync(item1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(
                actionResult.Result,
                typeof(OkObjectResult),
                "Operation result should be Ok");
            var result = (IEnumerable<AllowedItemArea>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(
                0,
                result.Count(),
                "Wrong number of returned objects");

            #endregion
        }

        #endregion
    }
}

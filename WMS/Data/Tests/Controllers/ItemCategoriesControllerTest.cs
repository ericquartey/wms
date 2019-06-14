using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    [TestClass]
    public class ItemCategoriesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAllAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();

            var itemCategory1 = new Common.DataModels.ItemCategory { Id = 1, Description = "Item Category #1" };
            var itemCategory2 = new Common.DataModels.ItemCategory { Id = 2, Description = "Item Category #2" };
            var itemCategory3 = new Common.DataModels.ItemCategory { Id = 3, Description = "Item Category #3" };
            var itemCategory4 = new Common.DataModels.ItemCategory { Id = 4, Description = "Item Category #4" };
            using (var context = this.CreateContext())
            {
                context.ItemCategories.Add(itemCategory1);
                context.ItemCategories.Add(itemCategory2);
                context.ItemCategories.Add(itemCategory3);
                context.ItemCategories.Add(itemCategory4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (IEnumerable<ItemCategory>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(4, result.Count());

            #endregion
        }

        [TestMethod]
        public async Task GetAllAsync_NotFound()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetAllAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (IEnumerable<ItemCategory>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(0, result.Count());

            #endregion
        }

        [TestMethod]
        public async Task GetAllCountAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();

            var itemCategory1 = new Common.DataModels.ItemCategory { Id = 1, Description = "Item Category #1" };
            var itemCategory2 = new Common.DataModels.ItemCategory { Id = 2, Description = "Item Category #2" };
            var itemCategory3 = new Common.DataModels.ItemCategory { Id = 3, Description = "Item Category #3" };
            var itemCategory4 = new Common.DataModels.ItemCategory { Id = 4, Description = "Item Category #4" };
            using (var context = this.CreateContext())
            {
                context.ItemCategories.Add(itemCategory1);
                context.ItemCategories.Add(itemCategory2);
                context.ItemCategories.Add(itemCategory3);
                context.ItemCategories.Add(itemCategory4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllCountAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (int)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(4, result);

            #endregion
        }

        [TestMethod]
        public async Task GetAllCountAsync_NotFound()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetAllCountAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (int)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(0, result);

            #endregion
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public async Task GetByIdAsync_Found(int itemCategoryId)
        {
            #region Arrange

            var controller = this.MockController();

            var itemCategory1 = new Common.DataModels.ItemCategory { Id = 1, Description = "Item Category #1" };
            var itemCategory2 = new Common.DataModels.ItemCategory { Id = 2, Description = "Item Category #2" };
            var itemCategory3 = new Common.DataModels.ItemCategory { Id = 3, Description = "Item Category #3" };
            var itemCategory4 = new Common.DataModels.ItemCategory { Id = 4, Description = "Item Category #4" };
            using (var context = this.CreateContext())
            {
                context.ItemCategories.Add(itemCategory1);
                context.ItemCategories.Add(itemCategory2);
                context.ItemCategories.Add(itemCategory3);
                context.ItemCategories.Add(itemCategory4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(itemCategoryId);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (ItemCategory)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(itemCategoryId, result.Id);

            #endregion
        }

        [TestMethod]
        public async Task GetByIdAsync_NotFound()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(1);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult), GetDescription(actionResult.Result));

            #endregion
        }

        private ItemCategoriesController MockController()
        {
            return new ItemCategoriesController(
                new Mock<ILogger<ItemCategoriesController>>().Object,
                this.ServiceProvider.GetService(typeof(IItemCategoryProvider)) as IItemCategoryProvider);
        }

        #endregion
    }
}

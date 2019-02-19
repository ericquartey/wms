using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public class ItemCategoriesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestCleanup]
        public void Cleanup()
        {
            this.CleanupDatabase();
        }

        [TestMethod]
        public async Task GetAllCountFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();

                var itemCategory1 = new Common.DataModels.ItemCategory { Id = 1, Description = "Item Category #1" };
                var itemCategory2 = new Common.DataModels.ItemCategory { Id = 2, Description = "Item Category #2" };
                var itemCategory3 = new Common.DataModels.ItemCategory { Id = 3, Description = "Item Category #3" };
                var itemCategory4 = new Common.DataModels.ItemCategory { Id = 4, Description = "Item Category #4" };
                context.ItemCategories.Add(itemCategory1);
                context.ItemCategories.Add(itemCategory2);
                context.ItemCategories.Add(itemCategory3);
                context.ItemCategories.Add(itemCategory4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAllCountAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (int)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(4, result);

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAllCountNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();

                #endregion

                #region Act

                var actionResult = await controller.GetAllCountAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (int)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(0, result);

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAllFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();

                var itemCategory1 = new Common.DataModels.ItemCategory { Id = 1, Description = "Item Category #1" };
                var itemCategory2 = new Common.DataModels.ItemCategory { Id = 2, Description = "Item Category #2" };
                var itemCategory3 = new Common.DataModels.ItemCategory { Id = 3, Description = "Item Category #3" };
                var itemCategory4 = new Common.DataModels.ItemCategory { Id = 4, Description = "Item Category #4" };
                context.ItemCategories.Add(itemCategory1);
                context.ItemCategories.Add(itemCategory2);
                context.ItemCategories.Add(itemCategory3);
                context.ItemCategories.Add(itemCategory4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<ItemCategory>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(4, result.Count());

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAllNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<ItemCategory>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(0, result.Count());

                #endregion
            }
        }

        [TestMethod]
        public async Task GetByIdFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();

                var itemCategory1 = new Common.DataModels.ItemCategory { Id = 1, Description = "Item Category #1" };
                var itemCategory2 = new Common.DataModels.ItemCategory { Id = 2, Description = "Item Category #2" };
                var itemCategory3 = new Common.DataModels.ItemCategory { Id = 3, Description = "Item Category #3" };
                var itemCategory4 = new Common.DataModels.ItemCategory { Id = 4, Description = "Item Category #4" };
                context.ItemCategories.Add(itemCategory1);
                context.ItemCategories.Add(itemCategory2);
                context.ItemCategories.Add(itemCategory3);
                context.ItemCategories.Add(itemCategory4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult1 = await controller.GetByIdAsync(1);
                var actionResult2 = await controller.GetByIdAsync(2);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (ItemCategory)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual(1, result1.Id);

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = (ItemCategory)((OkObjectResult)actionResult2.Result).Value;
                Assert.AreEqual(2, result2.Id);

                #endregion
            }
        }

        [TestMethod]
        public async Task GetByIdNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();

                #endregion

                #region Act

                var actionResult = await controller.GetByIdAsync(1);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

                #endregion
            }
        }

        private ItemCategoriesController MockController()
        {
            var logger = new Mock<ILogger<ItemCategoriesController>>().Object;

            var itemCategoryProvider = this.ServiceProvider.GetService(typeof(IItemCategoryProvider)) as IItemCategoryProvider;

            return new ItemCategoriesController(logger, itemCategoryProvider);
        }

        #endregion
    }
}

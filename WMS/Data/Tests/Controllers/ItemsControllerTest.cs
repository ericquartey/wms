using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DataModels = Ferretto.Common.DataModels;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    [TestClass]
    public partial class ItemsControllerTest : BaseControllerTest
    {
        #region Methods

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        [TestMethod]
        public async Task UpdateAsync_BadItemId()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };

            ItemDetails existingModel;

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.SaveChanges();

                var getModelResult = await controller.GetByIdAsync(item1.Id);
                existingModel = (ItemDetails)((OkObjectResult)getModelResult.Result).Value;
            }

            var newModelCode = $"{item1.Code} modified";

            existingModel.Code = newModelCode;

            #endregion

            #region Act

            var actionResult = await controller.UpdateAsync(existingModel, existingModel.Id + 1);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(
                actionResult.Result,
                typeof(BadRequestResult),
                "Server response should be 400 Bad Request");

            #endregion
        }

        [TestMethod]
        public async Task UpdateAsync_NotExistingItemId()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };

            ItemDetails existingModel;

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.SaveChanges();

                var getModelResult = await controller.GetByIdAsync(item1.Id);
                existingModel = (ItemDetails)((OkObjectResult)getModelResult.Result).Value;
            }

            #endregion

            #region Act

            var newModelCode = $"{item1.Code} modified";
            existingModel.Id = item1.Id + 1;
            existingModel.Code = newModelCode;
            var actionResult = await controller.UpdateAsync(existingModel, existingModel.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(
                actionResult.Result,
                typeof(NotFoundObjectResult),
                "Server response should be 404 Not Found");

            #endregion
        }

        [TestMethod]
        public async Task UpdateAsync_UpdateItemCode()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };

            ItemDetails existingModel;

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.SaveChanges();

                var getModelResult = await controller.GetByIdAsync(item1.Id);
                existingModel = (ItemDetails)((OkObjectResult)getModelResult.Result).Value;
            }

            #endregion

            #region Act

            var newModelCode = $"{item1.Code} modified";
            existingModel.Code = newModelCode;
            var actionResult = await controller.UpdateAsync(existingModel, existingModel.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(
                actionResult.Result,
                typeof(OkObjectResult),
                "Server response should be 200 Ok");

            var result = (ItemDetails)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(
                newModelCode,
                result.Code,
                "Returned value should be equal of the value to be saved");

            using (var context = this.CreateContext())
            {
                var dataModel = context.Items.Find(item1.Id);
                Assert.AreEqual(
                    newModelCode,
                    dataModel.Code,
                    "DB value should be equal of the value to be saved");
            }

            #endregion
        }

        private ItemsController MockController()
        {
            return new ItemsController(
                this.ServiceProvider.GetService(typeof(IItemProvider)) as IItemProvider,
                this.ServiceProvider.GetService(typeof(IAreaProvider)) as IAreaProvider,
                this.ServiceProvider.GetService(typeof(IItemAreaProvider)) as IItemAreaProvider,
                this.ServiceProvider.GetService(typeof(ICompartmentProvider)) as ICompartmentProvider,
                this.ServiceProvider.GetService(typeof(IItemCompartmentTypeProvider)) as IItemCompartmentTypeProvider,
                this.ServiceProvider.GetService(typeof(ISchedulerService)) as ISchedulerService,
                this.ServiceProvider.GetService(typeof(INotificationService)) as INotificationService);
        }

        #endregion
    }
}

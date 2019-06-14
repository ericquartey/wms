using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataModels = Ferretto.Common.DataModels;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    [TestClass]
    public partial class ItemsControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        [DataRow(100, 1, "S1", null, null, 100)]
        [DataRow(75, 1, null, "S2", null, 125)]
        [DataRow(25, 1, "S1", "S2", null, 75)]
        [DataRow(25, 1, null, null, "AAA", 75)]
        [DataRow(25, 2, null, null, null, 35)]
        public async Task GetPickAvailabilityAsync(
            int requestedQuantity, int itemId, string sub1, string sub2, string lot, double outQuantity)
        {
            #region Arrange

            var controller = this.MockController();

            var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };
            var item2 = new DataModels.Item { Id = 2, Code = "Item #2" };

            var compartmentType = new DataModels.CompartmentType { Id = 1 };
            var itemCompartmentType = new DataModels.ItemCompartmentType { ItemId = 1, MaxCapacity = 100, CompartmentTypeId = 1 };

            var compartment1 = new DataModels.Compartment
            {
                ItemId = item1.Id,
                Stock = 25,
                Sub1 = "S1",
                Lot = "AAA",
                LoadingUnitId = this.LoadingUnit1.Id,
                CompartmentTypeId = 1,
            };
            var compartment2 = new DataModels.Compartment
            {
                ItemId = item1.Id,
                Stock = 50,
                Sub2 = "S2",
                Lot = "AAA",
                LoadingUnitId = this.LoadingUnit1.Id,
                CompartmentTypeId = 1,
            };
            var compartment3 = new DataModels.Compartment
            {
                ItemId = item1.Id,
                Stock = 75,
                Sub1 = "S1",
                Sub2 = "S2",
                Lot = "BBB",
                LoadingUnitId = this.LoadingUnit1.Id,
                CompartmentTypeId = 1,
            };
            var compartment4 = new DataModels.Compartment
            {
                ItemId = item2.Id,
                Stock = 50,
                Sub1 = "S1",
                Sub2 = "S2",
                LoadingUnitId = this.LoadingUnit1.Id,
                CompartmentTypeId = 1,
                ReservedForPick = 25,
                ReservedToPut = 10,
            };
            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.Items.Add(item2);
                context.CompartmentTypes.Add(compartmentType);
                context.ItemsCompartmentTypes.Add(itemCompartmentType);
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.Compartments.Add(compartment3);
                context.Compartments.Add(compartment4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var itemOption = new ItemOptions
            {
                AreaId = this.Area1.Id,
                Lot = lot,
                Sub1 = sub1,
                Sub2 = sub2,
                RequestedQuantity = requestedQuantity,
            };
            var answer = await controller.GetPickAvailabilityAsync(itemId, itemOption);

            #endregion

            #region Assert

            Assert.AreNotEqual(null, answer.Result);
            var value = ((ObjectResult)answer.Result).Value;
            Assert.AreEqual(outQuantity, value);

            #endregion
        }

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
                this.ServiceProvider.GetService(typeof(ISchedulerService)) as ISchedulerService);
        }

        #endregion
    }
}

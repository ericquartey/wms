using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
                var compartment1 = new DataModels.Compartment { Id = 1, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 10, ReservedToPut = 0 };

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
                var compartment1 = new DataModels.Compartment { Id = 1, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToPut = 0 };
                var compartment2 = new DataModels.Compartment { Id = 2, LoadingUnitId = this.LoadingUnit3.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToPut = 0 };

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
                var compartment1 = new DataModels.Compartment { Id = 1, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToPut = 0 };
                var compartment2 = new DataModels.Compartment { Id = 2, LoadingUnitId = this.LoadingUnit2.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToPut = 0 };

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

        [TestMethod]
        public async Task TryUpdateBadItemId()
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
        public async Task TryUpdateNotExistingItemId()
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
        public async Task PickAvailableItem()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item
            {
                Id = 1,
                Code = "Item #1",
                ManagementType = DataModels.ItemManagementType.Volume,
            };
            var compartment1 = new DataModels.Compartment
            {
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item1.Id,
                Stock = 10,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.Compartments.Add(compartment1);
                context.SaveChanges();
            }

            var itemOptions = new ItemOptions
            {
                AreaId = this.Area1.Id,
                BayId = this.Bay1.Id,
                RunImmediately = true,
                RequestedQuantity = 5,
            };

            #endregion

            #region Act

            var actionResult = await controller.PickAsync(item1.Id, itemOptions);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            #endregion
        }

        [TestMethod]
        public async Task PickNotAvailableItem()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item
            {
                Id = 1,
                Code = "Item #1",
                ManagementType = DataModels.ItemManagementType.Volume,
            };
            var compartment1 = new DataModels.Compartment
            {
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                ItemId = item1.Id,
                Stock = 0,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.Compartments.Add(compartment1);
                context.SaveChanges();
            }

            var itemOptions = new ItemOptions
            {
                AreaId = this.Area1.Id,
                BayId = this.Bay1.Id,
                RunImmediately = true,
                RequestedQuantity = 5,
            };

            #endregion

            #region Act

            var actionResult = await controller.PickAsync(item1.Id, itemOptions);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));

            #endregion
        }

        [TestMethod]
        public async Task PutItemWithCompartmentType()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item
            {
                Id = 1,
                Code = "Item #1",
                ManagementType = DataModels.ItemManagementType.Volume,
            };
            var compartmentType1 = new DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var compartment1 = new DataModels.Compartment
            {
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                CompartmentTypeId = compartmentType1.Id,
                ItemId = item1.Id,
                Stock = 10,
            };
            var itemCompartmentType1 = new DataModels.ItemCompartmentType
            {
                ItemId = item1.Id,
                CompartmentTypeId = compartmentType1.Id,
                MaxCapacity = 100,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.Compartments.Add(compartment1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.SaveChanges();
            }

            var itemOptions = new ItemOptions
            {
                AreaId = this.Area1.Id,
                BayId = this.Bay1.Id,
                RunImmediately = true,
                RequestedQuantity = 5,
            };

            #endregion

            #region Act

            var actionResult = await controller.PutAsync(item1.Id, itemOptions);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            #endregion
        }

        [TestMethod]
        public async Task PutItemWithoutCompartmentType()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item
            {
                Id = 1,
                Code = "Item #1",
                ManagementType = DataModels.ItemManagementType.Volume,
            };
            var compartmentType1 = new DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var compartment1 = new DataModels.Compartment
            {
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                CompartmentTypeId = compartmentType1.Id,
                ItemId = item1.Id,
                Stock = 10,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.Compartments.Add(compartment1);
                context.SaveChanges();
            }

            var itemOptions = new ItemOptions
            {
                AreaId = this.Area1.Id,
                BayId = this.Bay1.Id,
                RunImmediately = true,
                RequestedQuantity = 5,
            };

            #endregion

            #region Act

            var actionResult = await controller.PutAsync(item1.Id, itemOptions);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));

            #endregion
        }

        [TestMethod]
        public async Task UpdateItemCode()
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
                new Mock<ILogger<ItemsController>>().Object,
                new Mock<IHubContext<DataHub, IDataHub>>().Object,
                this.ServiceProvider.GetService(typeof(IItemProvider)) as IItemProvider,
                this.ServiceProvider.GetService(typeof(IAreaProvider)) as IAreaProvider,
                this.ServiceProvider.GetService(typeof(ICompartmentProvider)) as ICompartmentProvider,
                this.ServiceProvider.GetService(typeof(IItemCompartmentTypeProvider)) as IItemCompartmentTypeProvider,
                this.ServiceProvider.GetService(typeof(ISchedulerService)) as ISchedulerService);
        }

        #endregion
    }
}

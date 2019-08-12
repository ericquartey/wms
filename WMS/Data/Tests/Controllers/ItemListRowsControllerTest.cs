using System;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DataModels = Ferretto.Common.DataModels;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    [TestClass]
    public partial class ItemListRowsControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        [DataRow(Enums.ItemListRowStatus.New, typeof(CreatedAtActionResult))]
        [DataRow(Enums.ItemListRowStatus.Executing, typeof(BadRequestObjectResult))]
        [DataRow(Enums.ItemListRowStatus.Suspended, typeof(BadRequestObjectResult))]
        [DataRow(Enums.ItemListRowStatus.Waiting, typeof(BadRequestObjectResult))]
        [DataRow(Enums.ItemListRowStatus.Completed, typeof(BadRequestObjectResult))]
        [DataRow(Enums.ItemListRowStatus.Error, typeof(BadRequestObjectResult))]
        [DataRow(Enums.ItemListRowStatus.Incomplete, typeof(BadRequestObjectResult))]
        public async Task CreateAsync_WhenListIsInStatus(Enums.ItemListRowStatus rowStatus, Type resultType)
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };
            var item2 = new DataModels.Item { Id = 2, Code = "Item #2" };
            var compartment1 = new DataModels.Compartment
            { Id = 1, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item1.Id, Stock = 10 };
            var compartment2 = new DataModels.Compartment
            { Id = 2, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item2.Id, Stock = 10 };
            var list1Id = 1;

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = 10, // InMemoryDatabase does not handle autoincrement fields
                ItemId = item1.Id,
                RequestedQuantity = 10,
                ItemListId = list1Id,
                Status = rowStatus,
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = list1Id,
                ItemListRows = new[] { row1 },
            };

            var row2 = new ItemListRowDetails { ItemId = 2, ItemListId = list1Id, RequestedQuantity = 10 };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.Items.Add(item2);
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);
                context.ItemLists.Add(list1);
                context.ItemListRows.Add(row1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.CreateAsync(row2);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, resultType, GetDescription(actionResult.Result));

            #endregion
        }

        [TestMethod]
        [DataRow(Enums.ItemListRowStatus.New, typeof(OkResult))]
        [DataRow(Enums.ItemListRowStatus.Error, typeof(OkResult))]
        [DataRow(Enums.ItemListRowStatus.Incomplete, typeof(OkResult))]
        [DataRow(Enums.ItemListRowStatus.Suspended, typeof(OkResult))]
        [DataRow(Enums.ItemListRowStatus.Executing, typeof(BadRequestObjectResult))]
        [DataRow(Enums.ItemListRowStatus.Completed, typeof(BadRequestObjectResult))]
        [DataRow(Enums.ItemListRowStatus.Waiting, typeof(OkResult))]
        [DataRow(Enums.ItemListRowStatus.Ready, typeof(BadRequestObjectResult))]
        public async Task ExecuteAsync_InStatus(Enums.ItemListRowStatus rowStatus, Type resultType)
        {
            #region Arrange

            var controller = this.MockController();

            var item1 = new DataModels.Item
            {
                Id = 1,
                Code = "Item #1",
                ManagementType = Enums.ItemManagementType.Volume,
            };

            var itemArea1 = new DataModels.ItemArea
            {
                ItemId = 1,
                AreaId = this.Area1.Id,
            };

            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Depth = 10,
                Width = 10,
            };

            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
            {
                ItemId = item1.Id,
                CompartmentTypeId = compartmentType1.Id,
                MaxCapacity = 100,
            };

            var compartment1 = new DataModels.Compartment
            {
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                CompartmentTypeId = compartmentType1.Id,
                ItemId = item1.Id,
                Stock = 10,
            };

            var list1Id = 1;

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = 1, // InMemoryDatabase does not handle autoincrement fields
                ItemId = item1.Id,
                RequestedQuantity = 10,
                ItemListId = list1Id,
                Status = rowStatus,
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = list1Id,
                ItemListType = Enums.ItemListType.Put,
                ItemListRows = new[] { row1 },
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.ItemsAreas.Add(itemArea1);
                context.CompartmentTypes.Add(compartmentType1);
                context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                context.Compartments.Add(compartment1);
                context.ItemLists.Add(list1);
                context.ItemListRows.Add(row1);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.ExecuteAsync(row1.Id, this.Area1.Id);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult, resultType, GetDescription(actionResult));

            #endregion
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        private ItemListRowsController MockController()
        {
            var controller = new ItemListRowsController(
                new Mock<ILogger<ItemListRowsController>>().Object,
                this.ServiceProvider.GetService(typeof(IItemListSchedulerService)) as IItemListSchedulerService,
                this.ServiceProvider.GetService(typeof(IItemListRowProvider)) as IItemListRowProvider)
            {
                ControllerContext = new Mock<ControllerContext>().Object,
            };

            return controller;
        }

        #endregion
    }
}

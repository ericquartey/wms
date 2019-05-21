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
    public class ItemListRowsControllerTest : BaseControllerTest
    {
        #region Methods

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        [TestMethod]
        public async Task AddRowWhenListIsInNewStatus()
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
                Status = Common.DataModels.ItemListRowStatus.New,
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = list1Id,
                ItemListRows = new[] { row1 }
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

            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));

            #endregion
        }

        [TestMethod]
        public async Task AddRowWhenListIsInExecutingStatus()
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
                Status = Common.DataModels.ItemListRowStatus.Executing,
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = list1Id,
                ItemListRows = new[] { row1 }
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

            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));

            #endregion
        }

        [TestMethod]
        public async Task ExecuteListRowInNewStatus()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item { Id = 1, Code = "Item #1", ManagementType = DataModels.ItemManagementType.Volume };
            var itemArea1 = new DataModels.ItemArea { ItemId = 1, AreaId = this.Area1.Id };
            var compartment1 = new DataModels.Compartment
                { Id = 1, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item1.Id, Stock = 10 };
            var list1Id = 1;

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = 1, // InMemoryDatabase does not handle autoincrement fields
                ItemId = item1.Id,
                RequestedQuantity = 10,
                ItemListId = list1Id,
                Status = DataModels.ItemListRowStatus.New,
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = list1Id,
                ItemListRows = new[] { row1 }
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.ItemsAreas.Add(itemArea1);
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

            Assert.IsInstanceOfType(actionResult, typeof(OkResult));

            #endregion
        }

        [TestMethod]
        public async Task ExecuteListRowInExecutingStatus()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item { Id = 1, Code = "Item #1", ManagementType = DataModels.ItemManagementType.Volume };
            var itemArea1 = new DataModels.ItemArea { ItemId = 1, AreaId = this.Area1.Id };
            var compartment1 = new DataModels.Compartment
                { Id = 1, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item1.Id, Stock = 10 };
            var list1Id = 1;

            var row1 = new Common.DataModels.ItemListRow
            {
                Id = 1, // InMemoryDatabase does not handle autoincrement fields
                ItemId = item1.Id,
                RequestedQuantity = 10,
                ItemListId = list1Id,
                Status = DataModels.ItemListRowStatus.Executing,
            };

            var list1 = new Common.DataModels.ItemList
            {
                Id = list1Id,
                ItemListRows = new[] { row1 }
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.ItemsAreas.Add(itemArea1);
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

            Assert.IsInstanceOfType(actionResult, typeof(UnprocessableEntityObjectResult));

            #endregion
        }

        private ItemListRowsController MockController()
        {
            var controller = new ItemListRowsController(
                new Mock<ILogger<ItemListRowsController>>().Object,
                new Mock<IHubContext<DataHub, IDataHub>>().Object,
                this.ServiceProvider.GetService(typeof(ISchedulerService)) as ISchedulerService,
                this.ServiceProvider.GetService(typeof(IItemListRowProvider)) as IItemListRowProvider)
            {
                ControllerContext = new Mock<ControllerContext>().Object
            };

            return controller;
        }

        #endregion
    }
}

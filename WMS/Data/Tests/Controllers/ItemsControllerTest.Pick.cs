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
        public async Task PickAsync_AvailableItem()
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
        public async Task PickAsync_NotAvailableItem()
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

        #endregion
    }
}

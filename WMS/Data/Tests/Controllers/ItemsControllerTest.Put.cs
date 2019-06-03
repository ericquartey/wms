using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.Tests
{
    public partial class ItemsControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task PutAsync_ItemWithCompartmentType()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new Common.DataModels.Item
            {
                Id = 1,
                Code = "Item #1",
                ManagementType = Common.DataModels.ItemManagementType.Volume,
            };
            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var compartment1 = new Common.DataModels.Compartment
            {
                Id = 1,
                LoadingUnitId = this.LoadingUnit1.Id,
                CompartmentTypeId = compartmentType1.Id,
                ItemId = item1.Id,
                Stock = 10,
            };
            var itemCompartmentType1 = new Common.DataModels.ItemCompartmentType
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
        public async Task PutAsync_ItemWithoutCompartmentType()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new Common.DataModels.Item
            {
                Id = 1,
                Code = "Item #1",
                ManagementType = Common.DataModels.ItemManagementType.Volume,
            };
            var compartmentType1 = new Common.DataModels.CompartmentType
            {
                Id = 1,
                Height = 10,
                Width = 10,
            };
            var compartment1 = new Common.DataModels.Compartment
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

        #endregion
    }
}

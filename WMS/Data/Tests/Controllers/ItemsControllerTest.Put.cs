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
        [DataRow(false, false, typeof(BadRequestObjectResult))]
        [DataRow(false, true, typeof(BadRequestObjectResult))]
        [DataRow(true, false, typeof(BadRequestObjectResult))]
        [DataRow(true, true, typeof(CreatedAtActionResult))]
        public async Task PutAsync_ItemWithCompartmentType(
            bool hasCompartmentType,
            bool hasItemArea,
            System.Type outResultType)
        {
            #region Arrange

            var controller = this.MockController();

            var item1 = new DataModels.Item
            {
                Id = GetNewId(),
                Code = "Item #1",
                ManagementType = DataModels.ItemManagementType.Volume,
            };

            var compartmentType1 = new DataModels.CompartmentType
            {
                Id = GetNewId(),
                Depth = 10,
                Width = 10,
            };

            var compartment1 = new DataModels.Compartment
            {
                Id = GetNewId(),
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

            var itemArea1 = new DataModels.ItemArea
            {
                AreaId = this.Area1.Id,
                ItemId = item1.Id,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);
                context.CompartmentTypes.Add(compartmentType1);
                context.Compartments.Add(compartment1);

                if (hasCompartmentType == true)
                {
                    context.ItemsCompartmentTypes.Add(itemCompartmentType1);
                }

                if (hasItemArea == true)
                {
                    context.ItemsAreas.Add(itemArea1);
                }

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

            Assert.IsInstanceOfType(actionResult.Result, outResultType, GetDescription(actionResult.Result));

            #endregion
        }

        #endregion
    }
}

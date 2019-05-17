using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DataModels = Ferretto.Common.DataModels;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public class ItemsControllerCompartmentTypesTest : BaseControllerTest
    {
        #region Methods

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        [TestMethod]
        public async Task DeleteItemCompartmentType()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };
            DataModels.CompartmentType[] compartmentTypes =
            {
                new DataModels.CompartmentType { Id = 1, Height = 1, Width = 2 },
                new DataModels.CompartmentType { Id = 2, Height = 3, Width = 4 },
                new DataModels.CompartmentType { Id = 3, Height = 5, Width = 6 },
                new DataModels.CompartmentType { Id = 4, Height = 7, Width = 8 },
                new DataModels.CompartmentType { Id = 5, Height = 9, Width = 10 },
            };
            DataModels.ItemCompartmentType[] itemCompartmentTypes =
            {
                new DataModels.ItemCompartmentType { ItemId = 1, CompartmentTypeId = 1, MaxCapacity = 10 },
                new DataModels.ItemCompartmentType { ItemId = 1, CompartmentTypeId = 2, MaxCapacity = 10 },
                new DataModels.ItemCompartmentType { ItemId = 1, CompartmentTypeId = 3, MaxCapacity = 10 },
                new DataModels.ItemCompartmentType { ItemId = 1, CompartmentTypeId = 4, MaxCapacity = 10 },
                new DataModels.ItemCompartmentType { ItemId = 1, CompartmentTypeId = 5, MaxCapacity = 10 },
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);

                foreach (var compartmentType in compartmentTypes)
                {
                    context.CompartmentTypes.Add(compartmentType);
                }

                foreach (var itemCompartmentType in itemCompartmentTypes)
                {
                    context.ItemsCompartmentTypes.Add(itemCompartmentType);
                }

                context.SaveChanges();
            }

            #endregion

            #region Act

            const int itemIdToDelete = 1;
            const int compartmentTypeIdToDelete = 2;
            var actionResult = await controller.DeleteCompartmentTypeAssociationAsync(
                itemIdToDelete,
                compartmentTypeIdToDelete);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(
                actionResult.Result,
                typeof(OkObjectResult),
                "Server response should be 200 Ok");

            using (var context = this.CreateContext())
            {
                var found = context.ItemsCompartmentTypes.Where(
                        ict => ict.ItemId == itemIdToDelete && ict.CompartmentTypeId == compartmentTypeIdToDelete)
                    .ToArray();
                Assert.AreEqual(
                    0,
                    found.Length,
                    "No DB records should be found");
            }

            #endregion
        }

        [TestMethod]
        public async Task PatchItemCompartmentType()
        {
            #region Arrange

            var controller = this.MockController();
            var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };
            DataModels.CompartmentType[] compartmentTypes =
            {
                new DataModels.CompartmentType { Id = 1, Height = 1, Width = 2 },
            };
            DataModels.ItemCompartmentType[] itemCompartmentTypes =
            {
                new DataModels.ItemCompartmentType { ItemId = 1, CompartmentTypeId = 1, MaxCapacity = 10 },
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(item1);

                foreach (var compartmentType in compartmentTypes)
                {
                    context.CompartmentTypes.Add(compartmentType);
                }

                foreach (var itemCompartmentType in itemCompartmentTypes)
                {
                    context.ItemsCompartmentTypes.Add(itemCompartmentType);
                }

                context.SaveChanges();
            }

            #endregion

            #region Act

            const int itemIdToUpdate = 1;
            const int compartmentTypeIdToUpdate = 1;
            const int newMaxCapacity = 20;
            var actionResult = await controller.UpdateCompartmentTypeAssociationAsync(
                itemIdToUpdate,
                compartmentTypeIdToUpdate,
                newMaxCapacity);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(
                actionResult.Result,
                typeof(OkObjectResult),
                "Server response should be 200 Ok");

            using (var context = this.CreateContext())
            {
                var found = context.ItemsCompartmentTypes
                    .First(ict => ict.ItemId == itemIdToUpdate && ict.CompartmentTypeId == compartmentTypeIdToUpdate);
                Assert.AreEqual(
                    newMaxCapacity,
                    found.MaxCapacity,
                    "MaxCapacity DB value has not been updated");
            }

            #endregion
        }

        private ItemsController MockController()
        {
            return new ItemsController(
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

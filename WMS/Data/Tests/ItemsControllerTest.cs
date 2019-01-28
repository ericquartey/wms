using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.WebAPI.Controllers;
using Ferretto.WMS.Data.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DataModels = Ferretto.Common.DataModels;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public class ItemsControllerTest
    {
        #region Properties

        private DataModels.Area Area1 { get; set; }

        private DataModels.Area Area2 { get; set; }

        private DataModels.Aisle Aisle1 { get; set; }

        private DataModels.Aisle Aisle2 { get; set; }

        private DataModels.Cell Cell1 { get; set; }

        private DataModels.Cell Cell2 { get; set; }

        private DataModels.Cell Cell3 { get; set; }

        private DataModels.Cell Cell4 { get; set; }

        private DataModels.LoadingUnit LoadingUnit1 { get; set; }

        private DataModels.LoadingUnit LoadingUnit2 { get; set; }

        private DataModels.LoadingUnit LoadingUnit3 { get; set; }

        private DataModels.LoadingUnit LoadingUnit4 { get; set; }

        #endregion Properties

        #region Methods

        [TestCleanup]
        public void CleanupDatabase()
        {
            using (var context = this.CreateContext())
            {
                context.Database.EnsureDeleted();
            }
        }

        [TestInitialize]
        public void InitializeDatabase()
        {
            this.Area1 = new DataModels.Area { Id = 1, Name = "Area #1" };
            this.Area2 = new DataModels.Area { Id = 2, Name = "Area #2" };
            this.Aisle1 = new DataModels.Aisle { Id = 1, AreaId = this.Area1.Id, Name = "Aisle #1" };
            this.Aisle2 = new DataModels.Aisle { Id = 2, AreaId = this.Area2.Id, Name = "Aisle #2" };
            this.Cell1 = new DataModels.Cell { Id = 1, AisleId = this.Aisle1.Id };
            this.Cell2 = new DataModels.Cell { Id = 2, AisleId = this.Aisle1.Id };
            this.Cell3 = new DataModels.Cell { Id = 3, AisleId = this.Aisle2.Id };
            this.Cell4 = new DataModels.Cell { Id = 4, AisleId = this.Aisle2.Id };
            this.LoadingUnit1 = new DataModels.LoadingUnit { Id = 1, Code = "Loading Unit #1", CellId = this.Cell1.Id };
            this.LoadingUnit2 = new DataModels.LoadingUnit { Id = 2, Code = "Loading Unit #2", CellId = this.Cell2.Id };
            this.LoadingUnit3 = new DataModels.LoadingUnit { Id = 3, Code = "Loading Unit #3", CellId = this.Cell3.Id };
            this.LoadingUnit4 = new DataModels.LoadingUnit { Id = 4, Code = "Loading Unit #4", CellId = this.Cell4.Id };

            using (var context = this.CreateContext())
            {
                context.Areas.Add(this.Area1);
                context.Areas.Add(this.Area2);
                context.Aisles.Add(this.Aisle1);
                context.Aisles.Add(this.Aisle2);
                context.Cells.Add(this.Cell1);
                context.Cells.Add(this.Cell2);
                context.Cells.Add(this.Cell3);
                context.Cells.Add(this.Cell4);
                context.LoadingUnits.Add(this.LoadingUnit1);
                context.LoadingUnits.Add(this.LoadingUnit2);
                context.LoadingUnits.Add(this.LoadingUnit3);
                context.LoadingUnits.Add(this.LoadingUnit4);

                context.SaveChanges();
            }
        }

        [TestMethod]
        public void ItemAvailableInTwoAreas()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var cache = new MemoryCache(new MemoryCacheOptions());
                var warehouse = new Warehouse(cache, context);
                var controller = new ItemsController(
                    new Mock<ILogger<ItemsController>>().Object,
                    warehouse,
                    context);

                var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };
                var compartment1 = new DataModels.Compartment { Id = 1, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToStore = 0 };
                var compartment2 = new DataModels.Compartment { Id = 2, LoadingUnitId = this.LoadingUnit3.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToStore = 0 };

                context.Items.Add(item1);
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);

                context.SaveChanges();

                #endregion Arrange

                #region Act

                var actionResult = controller.GetAreasWithAvailability(item1.Id);

                #endregion Act

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var resultAreas = (IEnumerable<Area>)((OkObjectResult)controller.GetAreasWithAvailability(item1.Id).Result).Value;
                Assert.AreEqual(2, resultAreas.Count());

                #endregion Assert
            }
        }

        [TestMethod]
        public void ItemAvailableTwoTimesInOneArea()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var cache = new MemoryCache(new MemoryCacheOptions());
                var warehouse = new Warehouse(cache, context);
                var controller = new ItemsController(
                    new Mock<ILogger<ItemsController>>().Object,
                    warehouse,
                    context);

                var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };
                var compartment1 = new DataModels.Compartment { Id = 1, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToStore = 0 };
                var compartment2 = new DataModels.Compartment { Id = 2, LoadingUnitId = this.LoadingUnit2.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 0, ReservedToStore = 0 };

                context.Items.Add(item1);
                context.Compartments.Add(compartment1);
                context.Compartments.Add(compartment2);

                context.SaveChanges();

                #endregion Arrange

                #region Act

                var actionResult = controller.GetAreasWithAvailability(item1.Id);

                #endregion Act

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var resultAreas = (IEnumerable<Area>)((OkObjectResult)controller.GetAreasWithAvailability(item1.Id).Result).Value;
                Assert.AreEqual(1, resultAreas.Count());

                #endregion Assert
            }
        }

        [TestMethod]
        public void ItemNotAvailable()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var cache = new MemoryCache(new MemoryCacheOptions());
                var warehouse = new Warehouse(cache, context);
                var controller = new ItemsController(
                    new Mock<ILogger<ItemsController>>().Object,
                    warehouse,
                    context);

                var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };

                context.Items.Add(item1);

                context.SaveChanges();

                #endregion Arrange

                #region Act

                var actionResult = controller.GetAreasWithAvailability(item1.Id);

                #endregion Act

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

                #endregion Assert
            }
        }

        [TestMethod]
        public void ItemAvailableButReserved()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var cache = new MemoryCache(new MemoryCacheOptions());
                var warehouse = new Warehouse(cache, context);
                var controller = new ItemsController(
                    new Mock<ILogger<ItemsController>>().Object,
                    warehouse,
                    context);

                var item1 = new DataModels.Item { Id = 1, Code = "Item #1" };
                var compartment1 = new DataModels.Compartment { Id = 1, LoadingUnitId = this.LoadingUnit1.Id, ItemId = item1.Id, Stock = 10, ReservedForPick = 10, ReservedToStore = 0 };

                context.Items.Add(item1);
                context.Compartments.Add(compartment1);

                context.SaveChanges();

                #endregion Arrange

                #region Act

                var actionResult = controller.GetAreasWithAvailability(item1.Id);

                #endregion Act

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

                #endregion Assert
            }
        }

        private DatabaseContext CreateContext()
        {
            return new DatabaseContext(
                new DbContextOptionsBuilder<DatabaseContext>()
                    .UseInMemoryDatabase(databaseName: this.GetType().FullName)
                    .Options);
        }

        #endregion Methods
    }
}

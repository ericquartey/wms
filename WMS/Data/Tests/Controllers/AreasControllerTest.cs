using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public class AreasControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAislesFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                this.InitializeDatabase();

                #endregion

                #region Act

                var actionResult1 = await controller.GetAisles(this.Area1.Id);
                var actionResult2 = await controller.GetAisles(this.Area2.Id);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (IEnumerable<Aisle>)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual(1, result1.Count());
                Assert.IsNotNull(result1.SingleOrDefault(a => (a.Id == this.Aisle1.Id && a.Name == this.Aisle1.Name &&
                                                               a.AreaId == this.Area1.Id &&
                                                               a.AreaName == this.Area1.Name)));

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = (IEnumerable<Aisle>)((OkObjectResult)actionResult2.Result).Value;
                Assert.AreEqual(2, result2.Count());
                Assert.IsNotNull(result2.SingleOrDefault(a => (a.Id == this.Aisle2.Id && a.Name == this.Aisle2.Name &&
                                                               a.AreaId == this.Area2.Id &&
                                                               a.AreaName == this.Area2.Name)));
                Assert.IsNotNull(result2.SingleOrDefault(a => (a.Id == this.Aisle3.Id && a.Name == this.Aisle3.Name &&
                                                               a.AreaId == this.Area2.Id &&
                                                               a.AreaName == this.Area2.Name)));

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAislesNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                this.InitializeDatabase();

                #endregion

                #region Act

                var actionResult = await controller.GetAisles(this.Area3.Id);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<Aisle>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(0, result.Count());

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAllCountFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                var area1 = new Common.DataModels.Area { Id = 1, Name = "Area #1" };
                var area2 = new Common.DataModels.Area { Id = 2, Name = "Area #2" };
                context.Areas.Add(area1);
                context.Areas.Add(area2);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAllCountAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (int)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(2, result);

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAllCountNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();

                #endregion

                #region Act

                var actionResult = await controller.GetAllCountAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (int)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(0, result);

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAllFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                var area1 = new Common.DataModels.Area { Id = 1, Name = "Area #1" };
                var area2 = new Common.DataModels.Area { Id = 2, Name = "Area #2" };
                var area3 = new Common.DataModels.Area { Id = 3, Name = "Area #3" };
                context.Areas.Add(area1);
                context.Areas.Add(area2);
                context.Areas.Add(area3);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<Area>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(3, result.Count());
                Assert.IsNotNull(result.SingleOrDefault(a => (a.Id == area1.Id && a.Name == area1.Name)));
                Assert.IsNotNull(result.SingleOrDefault(a => (a.Id == area2.Id && a.Name == area2.Name)));
                Assert.IsNotNull(result.SingleOrDefault(a => (a.Id == area3.Id && a.Name == area3.Name)));

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAllNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<Area>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(0, result.Count());

                #endregion
            }
        }

        [TestMethod]
        public async Task GetBaysFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                var area1 = new Common.DataModels.Area { Id = 1, Name = "Area #1" };
                var area2 = new Common.DataModels.Area { Id = 2, Name = "Area #2" };
                var machine1 = new Common.DataModels.Machine { Id = 1, Nickname = "Machine #1" };
                var machine2 = new Common.DataModels.Machine { Id = 2, Nickname = "Machine #2" };
                var bayType1 = new Common.DataModels.BayType { Id = "1", Description = "Bay Type #1" };
                var bayType2 = new Common.DataModels.BayType { Id = "2", Description = "Bay Type #2" };
                var bay1 = new Common.DataModels.Bay
                { Id = 1, Description = "Bay #1", AreaId = 1, MachineId = 1, BayTypeId = "1" };
                var bay2 = new Common.DataModels.Bay
                { Id = 2, Description = "Bay #2", AreaId = 2, MachineId = 2, BayTypeId = "1" };
                var bay3 = new Common.DataModels.Bay
                { Id = 3, Description = "Bay #3", AreaId = 2, MachineId = 2, BayTypeId = "2" };
                context.Areas.Add(area1);
                context.Areas.Add(area2);
                context.Machines.Add(machine1);
                context.Machines.Add(machine2);
                context.BayTypes.Add(bayType1);
                context.BayTypes.Add(bayType2);
                context.Bays.Add(bay1);
                context.Bays.Add(bay2);
                context.Bays.Add(bay3);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult1 = await controller.GetBaysAsync(1);
                var actionResult2 = await controller.GetBaysAsync(2);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (IEnumerable<Bay>)((OkObjectResult)actionResult1.Result).Value;
                Assert.IsNotNull(result1.SingleOrDefault(
                                     b => b.Id == bay1.Id &&
                                          b.Description == bay1.Description &&
                                          b.AreaId == bay1.AreaId &&
                                          b.AreaName == bay1.Area.Name &&
                                          b.MachineId == bay1.MachineId &&
                                          b.MachineNickname == bay1.Machine.Nickname &&
                                          b.BayTypeId == bay1.BayTypeId &&
                                          b.BayTypeDescription == bay1.BayType.Description));

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = (IEnumerable<Bay>)((OkObjectResult)actionResult2.Result).Value;
                Assert.IsNotNull(result2.SingleOrDefault(
                                     b => b.Id == bay2.Id &&
                                          b.Description == bay2.Description &&
                                          b.AreaId == bay2.AreaId &&
                                          b.AreaName == bay2.Area.Name &&
                                          b.MachineId == bay2.MachineId &&
                                          b.MachineNickname == bay2.Machine.Nickname &&
                                          b.BayTypeId == bay2.BayTypeId &&
                                          b.BayTypeDescription == bay2.BayType.Description));
                Assert.IsNotNull(result2.SingleOrDefault(
                                     b => b.Id == bay3.Id &&
                                          b.Description == bay3.Description &&
                                          b.AreaId == bay3.AreaId &&
                                          b.AreaName == bay3.Area.Name &&
                                          b.MachineId == bay3.MachineId &&
                                          b.MachineNickname == bay3.Machine.Nickname &&
                                          b.BayTypeId == bay3.BayTypeId &&
                                          b.BayTypeDescription == bay3.BayType.Description));

                #endregion
            }
        }

        [TestMethod]
        public async Task GetBaysNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                var area1 = new Common.DataModels.Area { Id = 1, Name = "Area #1" };
                context.Areas.Add(area1);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetBaysAsync(1);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

                #endregion
            }
        }

        [TestMethod]
        public async Task GetByIdFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                var area1 = new Common.DataModels.Area { Id = 1, Name = "Area #1" };
                var area2 = new Common.DataModels.Area { Id = 2, Name = "Area #2" };
                var area3 = new Common.DataModels.Area { Id = 3, Name = "Area #3" };
                context.Areas.Add(area1);
                context.Areas.Add(area2);
                context.Areas.Add(area3);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult1 = await controller.GetByIdAsync(1);
                var actionResult2 = await controller.GetByIdAsync(2);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (Area)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual(1, result1.Id);
                Assert.IsTrue(result1.Id == area1.Id && result1.Name == area1.Name);

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = (Area)((OkObjectResult)actionResult2.Result).Value;
                Assert.IsTrue(result2.Id == area2.Id && result2.Name == area2.Name);

                #endregion
            }
        }

        [TestMethod]
        public async Task GetByIdNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();

                #endregion

                #region Act

                var actionResult = await controller.GetByIdAsync(1);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

                #endregion
            }
        }

        [TestMethod]
        public async Task GetCellsFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                this.InitializeDatabase();

                #endregion

                #region Act

                var actionResult1 = await controller.GetCellsAsync(this.Area1.Id);
                var actionResult2 = await controller.GetCellsAsync(this.Area2.Id);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (IEnumerable<Cell>)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual(2, result1.Count());
                Assert.IsNotNull(result1.SingleOrDefault(c => (c.Id == this.Cell1.Id)));
                Assert.IsNotNull(result1.SingleOrDefault(c => (c.Id == this.Cell2.Id)));

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = (IEnumerable<Cell>)((OkObjectResult)actionResult2.Result).Value;
                Assert.AreEqual(4, result2.Count());
                Assert.IsNotNull(result2.SingleOrDefault(c => (c.Id == this.Cell3.Id)));
                Assert.IsNotNull(result2.SingleOrDefault(c => (c.Id == this.Cell4.Id)));
                Assert.IsNotNull(result2.SingleOrDefault(c => (c.Id == this.Cell5.Id)));
                Assert.IsNotNull(result2.SingleOrDefault(c => (c.Id == this.Cell6.Id)));

                #endregion
            }
        }

        [TestMethod]
        public async Task GetCellsNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                this.InitializeDatabase();

                #endregion

                #region Act

                var actionResult = await controller.GetCellsAsync(this.Area3.Id);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<Cell>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(0, result.Count());

                #endregion
            }
        }

        private AreasController MockController()
        {
            return new AreasController(
                new Mock<ILogger<AreasController>>().Object,
                this.ServiceProvider.GetService(typeof(IAreaProvider)) as IAreaProvider,
                this.ServiceProvider.GetService(typeof(IBayProvider)) as IBayProvider,
                this.ServiceProvider.GetService(typeof(ICellProvider)) as ICellProvider);
        }

        #endregion
    }
}

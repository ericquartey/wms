using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    [TestClass]
    public class MeasureUnitsControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAllAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var measureUnit1 = new Common.DataModels.MeasureUnit { Id = "A", Description = "Measure Unit #A" };
            var measureUnit2 = new Common.DataModels.MeasureUnit { Id = "B", Description = "Measure Unit #B" };
            var measureUnit3 = new Common.DataModels.MeasureUnit { Id = "C", Description = "Measure Unit #C" };
            var measureUnit4 = new Common.DataModels.MeasureUnit { Id = "D", Description = "Measure Unit #D" };
            using (var context = this.CreateContext())
            {
                context.MeasureUnits.Add(measureUnit1);
                context.MeasureUnits.Add(measureUnit2);
                context.MeasureUnits.Add(measureUnit3);
                context.MeasureUnits.Add(measureUnit4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (IEnumerable<MeasureUnit>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(4, result.Count());

            #endregion
        }

        [TestMethod]
        public async Task GetAllCountAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var measureUnit1 = new Common.DataModels.MeasureUnit { Id = "A", Description = "Measure Unit #A" };
            var measureUnit2 = new Common.DataModels.MeasureUnit { Id = "B", Description = "Measure Unit #B" };
            var measureUnit3 = new Common.DataModels.MeasureUnit { Id = "C", Description = "Measure Unit #C" };
            var measureUnit4 = new Common.DataModels.MeasureUnit { Id = "D", Description = "Measure Unit #D" };

            using (var context = this.CreateContext())
            {
                context.MeasureUnits.Add(measureUnit1);
                context.MeasureUnits.Add(measureUnit2);
                context.MeasureUnits.Add(measureUnit3);
                context.MeasureUnits.Add(measureUnit4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllCountAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (int)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(4, result);

            #endregion
        }

        [TestMethod]
        public async Task GetAllCountAsync_NotFound()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetAllCountAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (int)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(0, result);

            #endregion
        }

        [TestMethod]
        public async Task GetAllNotFound()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetAllAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (IEnumerable<MeasureUnit>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(0, result.Count());

            #endregion
        }

        [TestMethod]
        [DataRow("A")]
        [DataRow("B")]
        public async Task GetByIdAsync_Found(string measureUnitId)
        {
            #region Arrange

            var controller = this.MockController();
            var measureUnit1 = new Common.DataModels.MeasureUnit { Id = "A", Description = "Measure Unit #A" };
            var measureUnit2 = new Common.DataModels.MeasureUnit { Id = "B", Description = "Measure Unit #B" };
            var measureUnit3 = new Common.DataModels.MeasureUnit { Id = "C", Description = "Measure Unit #C" };
            var measureUnit4 = new Common.DataModels.MeasureUnit { Id = "D", Description = "Measure Unit #D" };
            using (var context = this.CreateContext())
            {
                context.MeasureUnits.Add(measureUnit1);
                context.MeasureUnits.Add(measureUnit2);
                context.MeasureUnits.Add(measureUnit3);
                context.MeasureUnits.Add(measureUnit4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(measureUnitId);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (MeasureUnit)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(measureUnitId, result.Id);

            #endregion
        }

        [TestMethod]
        public async Task GetByIdAsync_NotFound()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync("A");

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult), GetDescription(actionResult.Result));

            #endregion
        }

        private MeasureUnitsController MockController()
        {
            return new MeasureUnitsController(
                new Mock<ILogger<MeasureUnitsController>>().Object,
                new Mock<IHubContext<DataHub, IDataHub>>().Object,
                this.ServiceProvider.GetService(typeof(IMeasureUnitProvider)) as IMeasureUnitProvider);
        }

        #endregion
    }
}

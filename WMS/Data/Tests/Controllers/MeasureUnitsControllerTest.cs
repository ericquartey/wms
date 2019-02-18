using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Providers;
using Ferretto.WMS.Data.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public class MeasureUnitsControllerTest : BaseControllerTest
    {
        #region Methods

        [TestCleanup]
        public void Cleanup()
        {
            this.CleanupDatabase();
        }

        [TestMethod]
        public async Task GetAllCountFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);
                var measureUnit1 = new Common.DataModels.MeasureUnit { Id = "A", Description = "Measure Unit #A" };
                var measureUnit2 = new Common.DataModels.MeasureUnit { Id = "B", Description = "Measure Unit #B" };
                var measureUnit3 = new Common.DataModels.MeasureUnit { Id = "C", Description = "Measure Unit #C" };
                var measureUnit4 = new Common.DataModels.MeasureUnit { Id = "D", Description = "Measure Unit #D" };
                context.MeasureUnits.Add(measureUnit1);
                context.MeasureUnits.Add(measureUnit2);
                context.MeasureUnits.Add(measureUnit3);
                context.MeasureUnits.Add(measureUnit4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAllCountAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (int)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(4, result);

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAllCountNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);

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

                var controller = MockController(context);
                var measureUnit1 = new Common.DataModels.MeasureUnit { Id = "A", Description = "Measure Unit #A" };
                var measureUnit2 = new Common.DataModels.MeasureUnit { Id = "B", Description = "Measure Unit #B" };
                var measureUnit3 = new Common.DataModels.MeasureUnit { Id = "C", Description = "Measure Unit #C" };
                var measureUnit4 = new Common.DataModels.MeasureUnit { Id = "D", Description = "Measure Unit #D" };
                context.MeasureUnits.Add(measureUnit1);
                context.MeasureUnits.Add(measureUnit2);
                context.MeasureUnits.Add(measureUnit3);
                context.MeasureUnits.Add(measureUnit4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<MeasureUnit>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(4, result.Count());

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAllNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<MeasureUnit>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(0, result.Count());

                #endregion
            }
        }

        [TestMethod]
        public async Task GetByIdFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);
                var measureUnit1 = new Common.DataModels.MeasureUnit { Id = "A", Description = "Measure Unit #A" };
                var measureUnit2 = new Common.DataModels.MeasureUnit { Id = "B", Description = "Measure Unit #B" };
                var measureUnit3 = new Common.DataModels.MeasureUnit { Id = "C", Description = "Measure Unit #C" };
                var measureUnit4 = new Common.DataModels.MeasureUnit { Id = "D", Description = "Measure Unit #D" };
                context.MeasureUnits.Add(measureUnit1);
                context.MeasureUnits.Add(measureUnit2);
                context.MeasureUnits.Add(measureUnit3);
                context.MeasureUnits.Add(measureUnit4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult1 = await controller.GetByIdAsync("A");
                var actionResult2 = await controller.GetByIdAsync("B");

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (MeasureUnit)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual("A", result1.Id);

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = (MeasureUnit)((OkObjectResult)actionResult2.Result).Value;
                Assert.AreEqual("B", result2.Id);

                #endregion
            }
        }

        [TestMethod]
        public async Task GetByIdNotFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);

                #endregion

                #region Act

                var actionResult = await controller.GetByIdAsync("A");

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

                #endregion
            }
        }

        private static MeasureUnitsController MockController(DatabaseContext context)
        {
            return new MeasureUnitsController(
                new Mock<ILogger<MeasureUnitsController>>().Object,
                new MeasureUnitProvider(context));
        }

        #endregion
    }
}

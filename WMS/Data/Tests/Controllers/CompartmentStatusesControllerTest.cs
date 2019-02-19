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
    public class CompartmentStatusesControllerTest : BaseControllerTest
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

                var controller = this.MockController();
                var compartmentStatus1 = new Common.DataModels.CompartmentStatus { Id = 1, Description = "Compartment Status #1" };
                var compartmentStatus2 = new Common.DataModels.CompartmentStatus { Id = 2, Description = "Compartment Status #2" };
                var compartmentStatus3 = new Common.DataModels.CompartmentStatus { Id = 3, Description = "Compartment Status #3" };
                var compartmentStatus4 = new Common.DataModels.CompartmentStatus { Id = 4, Description = "Compartment Status #4" };
                context.CompartmentStatuses.Add(compartmentStatus1);
                context.CompartmentStatuses.Add(compartmentStatus2);
                context.CompartmentStatuses.Add(compartmentStatus3);
                context.CompartmentStatuses.Add(compartmentStatus4);
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
                var compartmentStatus1 = new Common.DataModels.CompartmentStatus { Id = 1, Description = "Compartment Status #1" };
                var compartmentStatus2 = new Common.DataModels.CompartmentStatus { Id = 2, Description = "Compartment Status #2" };
                var compartmentStatus3 = new Common.DataModels.CompartmentStatus { Id = 3, Description = "Compartment Status #3" };
                var compartmentStatus4 = new Common.DataModels.CompartmentStatus { Id = 4, Description = "Compartment Status #4" };
                context.CompartmentStatuses.Add(compartmentStatus1);
                context.CompartmentStatuses.Add(compartmentStatus2);
                context.CompartmentStatuses.Add(compartmentStatus3);
                context.CompartmentStatuses.Add(compartmentStatus4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<CompartmentStatus>)((OkObjectResult)actionResult.Result).Value;
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

                var controller = this.MockController();

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<CompartmentStatus>)((OkObjectResult)actionResult.Result).Value;
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

                var controller = this.MockController();
                var compartmentStatus1 = new Common.DataModels.CompartmentStatus { Id = 1, Description = "Compartment Status #1" };
                var compartmentStatus2 = new Common.DataModels.CompartmentStatus { Id = 2, Description = "Compartment Status #2" };
                var compartmentStatus3 = new Common.DataModels.CompartmentStatus { Id = 3, Description = "Compartment Status #3" };
                var compartmentStatus4 = new Common.DataModels.CompartmentStatus { Id = 4, Description = "Compartment Status #4" };
                context.CompartmentStatuses.Add(compartmentStatus1);
                context.CompartmentStatuses.Add(compartmentStatus2);
                context.CompartmentStatuses.Add(compartmentStatus3);
                context.CompartmentStatuses.Add(compartmentStatus4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult1 = await controller.GetByIdAsync(1);
                var actionResult2 = await controller.GetByIdAsync(2);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (CompartmentStatus)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual(1, result1.Id);

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = (CompartmentStatus)((OkObjectResult)actionResult2.Result).Value;
                Assert.AreEqual(2, result2.Id);

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

        private CompartmentStatusesController MockController()
        {
            return new CompartmentStatusesController(
                new Mock<ILogger<CompartmentStatusesController>>().Object,
                this.ServiceProvider.GetService(typeof(ICompartmentStatusProvider)) as ICompartmentStatusProvider);
        }

        #endregion
    }
}

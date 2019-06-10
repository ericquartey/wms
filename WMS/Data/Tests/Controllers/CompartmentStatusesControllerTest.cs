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
    public class CompartmentStatusesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAllAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var compartmentStatus1 = new Common.DataModels.CompartmentStatus { Id = 1, Description = "Compartment Status #1" };
            var compartmentStatus2 = new Common.DataModels.CompartmentStatus { Id = 2, Description = "Compartment Status #2" };
            var compartmentStatus3 = new Common.DataModels.CompartmentStatus { Id = 3, Description = "Compartment Status #3" };
            var compartmentStatus4 = new Common.DataModels.CompartmentStatus { Id = 4, Description = "Compartment Status #4" };

            using (var context = this.CreateContext())
            {
                context.CompartmentStatuses.Add(compartmentStatus1);
                context.CompartmentStatuses.Add(compartmentStatus2);
                context.CompartmentStatuses.Add(compartmentStatus3);
                context.CompartmentStatuses.Add(compartmentStatus4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (IEnumerable<CompartmentStatus>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(4, result.Count());

            #endregion
        }

        [TestMethod]
        public async Task GetAllAsync_NotFound()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetAllAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (IEnumerable<CompartmentStatus>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(0, result.Count());

            #endregion
        }

        [TestMethod]
        public async Task GetAllCountAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var compartmentStatus1 = new Common.DataModels.CompartmentStatus { Id = 1, Description = "Compartment Status #1" };
            var compartmentStatus2 = new Common.DataModels.CompartmentStatus { Id = 2, Description = "Compartment Status #2" };
            var compartmentStatus3 = new Common.DataModels.CompartmentStatus { Id = 3, Description = "Compartment Status #3" };
            var compartmentStatus4 = new Common.DataModels.CompartmentStatus { Id = 4, Description = "Compartment Status #4" };
            using (var context = this.CreateContext())
            {
                context.CompartmentStatuses.Add(compartmentStatus1);
                context.CompartmentStatuses.Add(compartmentStatus2);
                context.CompartmentStatuses.Add(compartmentStatus3);
                context.CompartmentStatuses.Add(compartmentStatus4);
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
        [DataRow(1)]
        [DataRow(2)]
        public async Task GetByIdAsync_Found(int compartmentStatusId)
        {
            #region Arrange

            var controller = this.MockController();
            var compartmentStatus1 = new Common.DataModels.CompartmentStatus { Id = 1, Description = "Compartment Status #1" };
            var compartmentStatus2 = new Common.DataModels.CompartmentStatus { Id = 2, Description = "Compartment Status #2" };
            var compartmentStatus3 = new Common.DataModels.CompartmentStatus { Id = 3, Description = "Compartment Status #3" };
            var compartmentStatus4 = new Common.DataModels.CompartmentStatus { Id = 4, Description = "Compartment Status #4" };

            using (var context = this.CreateContext())
            {
                context.CompartmentStatuses.Add(compartmentStatus1);
                context.CompartmentStatuses.Add(compartmentStatus2);
                context.CompartmentStatuses.Add(compartmentStatus3);
                context.CompartmentStatuses.Add(compartmentStatus4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(compartmentStatusId);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (CompartmentStatus)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(compartmentStatusId, result.Id);

            #endregion
        }

        [TestMethod]
        public async Task GetByIdAsync_NotFound()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(1);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult), GetDescription(actionResult.Result));

            #endregion
        }

        private CompartmentStatusesController MockController()
        {
            return new CompartmentStatusesController(
                new Mock<ILogger<CompartmentStatusesController>>().Object,
                new Mock<IHubContext<DataHub, IDataHub>>().Object,
                this.ServiceProvider.GetService(typeof(ICompartmentStatusProvider)) as ICompartmentStatusProvider);
        }

        #endregion
    }
}

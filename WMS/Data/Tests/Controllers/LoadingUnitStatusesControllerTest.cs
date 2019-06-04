using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public class LoadingUnitStatusesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAllAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var loadingUnitStatus1 = new Common.DataModels.LoadingUnitStatus { Id = "A", Description = "Loading Unit Status #A" };
            var loadingUnitStatus2 = new Common.DataModels.LoadingUnitStatus { Id = "B", Description = "Loading Unit Status #B" };
            var loadingUnitStatus3 = new Common.DataModels.LoadingUnitStatus { Id = "C", Description = "Loading Unit Status #C" };
            var loadingUnitStatus4 = new Common.DataModels.LoadingUnitStatus { Id = "D", Description = "Loading Unit Status #D" };
            using (var context = this.CreateContext())
            {
                context.LoadingUnitStatuses.Add(loadingUnitStatus1);
                context.LoadingUnitStatuses.Add(loadingUnitStatus2);
                context.LoadingUnitStatuses.Add(loadingUnitStatus3);
                context.LoadingUnitStatuses.Add(loadingUnitStatus4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
            var result = (IEnumerable<LoadingUnitStatus>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(4, result.Count());

            #endregion
        }

        [TestMethod]
        public async Task GetAllCountAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var loadingUnitStatus1 = new Common.DataModels.LoadingUnitStatus { Id = "A", Description = "Loading Unit Status #A" };
            var loadingUnitStatus2 = new Common.DataModels.LoadingUnitStatus { Id = "B", Description = "Loading Unit Status #B" };
            var loadingUnitStatus3 = new Common.DataModels.LoadingUnitStatus { Id = "C", Description = "Loading Unit Status #C" };
            var loadingUnitStatus4 = new Common.DataModels.LoadingUnitStatus { Id = "D", Description = "Loading Unit Status #D" };
            using (var context = this.CreateContext())
            {
                context.LoadingUnitStatuses.Add(loadingUnitStatus1);
                context.LoadingUnitStatuses.Add(loadingUnitStatus2);
                context.LoadingUnitStatuses.Add(loadingUnitStatus3);
                context.LoadingUnitStatuses.Add(loadingUnitStatus4);
                context.SaveChanges();
            }

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

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
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

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
            var result = (IEnumerable<LoadingUnitStatus>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(0, result.Count());

            #endregion
        }

        [TestMethod]
        [DataRow("A")]
        [DataRow("B")]
        public async Task GetByIdAsync_Found(string loadingUnitStatusId)
        {
            #region Arrange

            var controller = this.MockController();
            var loadingUnitStatus1 = new Common.DataModels.LoadingUnitStatus { Id = "A", Description = "Loading Unit Status #A" };
            var loadingUnitStatus2 = new Common.DataModels.LoadingUnitStatus { Id = "B", Description = "Loading Unit Status #B" };
            var loadingUnitStatus3 = new Common.DataModels.LoadingUnitStatus { Id = "C", Description = "Loading Unit Status #C" };
            var loadingUnitStatus4 = new Common.DataModels.LoadingUnitStatus { Id = "D", Description = "Loading Unit Status #D" };

            using (var context = this.CreateContext())
            {
                context.LoadingUnitStatuses.Add(loadingUnitStatus1);
                context.LoadingUnitStatuses.Add(loadingUnitStatus2);
                context.LoadingUnitStatuses.Add(loadingUnitStatus3);
                context.LoadingUnitStatuses.Add(loadingUnitStatus4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(loadingUnitStatusId);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
            var result = (LoadingUnitStatus)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(loadingUnitStatusId, result.Id);

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

            Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

            #endregion
        }

        private LoadingUnitStatusesController MockController()
        {
            return new LoadingUnitStatusesController(
                new Mock<ILogger<LoadingUnitStatusesController>>().Object,
                new Mock<IHubContext<DataHub, IDataHub>>().Object,
                this.ServiceProvider.GetService(typeof(ILoadingUnitStatusProvider)) as ILoadingUnitStatusProvider);
        }

        #endregion
    }
}

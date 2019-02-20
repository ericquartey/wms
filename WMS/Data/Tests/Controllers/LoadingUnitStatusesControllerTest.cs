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
    public class LoadingUnitStatusesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAllCountFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                var loadingUnitStatus1 = new Common.DataModels.LoadingUnitStatus { Id = "A", Description = "Loading Unit Status #A" };
                var loadingUnitStatus2 = new Common.DataModels.LoadingUnitStatus { Id = "B", Description = "Loading Unit Status #B" };
                var loadingUnitStatus3 = new Common.DataModels.LoadingUnitStatus { Id = "C", Description = "Loading Unit Status #C" };
                var loadingUnitStatus4 = new Common.DataModels.LoadingUnitStatus { Id = "D", Description = "Loading Unit Status #D" };
                context.LoadingUnitStatuses.Add(loadingUnitStatus1);
                context.LoadingUnitStatuses.Add(loadingUnitStatus2);
                context.LoadingUnitStatuses.Add(loadingUnitStatus3);
                context.LoadingUnitStatuses.Add(loadingUnitStatus4);
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
                var loadingUnitStatus1 = new Common.DataModels.LoadingUnitStatus { Id = "A", Description = "Loading Unit Status #A" };
                var loadingUnitStatus2 = new Common.DataModels.LoadingUnitStatus { Id = "B", Description = "Loading Unit Status #B" };
                var loadingUnitStatus3 = new Common.DataModels.LoadingUnitStatus { Id = "C", Description = "Loading Unit Status #C" };
                var loadingUnitStatus4 = new Common.DataModels.LoadingUnitStatus { Id = "D", Description = "Loading Unit Status #D" };
                context.LoadingUnitStatuses.Add(loadingUnitStatus1);
                context.LoadingUnitStatuses.Add(loadingUnitStatus2);
                context.LoadingUnitStatuses.Add(loadingUnitStatus3);
                context.LoadingUnitStatuses.Add(loadingUnitStatus4);
                context.SaveChanges();

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
                var result = (IEnumerable<LoadingUnitStatus>)((OkObjectResult)actionResult.Result).Value;
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
                var loadingUnitStatus1 = new Common.DataModels.LoadingUnitStatus { Id = "A", Description = "Loading Unit Status #A" };
                var loadingUnitStatus2 = new Common.DataModels.LoadingUnitStatus { Id = "B", Description = "Loading Unit Status #B" };
                var loadingUnitStatus3 = new Common.DataModels.LoadingUnitStatus { Id = "C", Description = "Loading Unit Status #C" };
                var loadingUnitStatus4 = new Common.DataModels.LoadingUnitStatus { Id = "D", Description = "Loading Unit Status #D" };
                context.LoadingUnitStatuses.Add(loadingUnitStatus1);
                context.LoadingUnitStatuses.Add(loadingUnitStatus2);
                context.LoadingUnitStatuses.Add(loadingUnitStatus3);
                context.LoadingUnitStatuses.Add(loadingUnitStatus4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult1 = await controller.GetByIdAsync("A");
                var actionResult2 = await controller.GetByIdAsync("B");

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (LoadingUnitStatus)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual("A", result1.Id);

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = (LoadingUnitStatus)((OkObjectResult)actionResult2.Result).Value;
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

                var controller = this.MockController();

                #endregion

                #region Act

                var actionResult = await controller.GetByIdAsync("A");

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));

                #endregion
            }
        }

        private LoadingUnitStatusesController MockController()
        {
            return new LoadingUnitStatusesController(
                new Mock<ILogger<LoadingUnitStatusesController>>().Object,
                this.ServiceProvider.GetService(typeof(ILoadingUnitStatusProvider)) as ILoadingUnitStatusProvider);
        }

        #endregion
    }
}

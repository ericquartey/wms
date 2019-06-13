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
    public class LoadingUnitTypesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAllAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var loadingUnitType1 = new Common.DataModels.LoadingUnitType { Id = 1, Description = "Loading Unit Type #1" };
            var loadingUnitType2 = new Common.DataModels.LoadingUnitType { Id = 2, Description = "Loading Unit Type #2" };
            var loadingUnitType3 = new Common.DataModels.LoadingUnitType { Id = 3, Description = "Loading Unit Type #3" };
            var loadingUnitType4 = new Common.DataModels.LoadingUnitType { Id = 4, Description = "Loading Unit Type #4" };
            using (var context = this.CreateContext())
            {
                context.LoadingUnitTypes.Add(loadingUnitType1);
                context.LoadingUnitTypes.Add(loadingUnitType2);
                context.LoadingUnitTypes.Add(loadingUnitType3);
                context.LoadingUnitTypes.Add(loadingUnitType4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (IEnumerable<LoadingUnitType>)((OkObjectResult)actionResult.Result).Value;
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
            var result = (IEnumerable<LoadingUnitType>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(0, result.Count());

            #endregion
        }

        [TestMethod]
        public async Task GetAllCountAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var loadingUnitType1 = new Common.DataModels.LoadingUnitType { Id = 1, Description = "Loading Unit Type #1" };
            var loadingUnitType2 = new Common.DataModels.LoadingUnitType { Id = 2, Description = "Loading Unit Type #2" };
            var loadingUnitType3 = new Common.DataModels.LoadingUnitType { Id = 3, Description = "Loading Unit Type #3" };
            var loadingUnitType4 = new Common.DataModels.LoadingUnitType { Id = 4, Description = "Loading Unit Type #4" };
            using (var context = this.CreateContext())
            {
                context.LoadingUnitTypes.Add(loadingUnitType1);
                context.LoadingUnitTypes.Add(loadingUnitType2);
                context.LoadingUnitTypes.Add(loadingUnitType3);
                context.LoadingUnitTypes.Add(loadingUnitType4);
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
        public async Task GetByIdAsync_Found(int loadingUnitTypeId)
        {
            #region Arrange

            var controller = this.MockController();
            var loadingUnitType1 = new Common.DataModels.LoadingUnitType { Id = 1, Description = "Loading Unit Type #1" };
            var loadingUnitType2 = new Common.DataModels.LoadingUnitType { Id = 2, Description = "Loading Unit Type #2" };
            var loadingUnitType3 = new Common.DataModels.LoadingUnitType { Id = 3, Description = "Loading Unit Type #3" };
            var loadingUnitType4 = new Common.DataModels.LoadingUnitType { Id = 4, Description = "Loading Unit Type #4" };
            using (var context = this.CreateContext())
            {
                context.LoadingUnitTypes.Add(loadingUnitType1);
                context.LoadingUnitTypes.Add(loadingUnitType2);
                context.LoadingUnitTypes.Add(loadingUnitType3);
                context.LoadingUnitTypes.Add(loadingUnitType4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(loadingUnitTypeId);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (LoadingUnitType)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(loadingUnitTypeId, result.Id);

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

        private LoadingUnitTypesController MockController()
        {
            return new LoadingUnitTypesController(
                new Mock<ILogger<LoadingUnitTypesController>>().Object,
                this.ServiceProvider.GetService(typeof(ILoadingUnitTypeProvider)) as ILoadingUnitTypeProvider,
                this.ServiceProvider.GetService(typeof(ICellProvider)) as ICellProvider);
        }

        #endregion
    }
}

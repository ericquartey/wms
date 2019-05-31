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
    public class PackageTypesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAllAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var packageType1 = new Common.DataModels.PackageType { Id = 1, Description = "Package Type #1" };
            var packageType2 = new Common.DataModels.PackageType { Id = 2, Description = "Package Type #2" };
            var packageType3 = new Common.DataModels.PackageType { Id = 3, Description = "Package Type #3" };
            var packageType4 = new Common.DataModels.PackageType { Id = 4, Description = "Package Type #4" };
            using (var context = this.CreateContext())
            {
                context.PackageTypes.Add(packageType1);
                context.PackageTypes.Add(packageType2);
                context.PackageTypes.Add(packageType3);
                context.PackageTypes.Add(packageType4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (IEnumerable<PackageType>)((OkObjectResult)actionResult.Result).Value;
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
            var result = (IEnumerable<PackageType>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(0, result.Count());

            #endregion
        }

        [TestMethod]
        public async Task GetAllCountAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var packageType1 = new Common.DataModels.PackageType { Id = 1, Description = "Package Type #1" };
            var packageType2 = new Common.DataModels.PackageType { Id = 2, Description = "Package Type #2" };
            var packageType3 = new Common.DataModels.PackageType { Id = 3, Description = "Package Type #3" };
            var packageType4 = new Common.DataModels.PackageType { Id = 4, Description = "Package Type #4" };
            using (var context = this.CreateContext())
            {
                context.PackageTypes.Add(packageType1);
                context.PackageTypes.Add(packageType2);
                context.PackageTypes.Add(packageType3);
                context.PackageTypes.Add(packageType4);
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

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public async Task GetByIdFound(int packageTypeId)
        {
            #region Arrange

            var controller = this.MockController();
            var packageType1 = new Common.DataModels.PackageType { Id = 1, Description = "Package Type #1" };
            var packageType2 = new Common.DataModels.PackageType { Id = 2, Description = "Package Type #2" };
            var packageType3 = new Common.DataModels.PackageType { Id = 3, Description = "Package Type #3" };
            var packageType4 = new Common.DataModels.PackageType { Id = 4, Description = "Package Type #4" };

            using (var context = this.CreateContext())
            {
                context.PackageTypes.Add(packageType1);
                context.PackageTypes.Add(packageType2);
                context.PackageTypes.Add(packageType3);
                context.PackageTypes.Add(packageType4);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(packageTypeId);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (PackageType)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(packageTypeId, result.Id);

            #endregion
        }

        private PackageTypesController MockController()
        {
            return new PackageTypesController(
                new Mock<ILogger<PackageTypesController>>().Object,
                new Mock<IHubContext<DataHub, IDataHub>>().Object,
                this.ServiceProvider.GetService(typeof(IPackageTypeProvider)) as IPackageTypeProvider);
        }

        #endregion
    }
}

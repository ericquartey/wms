using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    [TestClass]
    public class AbcClassesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAllAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var abcClass1 = new Common.DataModels.AbcClass { Id = "A", Description = "A Class" };
            var abcClass2 = new Common.DataModels.AbcClass { Id = "B", Description = "B Class" };
            var abcClass3 = new Common.DataModels.AbcClass { Id = "C", Description = "C Class" };

            using (var context = this.CreateContext())
            {
                context.AbcClasses.Add(abcClass1);
                context.AbcClasses.Add(abcClass2);
                context.AbcClasses.Add(abcClass3);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (IEnumerable<AbcClass>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(3, result.Count());

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
            var result = (IEnumerable<AbcClass>)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(0, result.Count());

            #endregion
        }

        [TestMethod]
        public async Task GetAllCountAsync_Found()
        {
            #region Arrange

            var controller = this.MockController();
            var abcClass1 = new Common.DataModels.AbcClass { Id = "A", Description = "A Class" };
            var abcClass2 = new Common.DataModels.AbcClass { Id = "B", Description = "B Class" };
            var abcClass3 = new Common.DataModels.AbcClass { Id = "C", Description = "C Class" };

            using (var context = this.CreateContext())
            {
                context.AbcClasses.Add(abcClass1);
                context.AbcClasses.Add(abcClass2);
                context.AbcClasses.Add(abcClass3);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetAllCountAsync();

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (int)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(3, result);

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
        [DataRow("A")]
        [DataRow("B")]
        public async Task GetByIdAsync_Found(string abcClassId)
        {
            #region Arrange

            var controller = this.MockController();
            var abcClass1 = new Common.DataModels.AbcClass { Id = "A", Description = "A Class" };
            var abcClass2 = new Common.DataModels.AbcClass { Id = "B", Description = "B Class" };
            var abcClass3 = new Common.DataModels.AbcClass { Id = "C", Description = "C Class" };

            using (var context = this.CreateContext())
            {
                context.AbcClasses.Add(abcClass1);
                context.AbcClasses.Add(abcClass2);
                context.AbcClasses.Add(abcClass3);
                context.SaveChanges();
            }

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(abcClassId);

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), GetDescription(actionResult.Result));
            var result = (AbcClass)((OkObjectResult)actionResult.Result).Value;
            Assert.AreEqual(abcClassId, result.Id);

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

        private AbcClassesController MockController()
        {
            return new AbcClassesController(
                new Mock<ILogger<AbcClassesController>>().Object,
                this.ServiceProvider.GetService(typeof(IAbcClassProvider)) as IAbcClassProvider);
        }

        #endregion
    }
}

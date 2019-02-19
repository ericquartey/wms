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
    public class AbcClassesControllerTest : BaseControllerTest
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
                var abcClass1 = new Common.DataModels.AbcClass { Id = "A", Description = "A Class" };
                var abcClass2 = new Common.DataModels.AbcClass { Id = "B", Description = "B Class" };
                var abcClass3 = new Common.DataModels.AbcClass { Id = "C", Description = "C Class" };
                context.AbcClasses.Add(abcClass1);
                context.AbcClasses.Add(abcClass2);
                context.AbcClasses.Add(abcClass3);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAllCountAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (int)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(3, result);

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
                var abcClass1 = new Common.DataModels.AbcClass { Id = "A", Description = "A Class" };
                var abcClass2 = new Common.DataModels.AbcClass { Id = "B", Description = "B Class" };
                var abcClass3 = new Common.DataModels.AbcClass { Id = "C", Description = "C Class" };
                context.AbcClasses.Add(abcClass1);
                context.AbcClasses.Add(abcClass2);
                context.AbcClasses.Add(abcClass3);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<AbcClass>)((OkObjectResult)actionResult.Result).Value;
                Assert.AreEqual(3, result.Count());

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
                var result = (IEnumerable<AbcClass>)((OkObjectResult)actionResult.Result).Value;
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
                var abcClass1 = new Common.DataModels.AbcClass { Id = "A", Description = "A Class" };
                var abcClass2 = new Common.DataModels.AbcClass { Id = "B", Description = "B Class" };
                var abcClass3 = new Common.DataModels.AbcClass { Id = "C", Description = "C Class" };
                context.AbcClasses.Add(abcClass1);
                context.AbcClasses.Add(abcClass2);
                context.AbcClasses.Add(abcClass3);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult1 = await controller.GetByIdAsync("A");
                var actionResult2 = await controller.GetByIdAsync("B");

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (AbcClass)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual("A", result1.Id);

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = (AbcClass)((OkObjectResult)actionResult2.Result).Value;
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

        private AbcClassesController MockController()
        {
            return new AbcClassesController(
                new Mock<ILogger<AbcClassesController>>().Object,
                this.ServiceProvider.GetService(typeof(IAbcClassProvider)) as IAbcClassProvider);
        }

        #endregion
    }
}

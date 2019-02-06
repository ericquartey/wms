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
using DataModels = Ferretto.Common.DataModels;

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
        public async Task GetAllFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = MockController(context);
                var abcClass1 = new DataModels.AbcClass { Id = "A", Description = "A Class" };
                var abcClass2 = new DataModels.AbcClass { Id = "B", Description = "B Class" };
                var abcClass3 = new DataModels.AbcClass { Id = "C", Description = "C Class" };
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

                var controller = MockController(context);

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

                var controller = MockController(context);
                var abcClass1 = new DataModels.AbcClass { Id = "A", Description = "A Class" };
                var abcClass2 = new DataModels.AbcClass { Id = "B", Description = "B Class" };
                var abcClass3 = new DataModels.AbcClass { Id = "C", Description = "C Class" };
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

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        private static AbcClassesController MockController(DatabaseContext context)
        {
            return new AbcClassesController(
                new Mock<ILogger<AbcClassesController>>().Object,
                new AbcClassProvider(context));
        }

        #endregion
    }
}

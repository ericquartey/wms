using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.DataModels;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.Tests
{
    [TestClass]
    public class WhereConditionsTest : BaseControllerTest
    {
        #region Properties

        protected Common.DataModels.Item Item1 { get; set; }

        protected Common.DataModels.SchedulerRequest SchedulerRequest1 { get; set; }

        protected Common.DataModels.SchedulerRequest SchedulerRequest2 { get; set; }

        #endregion

        #region Methods

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
            this.Item1 = new Common.DataModels.Item
            {
                AbcClassId = this.AbcClass1.Id,
                Code = "Item #1 Code",
                Description = "Item #1 Description",
                Id = 1,
                ManagementType = ItemManagementType.FIFO,
            };
            this.SchedulerRequest1 = new Common.DataModels.SchedulerRequest
            {
                AreaId = this.Area1.Id,
                BayId = this.Bay1.Id,
                DispatchedQuantity = 0,
                Id = 1,
                IsInstant = false,
                ItemId = this.Item1.Id,
                OperationType = Common.DataModels.OperationType.Withdrawal,
                RequestedQuantity = 1,
            };
            this.SchedulerRequest2 = new Common.DataModels.SchedulerRequest
            {
                AreaId = this.Area1.Id,
                BayId = this.Bay2.Id,
                DispatchedQuantity = 0,
                Id = 2,
                IsInstant = false,
                ItemId = this.Item1.Id,
                OperationType = Common.DataModels.OperationType.Insertion,
                RequestedQuantity = 2,
            };

            using (var context = this.CreateContext())
            {
                context.Items.Add(this.Item1);
                context.SchedulerRequests.Add(this.SchedulerRequest1);
                context.SchedulerRequests.Add(this.SchedulerRequest2);
                context.SaveChanges();
            }
        }

        [TestMethod]
        public async Task StringAndOfFunctionsDifferentFields()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult1 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                $"StartsWith([BayDescription], 'Bay') And StartsWith([AreaDescription], 'Area')");

            var actionResult2 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "StartsWith([BayDescription], 'Bay') And StartsWith([AreaDescription], 'NOT PRESENT')");

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
            var result1 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult1.Result).Value;
            Assert.AreEqual(2, result1.Count());

            Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
            var result2 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult2.Result).Value;
            Assert.AreEqual(0, result2.Count());

            #endregion
        }

        [TestMethod]
        public async Task StringAndOfFunctionsSameField()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult1 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "StartsWith([BayDescription], 'Bay') And Contains([BayDescription], '#1')");

            var actionResult2 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "StartsWith([BayDescription], 'Bay') And Contains([BayDescription], 'NOT PRESENT')");

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
            var result1 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult1.Result).Value;
            Assert.AreEqual(1, result1.Count());

            Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
            var result2 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult2.Result).Value;
            Assert.AreEqual(0, result2.Count());

            #endregion
        }

        [TestMethod]
        public async Task StringContains()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult1 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "Contains([BayDescription], '#1')");

            var actionResult2 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "Contains([BayDescription], 'NOT PRESENT')");

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
            var result1 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult1.Result).Value;
            Assert.AreEqual(1, result1.Count());

            Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
            var result2 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult2.Result).Value;
            Assert.AreEqual(0, result2.Count());

            #endregion
        }

        [TestMethod]
        public async Task StringDoesNotContains()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult1 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "Not Contains([BayDescription], 'Bay')");

            var actionResult2 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "Not Contains([BayDescription], 'NOT PRESENT')");

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
            var result1 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult1.Result).Value;
            Assert.AreEqual(0, result1.Count());

            Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
            var result2 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult2.Result).Value;
            Assert.AreEqual(2, result2.Count());

            #endregion
        }

        [TestMethod]
        public async Task StringDoesNotEqual()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult1 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "[BayDescription] <> 'Bay #1'");

            var actionResult2 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "[BayDescription] <> 'NOT PRESENT'");

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
            var result1 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult1.Result).Value;
            Assert.AreEqual(1, result1.Count());

            Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
            var result2 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult2.Result).Value;
            Assert.AreEqual(2, result2.Count());

            #endregion
        }

        [TestMethod]
        public async Task StringEndsWith()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult1 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "EndsWith([BayDescription], '#1')");

            var actionResult2 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "EndsWith([BayDescription], 'NOT PRESENT')");

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
            var result1 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult1.Result).Value;
            Assert.AreEqual(1, result1.Count());

            Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
            var result2 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult2.Result).Value;
            Assert.AreEqual(0, result2.Count());

            #endregion
        }

        [TestMethod]
        public async Task StringEqual()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult1 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "[BayDescription] = 'Bay #1'");

            var actionResult2 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "[BayDescription] = 'NOT PRESENT'");

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
            var result1 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult1.Result).Value;
            Assert.AreEqual(1, result1.Count());

            Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
            var result2 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult2.Result).Value;
            Assert.AreEqual(0, result2.Count());

            #endregion
        }

        [TestMethod]
        public async Task StringStartsWith()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult1 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "StartsWith([BayDescription], 'Bay')");

            var actionResult2 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "StartsWith([BayDescription], 'NOT PRESENT')");

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
            var result1 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult1.Result).Value;
            Assert.AreEqual(2, result1.Count());

            Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
            var result2 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult2.Result).Value;
            Assert.AreEqual(0, result2.Count());

            #endregion
        }

        [TestMethod]
        public async Task ComplexAndOrGroups()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult1 = await controller.GetAllAsync(
                0,
                int.MaxValue,
                "StartsWith([BayDescription], 'Bay') And [OperationType] = ##ToString#Insertion# Or [BayDescription] <> 'Bay #2' And [RequestedQuantity] = 2");

            #endregion

            #region Assert

            Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
            var result1 = (IEnumerable<Core.Models.SchedulerRequest>)((OkObjectResult)actionResult1.Result).Value;
            Assert.AreEqual(1, result1.Count());

            #endregion
        }

        private SchedulerRequestsController MockController()
        {
            return new SchedulerRequestsController(
                this.ServiceProvider.GetService(
                    typeof(ISchedulerRequestProvider)) as ISchedulerRequestProvider);
        }

        #endregion
    }
}

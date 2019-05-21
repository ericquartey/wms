using System.Collections.Generic;
using System.ComponentModel;
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
    public class CompartmentTypesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetAllCountFound()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                var compartmentType1 = new Common.DataModels.CompartmentType { Id = 1, Height = 1, Width = 2 };
                var compartmentType2 = new Common.DataModels.CompartmentType { Id = 2, Height = 1, Width = 2 };
                var compartmentType3 = new Common.DataModels.CompartmentType { Id = 3, Height = 1, Width = 2 };
                var compartmentType4 = new Common.DataModels.CompartmentType { Id = 4, Height = 1, Width = 2 };
                context.CompartmentTypes.Add(compartmentType1);
                context.CompartmentTypes.Add(compartmentType2);
                context.CompartmentTypes.Add(compartmentType3);
                context.CompartmentTypes.Add(compartmentType4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAllCountAsync();

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Value, typeof(int));
                Assert.AreEqual(4, actionResult.Value);

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

                Assert.IsInstanceOfType(actionResult.Value, typeof(int));
                Assert.AreEqual(0, actionResult.Value);

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
                var compartmentType1 = new Common.DataModels.CompartmentType { Id = 1, Height = 1, Width = 2 };
                var compartmentType2 = new Common.DataModels.CompartmentType { Id = 2, Height = 1, Width = 2 };
                var compartmentType3 = new Common.DataModels.CompartmentType { Id = 3, Height = 1, Width = 2 };
                var compartmentType4 = new Common.DataModels.CompartmentType { Id = 4, Height = 1, Width = 2 };
                context.CompartmentTypes.Add(compartmentType1);
                context.CompartmentTypes.Add(compartmentType2);
                context.CompartmentTypes.Add(compartmentType3);
                context.CompartmentTypes.Add(compartmentType4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult = await controller.GetAllAsync(0, int.MaxValue);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
                var result = (IEnumerable<CompartmentType>)((OkObjectResult)actionResult.Result).Value;
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
                var result = (IEnumerable<CompartmentType>)((OkObjectResult)actionResult.Result).Value;
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
                var compartmentType1 = new Common.DataModels.CompartmentType { Id = 1, Height = 1, Width = 2 };
                var compartmentType2 = new Common.DataModels.CompartmentType { Id = 2, Height = 1, Width = 2 };
                var compartmentType3 = new Common.DataModels.CompartmentType { Id = 3, Height = 1, Width = 2 };
                var compartmentType4 = new Common.DataModels.CompartmentType { Id = 4, Height = 1, Width = 2 };
                context.CompartmentTypes.Add(compartmentType1);
                context.CompartmentTypes.Add(compartmentType2);
                context.CompartmentTypes.Add(compartmentType3);
                context.CompartmentTypes.Add(compartmentType4);
                context.SaveChanges();

                #endregion

                #region Act

                var actionResult1 = await controller.GetByIdAsync(1);
                var actionResult2 = await controller.GetByIdAsync(2);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (CompartmentType)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual(1, result1.Id);

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = (CompartmentType)((OkObjectResult)actionResult2.Result).Value;
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

        [TestMethod]
        public async Task GetTotalCompartmentCount()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                var compartmentType1 = new Common.DataModels.CompartmentType { Id = 1, Height = 1, Width = 2 };

                Common.DataModels.Compartment[] compartments =
                {
                    new Common.DataModels.Compartment { Id = 1, CompartmentTypeId = 1 },
                    new Common.DataModels.Compartment { Id = 2, CompartmentTypeId = 1 },
                    new Common.DataModels.Compartment { Id = 3, CompartmentTypeId = 1 },
                };

                context.CompartmentTypes.Add(compartmentType1);
                foreach (var compartment in compartments)
                {
                    context.Compartments.Add(compartment);
                }

                context.SaveChanges();

                #endregion

                #region Act

                var actionResult1 = await controller.GetByIdAsync(1);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (CompartmentType)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual(
                    compartments.Length,
                    result1.CompartmentsCount,
                    "Wrong Total Compartments Count");

                #endregion
            }
        }

        [TestMethod]
        public async Task GetEmptyCompartmentCount()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                var compartmentType1 = new Common.DataModels.CompartmentType { Id = 1, Height = 1, Width = 2 };

                Common.DataModels.Compartment[] compartments =
                {
                    new Common.DataModels.Compartment { Id = 1, CompartmentTypeId = 1, Stock = 10 },
                    new Common.DataModels.Compartment { Id = 2, CompartmentTypeId = 1, Stock = 0 },
                    new Common.DataModels.Compartment { Id = 3, CompartmentTypeId = 1, Stock = 10 },
                    new Common.DataModels.Compartment { Id = 4, CompartmentTypeId = 1, Stock = 0 },
                    new Common.DataModels.Compartment { Id = 5, CompartmentTypeId = 1, Stock = 0 },
                };

                context.CompartmentTypes.Add(compartmentType1);
                foreach (var compartment in compartments)
                {
                    context.Compartments.Add(compartment);
                }

                context.SaveChanges();

                #endregion

                #region Act

                var actionResult1 = await controller.GetByIdAsync(1);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (CompartmentType)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual(
                    compartments.Where(x => x.Stock.Equals(0)).ToArray().Length,
                    result1.EmptyCompartmentsCount,
                    "Wrong Empty Compartments Count");

                #endregion
            }
        }

        [TestMethod]
        public async Task GetAllPaged()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();
                Common.DataModels.CompartmentType[] compartmentTypes =
                {
                    new Common.DataModels.CompartmentType { Id = 1, Height = 1, Width = 2 },
                    new Common.DataModels.CompartmentType { Id = 2, Height = 1, Width = 2 },
                    new Common.DataModels.CompartmentType { Id = 3, Height = 1, Width = 2 },
                    new Common.DataModels.CompartmentType { Id = 4, Height = 1, Width = 2 },
                    new Common.DataModels.CompartmentType { Id = 5, Height = 1, Width = 2 },
                };

                foreach (var compartmentType in compartmentTypes)
                {
                    context.CompartmentTypes.Add(compartmentType);
                }

                context.SaveChanges();

                #endregion

                #region Act

                var take = 3;
                var actionResult1 = await controller.GetAllAsync(0, take);
                var actionResult2 = await controller.GetAllAsync(compartmentTypes.Length - 1, take);

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = (IEnumerable<CompartmentType>)((OkObjectResult)actionResult1.Result).Value;
                Assert.AreEqual(
                    take,
                    result1.Count(),
                    "Take parameter not respected");

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = (IEnumerable<CompartmentType>)((OkObjectResult)actionResult2.Result).Value;
                Assert.AreEqual(
                    1,
                    result2.Count(),
                    "The last window should give remaining objects");

                #endregion
            }
        }

        [TestMethod]
        public async Task SortByEmptyCompartmentCount()
        {
            using (var context = this.CreateContext())
            {
                #region Arrange

                var controller = this.MockController();

                Common.DataModels.CompartmentType[] compartmentTypes =
                {
                    new Common.DataModels.CompartmentType { Id = 1, Height = 1, Width = 2 },
                    new Common.DataModels.CompartmentType { Id = 2, Height = 1, Width = 2 },
                    new Common.DataModels.CompartmentType { Id = 3, Height = 1, Width = 2 },
                };

                Common.DataModels.Compartment[] compartments =
                {
                    new Common.DataModels.Compartment { Id = 1, CompartmentTypeId = 1, Stock = 10 },
                    new Common.DataModels.Compartment { Id = 2, CompartmentTypeId = 1, Stock = 0 },
                    new Common.DataModels.Compartment { Id = 3, CompartmentTypeId = 1, Stock = 10 },
                    new Common.DataModels.Compartment { Id = 4, CompartmentTypeId = 1, Stock = 10 },
                    new Common.DataModels.Compartment { Id = 5, CompartmentTypeId = 1, Stock = 10 },

                    new Common.DataModels.Compartment { Id = 6, CompartmentTypeId = 2, Stock = 10 },
                    new Common.DataModels.Compartment { Id = 7, CompartmentTypeId = 2, Stock = 10 },
                    new Common.DataModels.Compartment { Id = 8, CompartmentTypeId = 2, Stock = 0 },
                    new Common.DataModels.Compartment { Id = 9, CompartmentTypeId = 2, Stock = 0 },
                    new Common.DataModels.Compartment { Id = 10, CompartmentTypeId = 2, Stock = 10 },

                    new Common.DataModels.Compartment { Id = 11, CompartmentTypeId = 3, Stock = 10 },
                    new Common.DataModels.Compartment { Id = 12, CompartmentTypeId = 3, Stock = 0 },
                    new Common.DataModels.Compartment { Id = 13, CompartmentTypeId = 3, Stock = 0 },
                    new Common.DataModels.Compartment { Id = 14, CompartmentTypeId = 3, Stock = 10 },
                    new Common.DataModels.Compartment { Id = 15, CompartmentTypeId = 3, Stock = 0 },
                };

                foreach (var compartmentType in compartmentTypes)
                {
                    context.CompartmentTypes.Add(compartmentType);
                }

                foreach (var compartment in compartments)
                {
                    context.Compartments.Add(compartment);
                }

                context.SaveChanges();

                #endregion

                #region Act

                var actionResult1 = await controller.GetAllAsync(
                    0,
                    int.MaxValue,
                    null,
                    nameof(CompartmentType.EmptyCompartmentsCount));

                var actionResult2 = await controller.GetAllAsync(
                    0,
                    int.MaxValue,
                    null,
                    $"{nameof(CompartmentType.EmptyCompartmentsCount)} {nameof(ListSortDirection.Descending)}");

                #endregion

                #region Assert

                Assert.IsInstanceOfType(actionResult1.Result, typeof(OkObjectResult));
                var result1 = ((IEnumerable<CompartmentType>)((OkObjectResult)actionResult1.Result).Value).ToArray();
                Assert.AreEqual(
                    1,
                    result1[0].Id,
                    "Elements not in the right order");
                Assert.AreEqual(
                    2,
                    result1[1].Id,
                    "Elements not in the right order");
                Assert.AreEqual(
                    3,
                    result1[2].Id,
                    "Elements not in the right order");

                Assert.IsInstanceOfType(actionResult2.Result, typeof(OkObjectResult));
                var result2 = ((IEnumerable<CompartmentType>)((OkObjectResult)actionResult2.Result).Value).ToArray();
                Assert.AreEqual(
                    3,
                    result2[0].Id,
                    "Elements not in the right order");
                Assert.AreEqual(
                    2,
                    result2[1].Id,
                    "Elements not in the right order");
                Assert.AreEqual(
                    1,
                    result2[2].Id,
                    "Elements not in the right order");

                #endregion
            }
        }

        private CompartmentTypesController MockController()
        {
            return new CompartmentTypesController(
                new Mock<ILogger<CompartmentTypesController>>().Object,
                new Mock<IHubContext<DataHub, IDataHub>>().Object,
                this.ServiceProvider.GetService(typeof(IItemCompartmentTypeProvider)) as IItemCompartmentTypeProvider,
                this.ServiceProvider.GetService(typeof(ICompartmentTypeProvider)) as ICompartmentTypeProvider);
        }

        #endregion
    }
}

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
    public class MachinesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GrossWeight()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(1);

            #endregion

            #region Assert

            var resultMachine = (Machine)((OkObjectResult)actionResult.Result).Value;
            var totalWeight = this.LoadingUnit1.Weight + this.LoadingUnit2.Weight;
            Assert.IsTrue(resultMachine.GrossWeight == totalWeight);

            #endregion
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        [TestMethod]
        public async Task NetMaxWeight()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(1);

            #endregion

            #region Assert

            var resultMachine = (Machine)((OkObjectResult)actionResult.Result).Value;
            var netMaxWeight = this.Machine1.TotalMaxWeight -
                this.Machine1.Aisle.Cells.Sum(c => c.LoadingUnits.Sum(l => l.LoadingUnitType.EmptyWeight));
            Assert.IsTrue(resultMachine.NetMaxWeight == netMaxWeight);

            #endregion
        }

        [TestMethod]
        public async Task NetWeight()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(1);

            #endregion

            #region Assert

            var resultMachine = (Machine)((OkObjectResult)actionResult.Result).Value;
            var netWeight = this.Machine1.Aisle.Cells.Sum(c => c.LoadingUnits.Sum(l => l.Weight - l.LoadingUnitType.EmptyWeight));
            Assert.IsTrue(resultMachine.NetWeight == netWeight);

            #endregion
        }

        private MachinesController MockController()
        {
            return new MachinesController(
                new Mock<ILogger<MachinesController>>().Object,
                new Mock<IHubContext<DataHub, IDataHub>>().Object,
                this.ServiceProvider.GetService(typeof(IMachineProvider)) as IMachineProvider,
                this.ServiceProvider.GetService(typeof(IMissionProvider)) as IMissionProvider);
        }

        #endregion
    }
}

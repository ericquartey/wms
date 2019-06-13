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
    public class MachinesControllerTest : BaseControllerTest
    {
        #region Methods

        [TestMethod]
        public async Task GetByIdAsync_GrossWeight()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(1);

            #endregion

            #region Assert

            var resultMachine = (MachineDetails)((OkObjectResult)actionResult.Result).Value;
            var totalWeight = this.LoadingUnit1.Weight + this.LoadingUnit2.Weight;
            Assert.IsTrue(resultMachine.GrossWeight == totalWeight);

            #endregion
        }

        [TestMethod]
        public async Task GetByIdAsync_NetMaxWeight()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(1);

            #endregion

            #region Assert

            var resultMachine = (MachineDetails)((OkObjectResult)actionResult.Result).Value;
            var netMaxWeight = this.Machine1.TotalMaxWeight -
                this.Machine1.Aisle.Cells.Sum(c => c.LoadingUnits.Sum(l => l.LoadingUnitType.EmptyWeight));
            Assert.IsTrue(resultMachine.NetMaxWeight == netMaxWeight);

            #endregion
        }

        [TestMethod]
        public async Task GetByIdAsync_NetWeight()
        {
            #region Arrange

            var controller = this.MockController();

            #endregion

            #region Act

            var actionResult = await controller.GetByIdAsync(1);

            #endregion

            #region Assert

            var resultMachine = (MachineDetails)((OkObjectResult)actionResult.Result).Value;
            var netWeight = this.Machine1.Aisle.Cells.Sum(c => c.LoadingUnits.Sum(l => l.Weight - l.LoadingUnitType.EmptyWeight));
            Assert.IsTrue(resultMachine.NetWeight == netWeight);

            #endregion
        }

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        private MachinesController MockController()
        {
            return new MachinesController(
                new Mock<ILogger<MachinesController>>().Object,
                this.ServiceProvider.GetService(typeof(IMachineProvider)) as IMachineProvider,
                this.ServiceProvider.GetService(typeof(IMissionProvider)) as IMissionProvider,
                this.ServiceProvider.GetService(typeof(IBayProvider)) as IBayProvider);
        }

        #endregion
    }
}

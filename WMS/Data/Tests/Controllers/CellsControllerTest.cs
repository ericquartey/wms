using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    [TestClass]
    public partial class CellsControllerTest : BaseControllerTest
    {
        #region Methods

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        private CellsController MockController()
        {
            return new CellsController(
                new Mock<IHubContext<DataHub, IDataHub>>().Object,
                this.ServiceProvider.GetService(typeof(ICellProvider)) as ICellProvider,
                this.ServiceProvider.GetService(typeof(ILoadingUnitProvider)) as ILoadingUnitProvider,
                this.ServiceProvider.GetService(typeof(INotificationService)) as INotificationService);
        }

        #endregion
    }
}

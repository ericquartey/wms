using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    [TestClass]
    public partial class LoadingUnitsControllerTest : BaseControllerTest
    {
        #region Methods

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        private LoadingUnitsController MockController()
        {
            return new LoadingUnitsController(
                new Mock<IHubContext<DataHub, IDataHub>>().Object,
                this.ServiceProvider.GetService(typeof(ILoadingUnitProvider)) as ILoadingUnitProvider,
                this.ServiceProvider.GetService(typeof(ICompartmentProvider)) as ICompartmentProvider,
                this.ServiceProvider.GetService(typeof(IItemProvider)) as IItemProvider,
                this.ServiceProvider.GetService(typeof(ISchedulerService)) as ISchedulerService,
                this.ServiceProvider.GetService(typeof(INotificationService)) as INotificationService);
        }

        #endregion
    }
}

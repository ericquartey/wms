using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    [TestClass]
    public partial class MissionsControllerTest : BaseControllerTest
    {
        #region Methods

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        private MissionsController MockController()
        {
            return new MissionsController(
                new Mock<ILogger<MissionsController>>().Object,
                this.ServiceProvider.GetService(typeof(IMissionProvider)) as IMissionProvider,
                this.ServiceProvider.GetService(typeof(ISchedulerService)) as ISchedulerService,
                this.ServiceProvider.GetService(typeof(INotificationService)) as INotificationService);
        }

        #endregion
    }
}

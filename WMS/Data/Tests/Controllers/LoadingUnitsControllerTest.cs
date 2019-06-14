using Ferretto.WMS.Data.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                this.ServiceProvider.GetService(typeof(ILoadingUnitProvider)) as ILoadingUnitProvider,
                this.ServiceProvider.GetService(typeof(ICompartmentProvider)) as ICompartmentProvider,
                this.ServiceProvider.GetService(typeof(IItemProvider)) as IItemProvider,
                this.ServiceProvider.GetService(typeof(ISchedulerService)) as ISchedulerService);
        }

        #endregion
    }
}

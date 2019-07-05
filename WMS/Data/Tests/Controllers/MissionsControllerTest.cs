using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            return this.ServiceProvider.GetService(typeof(MissionsController)) as MissionsController;
        }

        #endregion
    }
}

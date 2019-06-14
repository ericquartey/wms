using Ferretto.WMS.Data.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.Data.WebAPI.Controllers.Tests
{
    [TestClass]
    public partial class CompartmentsControllerTest : BaseControllerTest
    {
        #region Methods

        [TestInitialize]
        public void Initialize()
        {
            this.InitializeDatabase();
        }

        private CompartmentsController MockController()
        {
            return new CompartmentsController(
                this.ServiceProvider.GetService(typeof(ICompartmentProvider)) as ICompartmentProvider);
        }

        #endregion
    }
}

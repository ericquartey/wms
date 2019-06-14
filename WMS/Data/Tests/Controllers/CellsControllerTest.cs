using Ferretto.WMS.Data.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                this.ServiceProvider.GetService(typeof(ICellProvider)) as ICellProvider,
                this.ServiceProvider.GetService(typeof(ILoadingUnitProvider)) as ILoadingUnitProvider);
        }

        #endregion
    }
}

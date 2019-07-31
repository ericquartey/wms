using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.App.Tests
{
    [TestClass]
    public class ModulesUITest : MockUI
    {
        #region Methods

        [TestMethod]
        [DeploymentItem("Ferretto.WMS.App.Themes.dll")]
        [DeploymentItem("DevExpress.Xpf.Accordion.v19.1.dll")]
        [DeploymentItem("DevExpress.Xpf.Controls.v19.1.dll")]
        [DeploymentItem("DevExpress.Xpf.Core.v19.1.dll")]
        [DeploymentItem("DevExpress.Xpf.Core.v19.1.Extensions.dll")]
        [DeploymentItem("DevExpress.Xpf.Core.v19.1.dll")]
        [DeploymentItem("DevExpress.Xpf.Docking.v19.1.dll")]
        [DeploymentItem("DevExpress.Xpf.Grid.v19.1.Core.dll")]
        [DeploymentItem("DevExpress.Xpf.Grid.v19.1.dll")]
        [DeploymentItem("DevExpress.Xpf.Grid.v19.1.Extensions.dll")]
        [DeploymentItem("DevExpress.Xpf.Layout.v19.1.Core.dll")]
        [DeploymentItem("DevExpress.Xpf.NavBar.v19.1.dll")]
        [DeploymentItem("DevExpress.Xpf.PrismAdapters.v19.1.dll")]
        [DeploymentItem("DevExpress.Xpf.Ribbon.v19.1.dll")]
        [DeploymentItem("DevExpress.Xpf.Themes.Office2016White.v19.1.dll")]
        [DeploymentItem("DevExpress.Xpo.v19.1.dll")]
        [DeploymentItem("Ferretto.WMS.App.Modules.BLL.dll")]
        [DeploymentItem("Ferretto.WMS.App.Modules.ItemLists.dll")]
        [DeploymentItem("Ferretto.WMS.App.Modules.Layout.dll")]
        [DeploymentItem("Ferretto.WMS.App.Modules.Machines.dll")]
        [DeploymentItem("Ferretto.WMS.App.Modules.MasterData.dll")]
        [DeploymentItem("Ferretto.WMS.App.Modules.Scheduler.dll")]
#pragma warning disable S2699 // Tests should include assertions
        public void TestUiModules()
#pragma warning restore S2699 // Tests should include assertions
        {
            AppearViews(typeof(Common.Utils.Modules.ItemLists));
            AppearViews(typeof(Common.Utils.Modules.Layout));
            AppearViews(typeof(Common.Utils.Modules.Machines));
            AppearViews(typeof(Common.Utils.Modules.Scheduler));
            AppearViews(typeof(Common.Utils.Modules.MasterData));
            this.WaitUiComplete();
        }

        #endregion
    }
}

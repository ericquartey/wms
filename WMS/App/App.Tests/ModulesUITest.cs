using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.App.Tests
{
    [TestClass]
    public class ModulesUITest : MockUI
    {
        #region Methods

        [TestMethod]
        [DeploymentItem("Ferretto.WMS.App.Themes.dll")]
        [DeploymentItem("DevExpress.Xpf.Accordion.v18.2.dll")]
        [DeploymentItem("DevExpress.Xpf.Controls.v18.2.dll")]
        [DeploymentItem("DevExpress.Xpf.Core.v18.2.dll")]
        [DeploymentItem("DevExpress.Xpf.Core.v18.2.Extensions.dll")]
        [DeploymentItem("DevExpress.Xpf.Core.v18.2.dll")]
        [DeploymentItem("DevExpress.Xpf.Docking.v18.2.dll")]
        [DeploymentItem("DevExpress.Xpf.Grid.v18.2.Core.dll")]
        [DeploymentItem("DevExpress.Xpf.Grid.v18.2.dll")]
        [DeploymentItem("DevExpress.Xpf.Grid.v18.2.Extensions.dll")]
        [DeploymentItem("DevExpress.Xpf.Layout.v18.2.Core.dll")]
        [DeploymentItem("DevExpress.Xpf.NavBar.v18.2.dll")]
        [DeploymentItem("DevExpress.Xpf.PrismAdapters.v18.2.dll")]
        [DeploymentItem("DevExpress.Xpf.Ribbon.v18.2.dll")]
        [DeploymentItem("DevExpress.Xpf.Themes.Office2016White.v18.2.dll")]
        [DeploymentItem("DevExpress.Xpo.v18.2.dll")]
#pragma warning disable S2699 // Tests should include assertions
        public void TestUiModuleLayout()
#pragma warning restore S2699 // Tests should include assertions
        {
            AppearViews(typeof(Common.Utils.Modules.Layout));
            AppearViews(typeof(Common.Utils.Modules.Machines));
            AppearViews(typeof(Common.Utils.Modules.Scheduler));
            AppearViews(typeof(Common.Utils.Modules.MasterData));
            this.WaitUiComplete();
        }

        #endregion
    }
}

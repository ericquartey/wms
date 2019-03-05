using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.App.ModulesUITests
{
    [TestClass]
    public class ModulesUITest : MockUI
    {
        #region Methods

        [TestMethod]
        [DeploymentItem("Ferretto.WMS.App.Themes.dll")]
        [DeploymentItem("DevExpress.Xpf.Accordion.v18.2")]
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
        public void TestUIModuleLayout()
        {
            this.AppearViews(typeof(Common.Utils.Modules.Layout));
            this.AppearViews(typeof(Common.Utils.Modules.Machines));
            this.AppearViews(typeof(Common.Utils.Modules.Scheduler));
            this.AppearViews(typeof(Common.Utils.Modules.MasterData));
            this.WaitUIComplete();
        }

        #endregion
    }
}

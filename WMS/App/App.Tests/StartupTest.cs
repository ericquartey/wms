using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;

namespace Ferretto.WMS.App.Tests
{
    [TestClass]
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
    [DeploymentItem("Ferretto.WMS.App.exe")]
    [DeploymentItem("Ferretto.WMS.App.Modules.BLL.dll")]
    [DeploymentItem("Ferretto.WMS.App.Modules.Layout.dll")]
    [DeploymentItem("Ferretto.WMS.App.Modules.Machines.dll")]
    [DeploymentItem("Ferretto.WMS.App.Modules.MasterData.dll")]
    [DeploymentItem("Ferretto.WMS.App.Modules.Scheduler.dll")]
    [DeploymentItem("Ferretto.WMS.App.exe.config")]
    public class StartupTest : EndToEndTest
    {
        #region Methods

        [TestCleanup]
        public void Cleanup()
        {
            this.CloseApp();
        }

        [TestInitialize]
        public void Initialize()
        {
            this.StartupApp();
        }

        [TestMethod]
        public void IsLoginFormLoaded()
        {
            var usernameTextBox = this.GetUsernameTextBox();
            Assert.IsNotNull(usernameTextBox);
        }

        [TestMethod]
        public void IsMainWindowLoaded()
        {
            var usernameTextBox = this.GetUsernameTextBox();
            var passwordTextBox = this.GetPasswordTextBox();
            var loginButton = this.GetLoginButton();

            usernameTextBox.SetValue("test");
            passwordTextBox.SetValue("test");

            loginButton.Click();
            System.Threading.Thread.Sleep(1000);
            Assert.IsNotNull(this.MainWindow);
        }

        private IUIItem GetLoginButton()
        {
            var loginButtonCriteria = SearchCriteria.ByAutomationId("LoginButton");
            var loginButton = this.MainWindow.Get(loginButtonCriteria);
            Assert.IsNotNull(loginButton);
            return loginButton;
        }

        private IUIItem GetPasswordTextBox()
        {
            var passwordTextBoxCriteria = SearchCriteria.ByAutomationId("PasswordTextBox");
            var passwordTextBox = this.MainWindow.Get(passwordTextBoxCriteria);
            Assert.IsNotNull(passwordTextBox);
            return passwordTextBox;
        }

        private IUIItem GetUsernameTextBox()
        {
            var usernameTextBoxCriteria = SearchCriteria.ByAutomationId("UsernameTextBox");
            var usernameTextBox = this.MainWindow.Get(usernameTextBoxCriteria);
            Assert.IsNotNull(usernameTextBox);
            return usernameTextBox;
        }

        #endregion
    }
}

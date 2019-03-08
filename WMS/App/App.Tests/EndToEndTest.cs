using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestStack.White;
using TestStack.White.Factory;
using TestStack.White.UIItems.WindowItems;

namespace Ferretto.WMS.App.Tests
{
    public class EndToEndTest
    {
        #region Fields

        private Application application;

        #endregion

        #region Properties

        public Window MainWindow =>
            this.application?.GetWindow(Common.Resources.DesktopApp.Application_Title, InitializeOption.NoCache);

        public TestContext TestContext { get; set; }

        #endregion

        #region Methods

        public void CloseApp()
        {
            this.application?.Close();
        }

        public void StartupApp()
        {
            var applicationPath = System.Environment.CurrentDirectory;
            var applicationFilePath = Path.Combine(applicationPath, "Ferretto.WMS.App.exe");

            var appProcess = new System.Diagnostics.Process();
            appProcess.StartInfo.WorkingDirectory = applicationPath;
            appProcess.StartInfo.FileName = applicationFilePath;
            appProcess.Start();

            System.Threading.Thread.Sleep(1000);
            Assert.IsFalse(appProcess.HasExited);
            appProcess.WaitForInputIdle();

            this.application = Application.Attach(appProcess);
        }

        #endregion
    }
}

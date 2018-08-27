using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestStack.White;
using TestStack.White.Factory;
using TestStack.White.UIItems.WindowItems;

namespace Ferretto.WMS.App.Tests
{
  [TestClass]
  public class EndToEndTest
  {
    private Application application;
    private Window mainWindow;
    private TestContext testContext;

    public Window MainWindow => this.mainWindow;

    public TestContext TestContext
    {
      get => this.testContext;
      set => this.testContext = value;
    }

    [TestInitialize]
    public void Initialize()
    {
      var applicationPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent.FullName;

      var applicationFilePath = Path.Combine(applicationPath, "Ferretto.WMS.App.exe");

      var appProcess = new System.Diagnostics.Process();
      appProcess.StartInfo.WorkingDirectory = applicationPath;
      appProcess.StartInfo.FileName = applicationFilePath;
      appProcess.Start();
      appProcess.WaitForInputIdle();

      this.application = Application.Attach(appProcess);

      this.mainWindow = this.application.GetWindow(
       "Ferretto Warehouse Management System",
       InitializeOption.NoCache);
    }

    [TestCleanup]
    public void Cleanup()
    {
      this.application?.Close();
    }

  }
}

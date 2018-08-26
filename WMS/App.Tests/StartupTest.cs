using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestStack.White;
using TestStack.White.Factory;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WindowItems;

namespace Ferretto.WMS.App.Tests
{
  [TestClass]
  public class StartupTest
  {
    private Application application;
    private Window mainWindow;

    private TestContext testContext;
    public TestContext TestContext
    {
      get => this.testContext;
      set => this.testContext = value;
    }


    [TestCleanup]
    public void Cleanup()
    {
      this.application?.Close();
    }

    [TestInitialize]
    public void Initialize()
    {
      var applicationPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent.FullName;

      var applicationFilePath = Path.Combine(applicationPath, "Ferretto.WMS.App.exe");
     
      var appProcess = new Process();
      appProcess.StartInfo.WorkingDirectory = applicationPath;
      appProcess.StartInfo.FileName = applicationFilePath;
      appProcess.Start();
      appProcess.WaitForInputIdle();

      this.application = Application.Attach(appProcess);

      this.mainWindow = this.application.GetWindow(
       "Ferretto Warehouse Management System",
       InitializeOption.NoCache);
    }

    [TestMethod]
    [TestCategory("Smoke")]
    [TestCategory("End to end")]
    public void TestNavMenuIsLoaded()
    {
      var window = this.application.GetWindow(
        "Ferretto Warehouse Management System",
        InitializeOption.NoCache);
      

      var menuContentCriteria = SearchCriteria.ByText("Catalog");
      var menuContent = window.Get(menuContentCriteria);

      Assert.IsNotNull(menuContent);
    }
  }
}

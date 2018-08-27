using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestStack.White.UIItems.Finders;

namespace Ferretto.WMS.App.Tests
{
  [TestClass]
  public class StartupTest : EndToEndTest
  {
    [TestMethod]
    [TestCategory("Smoke")]
    [TestCategory("End to end")]
    public void TestNavMenuIsLoaded()
    {
      var menuContentCriteria = SearchCriteria.ByText("Catalog");
      var menuContent = this.MainWindow.Get(menuContentCriteria);

      Assert.IsNotNull(menuContent);
    }
  }
}

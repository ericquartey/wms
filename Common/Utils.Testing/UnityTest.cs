using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.Common.Utils.Testing
{
  [TestClass]
  public class UnityTest
  {
    private UnityContainer container;
    private TestContext testContext;

    protected UnityTest() { }

    public virtual void Initialize()
    {
      this.container = new UnityContainer();
      var locator = new UnityServiceLocator(this.container);

      ServiceLocator.SetLocatorProvider(() => locator);

      this.container.RegisterInstance<IUnityContainer>(this.container);
    }

    public UnityContainer Container => this.container;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get => this.testContext;
      set => this.testContext = value;
    }
  }
}

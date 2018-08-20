using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Feretto.Common.Modules.BLL.Tests
{
  [TestClass]
  public class UnityTest
  {
    public static UnityContainer container;

    protected UnityTest() { }

    [AssemblyInitialize]
    static public void InitializeContainer(TestContext context)
    {
      container = new UnityContainer();
      var locator = new UnityServiceLocator(container);

      ServiceLocator.SetLocatorProvider(() => locator);

      container.RegisterInstance<IUnityContainer>(container);
    }
  }
}

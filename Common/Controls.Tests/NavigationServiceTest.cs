using System.Linq;
using Ferretto.Common.Utils.Testing;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Feretto.Common.Controls.Tests
{
  [TestClass]
  public class NavigationServiceTest : PrismTest
  {
    private INavigationService navigationService;

    [TestInitialize]
    public override void Initialize()
    {
      base.Initialize();

      this.Container.RegisterType<INavigationService, NavigationService>();

      this.navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
    }

    [TestMethod]
    public void TestRegister()
    {
      this.navigationService.Register<TestView, TestViewModel>();

      var expectedRegistrationName = $"{typeof(TestView).FullName}.1";

      Assert.IsNotNull(this.Container.Registrations.SingleOrDefault(registration =>
        registration.RegisteredType == typeof(INavigableView)
        &&
        registration.MappedToType == typeof(TestView)
        &&
        registration.Name == expectedRegistrationName
      ));

      Assert.IsNotNull(this.Container.Registrations.SingleOrDefault(registration =>
        registration.RegisteredType == typeof(INavigableViewModel)
        &&
        registration.MappedToType == typeof(TestViewModel)
        &&
        registration.Name == expectedRegistrationName
      ));
    }

    [TestMethod]
    public void TestRegisterTwice()
    {
      this.navigationService.Register<TestView, TestViewModel>();
      this.navigationService.Register<TestView, TestViewModel>();

      var expectedRegistrationName = $"{typeof(TestView).FullName}.2";

      Assert.AreEqual(2, this.Container.Registrations.Count(registration =>
       registration.RegisteredType == typeof(INavigableView)
       &&
       registration.MappedToType == typeof(TestView)
     ));

      Assert.AreEqual(2, this.Container.Registrations.Count(registration =>
        registration.RegisteredType == typeof(INavigableView)
        &&
        registration.MappedToType == typeof(TestView)
      ));

      Assert.IsNotNull(this.Container.Registrations.SingleOrDefault(registration =>
       registration.RegisteredType == typeof(INavigableView)
       &&
       registration.MappedToType == typeof(TestView)
       &&
       registration.Name == expectedRegistrationName
     ));

      Assert.IsNotNull(this.Container.Registrations.SingleOrDefault(registration =>
        registration.RegisteredType == typeof(INavigableViewModel)
        &&
        registration.MappedToType == typeof(TestViewModel)
        &&
        registration.Name == expectedRegistrationName
      ));
    }

    #region Test Types

    private class TestView : INavigableView
    {
      public string Token { get; set; }
      public string MapId { get; set; }
      public string Title { get; set; }
    }

    private class TestViewModel : INavigableViewModel
    {
      public System.String Token { get; set; }
      public System.String StateId { get; set; }

      public void Appear()
      {
        // Test method. Nothing to do here.
      }

      public void Disappear()
      {
        // Test method. Nothing to do here.
      }
    }

    #endregion
  }
}

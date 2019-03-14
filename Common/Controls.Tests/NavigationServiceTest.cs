using System.Linq;
using CommonServiceLocator;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Utils.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prism.Ioc;
using Unity;

#pragma warning disable S3261 // Namespaces should not be empty

namespace Feretto.Common.Controls.Tests
{
#pragma warning disable SA1005
#pragma warning disable S125 // Sections of code should not be commented out
    // Single line comments must begin with single space
    //[TestClass]
    //public class NavigationServiceTest : PrismTest

    //{
    //    #region Fields

    //    private INavigationService navigationService;

    //    #endregion

    //    #region Methods

    //    [TestInitialize]
    //    public override void Initialize()
    //    {
    //        base.Initialize();

    //        this.Container.RegisterType<INavigationService, NavigationService>();

    //        this.navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
    //    }

    //    [TestMethod]
    //    public void TestRegister()
    //    {
    //        var unityContainer = this.Container. containerRegistry.GetContainer();
    //        this.Container.Lo
    //        IUnityContainer configuration = new UnityContainer().LoadConfiguration();
    //        // var containerRegistry = ServiceLocator.Current.GetInstance<IContainerRegistration>();
    //        // Arrange
    //        // (nothing)

    //        // Act
    //        this.navigationService.Register<TestView, TestViewModel>();

    //        // Assert
    //        var expectedRegistrationName = $"{typeof(TestView).FullName}.1";

    //        //var reg = containerRegistry.GetContainer().Registrations;
    //        Assert.IsNotNull(this.Container.Registrations.SingleOrDefault(registration =>
    //            registration.RegisteredType == typeof(INavigableView)
    //            &&
    //            registration.MappedToType == typeof(TestView)
    //            &&
    //            registration.Name == expectedRegistrationName));

    //        Assert.IsNotNull(this.Container.Registrations.SingleOrDefault(registration =>
    //            registration.RegisteredType == typeof(INavigableViewModel)
    //            &&
    //            registration.MappedToType == typeof(TestViewModel)
    //            &&
    //            registration.Name == expectedRegistrationName));
    //    }

    //    [TestMethod]
    //    public void TestRegisterAndGetViewModel()
    //    {
    //        // Arrange
    //        this.navigationService.Register<TestView, TestViewModel>();

    //        // Act
    //        var viewName = typeof(TestView).FullName;
    //        var token = "a-token";
    //        var data = "id=1";
    //        var viewModel = this.navigationService.RegisterAndGetViewModel(viewName, token, data);

    //        // Assert
    //        Assert.IsNotNull(viewModel);
    //        Assert.AreEqual(viewModel.Token, token);
    //        Assert.AreEqual(viewModel.GetType(), typeof(TestViewModel));
    //        Assert.AreEqual(2, this.Container.Registrations.Count(registration =>
    //            registration.RegisteredType == typeof(INavigableViewModel)
    //            &&
    //            registration.MappedToType == typeof(TestViewModel)));
    //    }

    //    [TestMethod]
    //    public void TestRegisterAndGetViewModelMoreThanOnce()
    //    {
    //        // Arrange
    //        this.navigationService.Register<TestView, TestViewModel>();

    //        // Act
    //        var viewName = typeof(TestView).FullName;
    //        this.navigationService.RegisterAndGetViewModel(viewName, null, null);
    //        this.navigationService.RegisterAndGetViewModel(viewName, null, null);
    //        this.navigationService.RegisterAndGetViewModel(viewName, null, null);

    //        // Assert
    //        Assert.AreEqual(1 + 3, this.Container.Registrations.Count(registration =>
    //            registration.RegisteredType == typeof(INavigableViewModel)
    //            &&
    //            registration.MappedToType == typeof(TestViewModel)));

    //    }

    //    [TestMethod]
    //    public void TestRegisterAndGetViewModelWithoutInitialRegistration()
    //    {
    //        // Arrange
    //        // Do not call the INavigationService.Register method so that the call INavigationService.RegisterAndGetViewModel should fail

    //        // Act + Assert
    //        var viewName = typeof(TestView).FullName;
    //        var token = "a-token";
    //        var data = "id=1";
    //        Assert.ThrowsException<System.InvalidOperationException>(
    //            () => this.navigationService.RegisterAndGetViewModel(viewName, token, data));
    //    }

    //    [TestMethod]
    //    public void TestRegisterTwice()
    //    {
    //        // Arrange
    //        this.navigationService.Register<TestView, TestViewModel>();

    //        // Act
    //        this.navigationService.Register<TestView, TestViewModel>();

    //        // Assert
    //        var expectedRegistrationName = $"{typeof(TestView).FullName}.2";

    //        Assert.AreEqual(2, this.Container.Registrations.Count(registration =>
    //            registration.RegisteredType == typeof(INavigableView)
    //            &&
    //            registration.MappedToType == typeof(TestView)));

    //        Assert.AreEqual(2, this.Container.Registrations.Count(registration =>
    //            registration.RegisteredType == typeof(INavigableView)
    //            &&
    //            registration.MappedToType == typeof(TestView)));

    //        Assert.IsNotNull(this.Container.Registrations.SingleOrDefault(registration =>
    //            registration.RegisteredType == typeof(INavigableView)
    //            &&
    //            registration.MappedToType == typeof(TestView)
    //            &&
    //            registration.Name == expectedRegistrationName));

    //        Assert.IsNotNull(this.Container.Registrations.SingleOrDefault(registration =>
    //            registration.RegisteredType == typeof(INavigableViewModel)
    //            &&
    //            registration.MappedToType == typeof(TestViewModel)
    //            &&
    //            registration.Name == expectedRegistrationName));
    //    }

    //    #endregion

    //    #region Classes

    //    [System.Diagnostics.CodeAnalysis.SuppressMessage(
    //        "Major Code Smell",
    //        "CA1812: NavigationServiceTest.TestView is an internal class that is apparently never instantiated.",
    //        Justification = "Prism test class")]
    //    private class TestView : INavigableView
    //    {
    //        #region Properties

    //        public object Data { get; set; }

    //        public object DataContext { get; set; }

    //        public bool IsClosed { get; set; }

    //        public string MapId { get; set; }

    //        public string Title { get; set; }

    //        public string Token { get; set; }

    //        public WmsViewType ViewType { get; }

    //        #endregion

    //        #region Methods

    //        public void Disappear()
    //        {
    //            // TODO
    //        }

    //        #endregion
    //    }

    //    [System.Diagnostics.CodeAnalysis.SuppressMessage(
    //        "Major Code Smell",
    //        "CA1812: NavigationServiceTest.TestView is an internal class that is apparently never instantiated.",
    //        Justification = "Prism test class")]
    //    private class TestViewModel : INavigableViewModel
    //    {
    //        #region Properties

    //        public object Data { get; set; }

    //        public string MapId { get; set; }

    //        public string StateId { get; set; }

    //        public string Token { get; set; }

    //        #endregion

    //        #region Methods

    //        public void Appear()
    //        {
    //            // Test method. Nothing to do here.
    //        }

    //        public bool CanDisappear()
    //        {
    //            return true;
    //        }

    //        public void Disappear()
    //        {
    //            // Test method. Nothing to do here.
    //        }

    //        public void Dispose()
    //        {
    //            // Test method. Implement dispose
    //        }

    //        #endregion
    //    }

    //    #endregion
    //}
}

#pragma warning restore S125 // Sections of code should not be commented out
#pragma warning restore SA1005 // Single line comments must begin with single space

#pragma warning restore S3261 // Namespaces should not be empty

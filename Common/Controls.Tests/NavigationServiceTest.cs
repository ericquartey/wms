using System.Linq;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Utils.Testing;
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
            // Arrange
            // (nothing)

            // Act
            this.navigationService.Register<TestView, TestViewModel>();

            // Assert
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
            // Arrange
            this.navigationService.Register<TestView, TestViewModel>();

            // Act
            this.navigationService.Register<TestView, TestViewModel>();

            // Assert
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

        [TestMethod]
        public void TestRegisterAndGetViewModel()
        {
            // Arrange
            this.navigationService.Register<TestView, TestViewModel>();

            // Act
            var viewName = typeof(TestView).FullName;
            var token = "a-token";
            var viewModel = this.navigationService.RegisterAndGetViewModel(viewName, token);

            // Assert
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(viewModel.Token, token);
            Assert.AreEqual(viewModel.GetType(), typeof(TestViewModel));
            Assert.AreEqual(2, this.Container.Registrations.Count(registration =>
                registration.RegisteredType == typeof(INavigableViewModel)
                &&
                registration.MappedToType == typeof(TestViewModel)
            ));
        }

        [TestMethod]
        public void TestRegisterAndGetViewModelMoreThanOnce()
        {
            // Arrange
            this.navigationService.Register<TestView, TestViewModel>();

            // Act
            var viewName = typeof(TestView).FullName;
            this.navigationService.RegisterAndGetViewModel(viewName, null);
            this.navigationService.RegisterAndGetViewModel(viewName, null);
            this.navigationService.RegisterAndGetViewModel(viewName, null);

            // Assert
            Assert.AreEqual(1 + 3, this.Container.Registrations.Count(registration =>
                registration.RegisteredType == typeof(INavigableViewModel)
                &&
                registration.MappedToType == typeof(TestViewModel)
            ));
        }

        [TestMethod]
        public void TestRegisterAndGetViewModelWithoutInitialRegistration()
        {
            // Arrange
            // Do not call the INavigationService.Register method so that the call INavigationService.RegisterAndGetViewModel should fail

            // Act + Assert
            var viewName = typeof(TestView).FullName;
            var token = "a-token";

            Assert.ThrowsException<System.InvalidOperationException>(
                () => this.navigationService.RegisterAndGetViewModel(viewName, token));
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
            public System.String MapId { get; set; }
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

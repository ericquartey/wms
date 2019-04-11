﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferretto.WMS.App.Tests
{
    [TestClass]
    public class NavigationServiceTest : MockUI
    {
        #region Methods

        [TestMethod]
        public void TestRegister()
        {
            // Act
            this.NavigationService.Register<TestView, TestViewModel>();

            // Assert
            var expectedRegistrationName = $"{typeof(TestView).FullName}.1";

            Assert.IsNotNull(this.Container.Registrations.SingleOrDefault(registration =>
                registration.RegisteredType == typeof(INavigableView)
                &&
                registration.MappedToType == typeof(TestView)
                &&
                registration.Name == expectedRegistrationName));

            Assert.IsNotNull(this.Container.Registrations.SingleOrDefault(registration =>
                registration.RegisteredType == typeof(INavigableViewModel)
                &&
                registration.MappedToType == typeof(TestViewModel)
                &&
                registration.Name == expectedRegistrationName));
        }

        [TestMethod]
        public void TestRegisterAndGetViewModel()
        {
            // Arrange
            this.NavigationService.Register<TestView, TestViewModel>();

            // Act
            var viewName = typeof(TestView).FullName;
            var token = "a-token";
            var data = "id=1";
            var viewModel = this.NavigationService.RegisterAndGetViewModel(viewName, token, data);

            // Assert
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(viewModel.Token, token);
            Assert.AreEqual(viewModel.GetType(), typeof(TestViewModel));
            Assert.AreEqual(2, this.Container.Registrations.Count(registration =>
                registration.RegisteredType == typeof(INavigableViewModel)
                &&
                registration.MappedToType == typeof(TestViewModel)));
        }

        [TestMethod]
        public void TestRegisterAndGetViewModelMoreThanOnce()
        {
            // Arrange
            this.NavigationService.Register<TestView, TestViewModel>();

            // Act
            var viewName = typeof(TestView).FullName;
            this.NavigationService.RegisterAndGetViewModel(viewName, null, null);
            this.NavigationService.RegisterAndGetViewModel(viewName, null, null);
            this.NavigationService.RegisterAndGetViewModel(viewName, null, null);

            // Assert
            Assert.AreEqual(1 + 3, this.Container.Registrations.Count(registration =>
                registration.RegisteredType == typeof(INavigableViewModel)
                &&
                registration.MappedToType == typeof(TestViewModel)));
        }

        [TestMethod]
        public void TestRegisterAndGetViewModelWithoutInitialRegistration()
        {
            // Arrange
            // Do not call the INavigationService.Register method so that the call INavigationService.RegisterAndGetViewModel should fail

            // Act + Assert
            var viewName = typeof(TestView).FullName;
            var token = "a-token";
            var data = "id=1";
            Assert.ThrowsException<System.InvalidOperationException>(
                () => this.NavigationService.RegisterAndGetViewModel(viewName, token, data));
        }

        [TestMethod]
        public void TestRegisterTwice()
        {
            // Arrange
            this.NavigationService.Register<TestView, TestViewModel>();

            // Act
            this.NavigationService.Register<TestView, TestViewModel>();

            // Assert
            var expectedRegistrationName = $"{typeof(TestView).FullName}.2";

            Assert.AreEqual(2, this.Container.Registrations.Count(registration =>
                registration.RegisteredType == typeof(INavigableView)
                &&
                registration.MappedToType == typeof(TestView)));

            Assert.AreEqual(2, this.Container.Registrations.Count(registration =>
                registration.RegisteredType == typeof(INavigableView)
                &&
                registration.MappedToType == typeof(TestView)));

            Assert.IsNotNull(this.Container.Registrations.SingleOrDefault(registration =>
                registration.RegisteredType == typeof(INavigableView)
                &&
                registration.MappedToType == typeof(TestView)
                &&
                registration.Name == expectedRegistrationName));

            Assert.IsNotNull(this.Container.Registrations.SingleOrDefault(registration =>
                registration.RegisteredType == typeof(INavigableViewModel)
                &&
                registration.MappedToType == typeof(TestViewModel)
                &&
                registration.Name == expectedRegistrationName));
        }

        #endregion
    }
}

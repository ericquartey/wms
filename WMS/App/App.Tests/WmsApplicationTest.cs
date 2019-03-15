using System;
using System.Windows;
using CommonServiceLocator;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Prism.Unity.Ioc;
using Prism.Unity.Regions;
using Unity;
using Unity.Exceptions;

namespace Ferretto.WMS.App.Tests
{
    public class WmsApplicationTest : WmsApplicationBaseTest
#pragma warning restore S1200 // Classes should not be coupled to too many other classes (Single Responsibility Principle)
    {
        #region Methods

        public void InitializeTest()
        {
            this.InitializeInternal();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            WmsApplication.LoadCatalog(moduleCatalog);
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
            WmsApplication.RegisterAdapterMappings(regionAdapterMappings, this.Container);
        }

        protected override Window CreateShell()
        {
            return this.Container.Resolve<Shell>();
        }

#pragma warning disable S1185 // Overriding members should do more than simply call the same member in the base class

        protected override IContainerExtension CreateContainerExtension()
        {
            return new UnityContainerExtension();
        }

#pragma warning restore S1185 // Overriding members should do more than simply call the same member in the base class

        protected override void RegisterRequiredTypes(IContainerRegistry containerRegistry)
        {
            base.RegisterRequiredTypes(containerRegistry);
            containerRegistry.RegisterSingleton<IRegionNavigationContentLoader, UnityRegionNavigationContentLoader>();
            containerRegistry.RegisterSingleton<IServiceLocator, UnityServiceLocatorAdapter>();
        }

        protected override void RegisterFrameworkExceptionTypes()
        {
            base.RegisterFrameworkExceptionTypes();
            ExceptionExtensions.RegisterFrameworkExceptionType(typeof(ResolutionFailedException));
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            WmsApplication.RegisterTypes(containerRegistry, this.Container);
        }

        #endregion
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CommonServiceLocator;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Prism.Events;
using Prism.Ioc;
using Prism.Logging;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Regions.Behaviors;

namespace Ferretto.WMS.App.Tests
{
    public abstract class WmsApplicationBaseTest
    {
        #region Fields

        private IContainerExtension containerExtension;

        private IModuleCatalog moduleCatalog;

        #endregion

        #region Properties

        public IContainerProvider Container => this.containerExtension;

        #endregion

        #region Methods

        public virtual void Initialize()
        {
            this.containerExtension = this.CreateContainerExtension();
            this.moduleCatalog = this.CreateModuleCatalog();
            this.RegisterRequiredTypes(this.containerExtension);
            this.RegisterTypes(this.containerExtension);
            this.containerExtension.FinalizeExtension();

            this.ConfigureServiceLocator();

            this.ConfigureModuleCatalog(this.moduleCatalog);

            var regionAdapterMappins = this.containerExtension.Resolve<RegionAdapterMappings>();
            this.ConfigureRegionAdapterMappings(regionAdapterMappins);

            var defaultRegionBehaviors = this.containerExtension.Resolve<IRegionBehaviorFactory>();
            this.ConfigureDefaultRegionBehaviors(defaultRegionBehaviors);

            this.RegisterFrameworkExceptionTypes();

            var shell = this.CreateShell();
            if (shell != null)
            {
                RegionManager.SetRegionManager(shell, this.containerExtension.Resolve<IRegionManager>());
                RegionManager.UpdateRegions();
            }

            this.InitializeModules();
        }

        protected virtual void ConfigureDefaultRegionBehaviors(IRegionBehaviorFactory regionBehaviors)
        {
            if (regionBehaviors != null)
            {
                regionBehaviors.AddIfMissing(BindRegionContextToDependencyObjectBehavior.BehaviorKey, typeof(BindRegionContextToDependencyObjectBehavior));
                regionBehaviors.AddIfMissing(RegionActiveAwareBehavior.BehaviorKey, typeof(RegionActiveAwareBehavior));
                regionBehaviors.AddIfMissing(SyncRegionContextWithHostBehavior.BehaviorKey, typeof(SyncRegionContextWithHostBehavior));
                regionBehaviors.AddIfMissing(RegionManagerRegistrationBehavior.BehaviorKey, typeof(RegionManagerRegistrationBehavior));
                regionBehaviors.AddIfMissing(RegionMemberLifetimeBehavior.BehaviorKey, typeof(RegionMemberLifetimeBehavior));
                regionBehaviors.AddIfMissing(ClearChildViewsRegionBehavior.BehaviorKey, typeof(ClearChildViewsRegionBehavior));
                regionBehaviors.AddIfMissing(AutoPopulateRegionBehavior.BehaviorKey, typeof(AutoPopulateRegionBehavior));
            }
        }

        protected virtual void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
        }

        protected virtual void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
            if (regionAdapterMappings != null)
            {
                regionAdapterMappings.RegisterMapping(typeof(Selector), this.containerExtension.Resolve<SelectorRegionAdapter>());
                regionAdapterMappings.RegisterMapping(typeof(ItemsControl), this.containerExtension.Resolve<ItemsControlRegionAdapter>());
                regionAdapterMappings.RegisterMapping(typeof(ContentControl), this.containerExtension.Resolve<ContentControlRegionAdapter>());
            }
        }

        protected virtual void ConfigureServiceLocator()
        {
            ServiceLocator.SetLocatorProvider(() => this.containerExtension.Resolve<IServiceLocator>());
        }

        protected virtual void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewModelFactory((view, type) =>
            {
                return this.Container.Resolve(type);
            });
        }

        protected abstract IContainerExtension CreateContainerExtension();

        protected virtual IModuleCatalog CreateModuleCatalog()
        {
            return new ModuleCatalog();
        }

        protected abstract Window CreateShell();

        protected void InitializeInternal()
        {
            this.ConfigureViewModelLocator();
            this.Initialize();
        }

        protected virtual void InitializeModules()
        {
            var manager = this.containerExtension.Resolve<IModuleManager>();
            manager.Run();
        }

        protected virtual void RegisterFrameworkExceptionTypes()
        {
            ExceptionExtensions.RegisterFrameworkExceptionType(typeof(ActivationException));
        }

        protected virtual void RegisterRequiredTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance(this.containerExtension);
            containerRegistry.RegisterInstance(this.moduleCatalog);
            containerRegistry.RegisterSingleton<ILoggerFacade, TextLogger>();
            containerRegistry.RegisterSingleton<IDialogService, DialogService>();
            containerRegistry.RegisterSingleton<IModuleInitializer, ModuleInitializer>();
            containerRegistry.RegisterSingleton<IModuleManager, ModuleManager>();
            containerRegistry.RegisterSingleton<RegionAdapterMappings>();
            containerRegistry.RegisterSingleton<IRegionManager, RegionManager>();
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<IRegionViewRegistry, RegionViewRegistry>();
            containerRegistry.RegisterSingleton<IRegionBehaviorFactory, RegionBehaviorFactory>();
            containerRegistry.Register<IRegionNavigationJournalEntry, RegionNavigationJournalEntry>();
            containerRegistry.Register<IRegionNavigationJournal, RegionNavigationJournal>();
            containerRegistry.Register<IRegionNavigationService, RegionNavigationService>();
        }

        protected abstract void RegisterTypes(IContainerRegistry containerRegistry);

        #endregion
    }
}

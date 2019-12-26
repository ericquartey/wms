using System;
using System.Configuration;
using System.Reflection;
using System.Windows;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Unity;

namespace Ferretto.VW.App
{
    public partial class App : PrismApplication
    {
        #region Fields

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;

            this.logger.Info("*** Starting application ***");
        }

        #endregion

        #region Methods

        public static void LoadCatalog(IModuleCatalog moduleCatalog)
        {
            (moduleCatalog as ModuleCatalog)?.Load();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            LoadCatalog(moduleCatalog);
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                var viewName = viewType.FullName;
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                var viewModelName = $"{MvvmNaming.GetViewModelName(viewName)}, {viewAssemblyName}";
                return Type.GetType(viewModelName);
            });
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }

        protected override Window CreateShell()
        {
            return this.Container.Resolve<Shell>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!AppCheck.Start())
            {
                this.Shutdown(1);
            }

            SplashScreenService.Show();

            this.HACK_ForceItalianLanguage();

            base.OnStartup(e);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // UI controls services
            containerRegistry.RegisterUiControlServices(
                new NavigationOptions { MainContentRegionName = Utils.Modules.Layout.REGION_MAINCONTENT });

            // App services
            var serviceUrl = ConfigurationManager.AppSettings.GetAutomationServiceUrl();
            var serviceLiveHealthPath = ConfigurationManager.AppSettings.GetAutomationServiceLiveHealthPath();
            var serviceReadyHealthPath = ConfigurationManager.AppSettings.GetAutomationServiceReadyHealthPath();

            containerRegistry.RegisterAppServices(serviceUrl, serviceLiveHealthPath, serviceReadyHealthPath);

            // MAS Web API services
            var operatorHubPath = ConfigurationManager.AppSettings.GetAutomationServiceOperatorHubPath();
            var installationHubPath = ConfigurationManager.AppSettings.GetAutomationServiceInstallationHubPath();
            containerRegistry.RegisterMachineAutomationWebServices(serviceUrl, c =>
            {
                var client = c.Resolve<RetryHttpClient>();

                var bayNumber = ConfigurationManager.AppSettings.GetBayNumber();
                client.DefaultRequestHeaders.Add("Bay-Number", bayNumber.ToString());
                client.DefaultRequestHeaders.Add("Accept-Language", System.Globalization.CultureInfo.CurrentUICulture.Name);

                return client;
            });

            containerRegistry.RegisterMachineAutomationHubs(serviceUrl, operatorHubPath, installationHubPath);

            // WMS Web API services
            RegisterWmsHubs(containerRegistry);

            RegisterWmsProviders(containerRegistry);
        }

        private static void RegisterWmsHubs(IContainerRegistry container)
        {
            var wmsHubPath = ConfigurationManager.AppSettings.GetWMSDataServiceHubDataPath();
            var hubClient = DataServiceFactory.GetService<IDataHubClient>(wmsHubPath);
            container.RegisterInstance(hubClient);
        }

        private static void RegisterWmsProviders(IContainerRegistry container)
        {
            var wmsServiceUrl = ConfigurationManager.AppSettings.GetWMSDataServiceUrl();

            container.RegisterInstance(DataServiceFactory.GetService<IBaysWmsWebService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IBarcodesWmsWebService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IImagesWmsWebService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IMissionOperationsWmsWebService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IMissionsWmsWebService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<ILoadingUnitsWmsWebService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IItemsWmsWebService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IItemListsWmsWebService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IAreasWmsWebService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IMachinesWmsWebService>(wmsServiceUrl));
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            AppCheck.End();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            this.logger.Error(e.ExceptionObject as Exception, "An unhandled exception was thrown.");

            NLog.LogManager.Flush();
            NLog.LogManager.Shutdown();
        }

        private void HACK_ForceItalianLanguage()
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("it-IT");
            System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("it-IT");
        }

        #endregion
    }
}

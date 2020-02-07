using System;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
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

        protected override void OnExit(ExitEventArgs e)
        {
            AppCheck.End();

            this.DeactivateBay();

            base.OnExit(e);
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
            var serviceWmsUrl = ConfigurationManager.AppSettings.GetWMSDataServiceUrl();
            var serviceLiveHealthPath = ConfigurationManager.AppSettings.GetAutomationServiceLiveHealthPath();
            var serviceReadyHealthPath = ConfigurationManager.AppSettings.GetAutomationServiceReadyHealthPath();

            containerRegistry.RegisterAppServices(serviceUrl, serviceWmsUrl, serviceLiveHealthPath, serviceReadyHealthPath);

            // MAS Web API services
            var operatorHubPath = ConfigurationManager.AppSettings.GetAutomationServiceOperatorHubPath();
            var installationHubPath = ConfigurationManager.AppSettings.GetAutomationServiceInstallationHubPath();
            containerRegistry.RegisterMasWebServices(serviceUrl, c =>
            {
                var client = c.Resolve<RetryHttpClient>();

                var bayNumber = ConfigurationManager.AppSettings.GetBayNumber();
                client.DefaultRequestHeaders.Add("Bay-Number", bayNumber.ToString());
                client.DefaultRequestHeaders.Add("Accept-Language", System.Globalization.CultureInfo.CurrentUICulture.Name);

                return client;
            });

            containerRegistry.RegisterMasHubs(serviceUrl, operatorHubPath, installationHubPath);

            // WMS Web API services
            RegisterWmsProviders(containerRegistry);
            var wmsHubPath = ConfigurationManager.AppSettings.GetWMSDataServiceHubDataPath();
            containerRegistry.RegisterWmsHub(wmsHubPath);

            // USB Watcher
            RegisterUsbWatcher(containerRegistry);
        }

        private static void RegisterUsbWatcher(IContainerRegistry container)
        {
            container.Register<Ferretto.VW.App.Services.IO.UsbWatcherService>();
        }

        private static void RegisterWmsProviders(IContainerRegistry container)
        {
            var wmsServiceEnabled = ConfigurationManager.AppSettings.GetWmsDataServiceEnabled();
            var wmsServiceUrl = ConfigurationManager.AppSettings.GetWMSDataServiceUrl();

            container.RegisterWmsWebServices(wmsServiceUrl, c =>
            {
                var client = c.Resolve<RetryHttpClient>();

                client.DefaultRequestHeaders.Add("Accept-Language", System.Globalization.CultureInfo.CurrentUICulture.Name);

                return client;
            });
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            this.logger.Error(e.ExceptionObject as Exception, "An unhandled exception was thrown.");

            this.DeactivateBay();

            NLog.LogManager.Flush();
            NLog.LogManager.Shutdown();
        }

        private void DeactivateBay()
        {
            try
            {
                var healthService = this.Container.Resolve<IHealthProbeService>();
                if (healthService.HealthMasStatus != HealthStatus.Unhealthy && healthService.HealthMasStatus != HealthStatus.Unknown)
                {
                    this.logger.Info("Deactivating bay on application exit.");

                    var baysWebService = this.Container.Resolve<IMachineBaysWebService>();

                    Task
                        .Run(async () => await baysWebService.DeactivateAsync().ConfigureAwait(false))
                        .GetAwaiter().GetResult();
                }
            }
            catch (HttpRequestException)
            {
            }
        }

        private void HACK_ForceItalianLanguage()
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
            System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
        }

        #endregion
    }
}

using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Models;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils;
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

            this.SetLanguage();

            this.ClearTempFolder();

            base.OnStartup(e);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // MAS Web API services
            var operatorHubPath = ConfigurationManager.AppSettings.GetAutomationServiceOperatorHubPath();
            var installationHubPath = ConfigurationManager.AppSettings.GetAutomationServiceInstallationHubPath();

            var serviceUrl = ConfigurationManager.AppSettings.GetAutomationServiceUrl();
            containerRegistry.RegisterMasHubs(serviceUrl, operatorHubPath, installationHubPath);
            containerRegistry.RegisterMasWebServices(serviceUrl, c =>
            {
                var client = new HttpClient();

                var bayNumber = ConfigurationManager.AppSettings.GetBayNumber();
                client.DefaultRequestHeaders.Add("Bay-Number", bayNumber.ToString());
                client.DefaultRequestHeaders.Add("Accept-Language", System.Globalization.CultureInfo.CurrentUICulture.Name);

                return client;
            });

            // UI controls services
            containerRegistry.RegisterUiControlServices(
                new NavigationOptions { MainContentRegionName = Utils.Modules.Layout.REGION_MAINCONTENT });

            // App services
            var serviceLiveHealthPath = ConfigurationManager.AppSettings.GetAutomationServiceLiveHealthPath();
            var serviceReadyHealthPath = ConfigurationManager.AppSettings.GetAutomationServiceReadyHealthPath();

            containerRegistry.RegisterAppServices(serviceUrl, serviceLiveHealthPath, serviceReadyHealthPath);

            // USB Watcher
            RegisterUsbWatcher(containerRegistry);
        }

        private static void RegisterUsbWatcher(IContainerRegistry container)
        {
            container.Register<Ferretto.VW.App.Services.IO.UsbWatcherService>();
        }

        private void ClearTempFolder()
        {
            var tempFolder = ConfigurationManager.AppSettings["Update:Exchange:Temp"];
#if !DEBUG
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }
#endif
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
            catch (Exception ex) when (ex is MasWebApiException || ex is HttpRequestException)
            {
            }
        }

        private void SetLanguage()
        {
            try
            {
                string language = System.Configuration.ConfigurationManager.AppSettings["Language"];

                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(language);
                System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(language);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex.ToString());

                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-EN");
                System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-EN");
            }                    

            // Setup resource manager
            LocalizedResources.Instance.AddResourceManager(Errors.ResourceManager);
            LocalizedResources.Instance.AddResourceManager(ErrorsApp.ResourceManager);
            LocalizedResources.Instance.AddResourceManager(EventArgsMissionChanged.ResourceManager);
            LocalizedResources.Instance.AddResourceManager(General.ResourceManager);
            LocalizedResources.Instance.AddResourceManager(HelpDescriptions.ResourceManager);
            LocalizedResources.Instance.AddResourceManager(InstallationApp.ResourceManager);
            LocalizedResources.Instance.AddResourceManager(LoadLogin.ResourceManager);
            LocalizedResources.Instance.AddResourceManager(MainMenu.ResourceManager);
            LocalizedResources.Instance.AddResourceManager(MaintenanceMenu.ResourceManager);
            LocalizedResources.Instance.AddResourceManager(Menu.ResourceManager);
            LocalizedResources.Instance.AddResourceManager(OperatorApp.ResourceManager);
            LocalizedResources.Instance.AddResourceManager(SensorCard.ResourceManager);
            LocalizedResources.Instance.AddResourceManager(ServiceHealthProbe.ResourceManager);
            LocalizedResources.Instance.AddResourceManager(ServiceMachine.ResourceManager);

            LocalizedResources.Instance.CurrentCulture = CultureInfo.CurrentCulture;
        }

        #endregion
    }
}

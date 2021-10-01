using System;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Models;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Telemetry.Contracts.Hub;
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
            Current.DispatcherUnhandledException += this.App_OnDispatcherUnhandledException;
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
            this.logger.Info("*** Closing application ***");
            AppCheck.End();

            this.DeactivateBay();

            this.DeactivateAccessories();

            base.OnExit(e);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Container.UseTelemetryHubs();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!AppCheck.Start())
            {
                this.Shutdown(1);
            }

            SplashScreenService.Show();

            this.SetLanguage();

            ClearTempFolder();

            base.OnStartup(e);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // MAS Web API services
            var operatorHubPath = ConfigurationManager.AppSettings.GetAutomationServiceOperatorHubPath();
            var installationHubPath = ConfigurationManager.AppSettings.GetAutomationServiceInstallationHubPath();

            var serviceUrl = ConfigurationManager.AppSettings.GetAutomationServiceUrl();
            containerRegistry.RegisterMasHubs(serviceUrl, operatorHubPath, installationHubPath);
            var bayNumber = ConfigurationManager.AppSettings.GetBayNumber();
            containerRegistry.RegisterMasWebServices(serviceUrl, c =>
            {
                var client = new HttpClient();

                client.DefaultRequestHeaders.Add("Bay-Number", bayNumber.ToString());
                client.DefaultRequestHeaders.Add("Accept-Language", System.Globalization.CultureInfo.CurrentUICulture.Name);

                return client;
            });

            // Telemetry
            var telemetryHubPathUri = ConfigurationManager.AppSettings.GetTelemetryHubPath();
            containerRegistry.RegisterTelemetryHub(telemetryHubPathUri);

            // UI controls services
            containerRegistry.RegisterUiControlServices(
                new NavigationOptions { MainContentRegionName = Utils.Modules.Layout.REGION_MAINCONTENT });

            // App services
            var serviceLiveHealthPath = ConfigurationManager.AppSettings.GetAutomationServiceLiveHealthPath();
            var serviceReadyHealthPath = ConfigurationManager.AppSettings.GetAutomationServiceReadyHealthPath();
            var serviceName = ConfigurationManager.AppSettings.GetAutomationServiceName();
            var bayNumberDef = (BayNumber)Enum.Parse(typeof(BayNumber), bayNumber);
            var isMaster = (bayNumberDef == BayNumber.BayOne);

            containerRegistry.RegisterAppServices(serviceUrl, serviceLiveHealthPath, serviceReadyHealthPath, serviceName, isMaster);
        }

        private static void ClearTempFolder()
        {
            var tempFolder = ConfigurationManager.AppSettings["Update:Exchange:Temp"];
#if !DEBUG
            if (System.IO.Directory.Exists(tempFolder))
            {
                System.IO.Directory.Delete(tempFolder, true);
            }
#endif
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            this.logger.Error(e.ExceptionObject as Exception, "An unhandled exception was thrown.");

            this.DeactivateBay();
            this.DeactivateAccessories();

            NLog.LogManager.Flush();
            NLog.LogManager.Shutdown();
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            this.logger.Error(e.Exception, "An unhandled dispatcher exception was thrown.");

            e.Handled = true;
        }

        private void DeactivateAccessories()
        {
            try
            {
                var serialPortService = this.Container.Resolve<ISerialPortsService>();
                this.logger.Info("Deactivating serial port service on application exit.");

                serialPortService.Stop();

                var barcodeReaderService = this.Container.Resolve<IBarcodeReaderService>();
                this.logger.Info("Deactivating barcode reader on application exit.");

                var taskBarcode = Task.Run(() => barcodeReaderService.StopAsync());

                var laserPointerService = this.Container.Resolve<ILaserPointerService>();
                this.logger.Info("Deactivating laser on application exit.");

                var taskLaser = Task.Run(() => laserPointerService.StopAsync());

                var alphaNumericBarService = this.Container.Resolve<IAlphaNumericBarService>();
                this.logger.Info("Deactivating alpha bar on application exit.");

                var taskAlpha = Task.Run(() => alphaNumericBarService.StopAsync());

                Task.WaitAll(taskBarcode, taskLaser, taskAlpha);

                this.logger.Debug("Accessories Deactivated.");
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
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
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
        }

        private void SetLanguage()
        {
            try
            {
                var language = System.Configuration.ConfigurationManager.AppSettings["Language"];

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
            Localized.Instance.AddResourceManager(ErrorsApp.ResourceManager);
            Localized.Instance.AddResourceManager(EventArgsMissionChanged.ResourceManager);
            Localized.Instance.AddResourceManager(General.ResourceManager);
            Localized.Instance.AddResourceManager(HelpDescriptions.ResourceManager);
            Localized.Instance.AddResourceManager(InstallationApp.ResourceManager);
            Localized.Instance.AddResourceManager(LoadLogin.ResourceManager);
            Localized.Instance.AddResourceManager(MainMenu.ResourceManager);
            Localized.Instance.AddResourceManager(MaintenanceMenu.ResourceManager);
            Localized.Instance.AddResourceManager(VW.App.Resources.Menu.ResourceManager);
            Localized.Instance.AddResourceManager(OperatorApp.ResourceManager);
            Localized.Instance.AddResourceManager(SensorCard.ResourceManager);
            Localized.Instance.AddResourceManager(ServiceHealthProbe.ResourceManager);
            Localized.Instance.AddResourceManager(ServiceMachine.ResourceManager);

            Localized.Instance.CurrentCulture = CultureInfo.CurrentCulture;
            Localized.Instance.CurrentKeyboardCulture = CultureInfo.CurrentCulture;
        }

        #endregion
    }
}

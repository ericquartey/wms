using System;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App
{
    public partial class App
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

        protected override void OnInitialized()
        {
            try
            {
                SplashScreenService.SetMessage(DesktopApp.InitializingLogin);
                SplashScreenService.Hide();

                var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
                navigationService.Appear(nameof(Common.Utils.Modules.Layout), Common.Utils.Modules.Layout.LOGINVIEW);

                var assembly = typeof(App).Assembly;
                var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

                this.logger.Info($"Starting application, version '{versionInfo.ProductVersion}'.");
            }
            catch (System.Exception ex)
            {
                this.logger.Error(ex, "An error occurred on application startup.");
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!AppCheck.Start())
            {
                this.Shutdown(1);
            }

            this.HACK_ForceItalianLanguage();

            base.OnStartup(e);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var navigationService = this.Container.Resolve<NavigationService>();
            containerRegistry.RegisterInstance<INavigationService>(navigationService);

            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<IDialogService, DialogService>();

            // UI services
            var serviceUrl = ConfigurationManager.AppSettings.GetAutomationServiceUrl();
            var serviceLiveHealthPath = ConfigurationManager.AppSettings.GetAutomationServiceLiveHealthPath();
            var serviceReadyHealthPath = ConfigurationManager.AppSettings.GetAutomationServiceReadyHealthPath();

            containerRegistry.RegisterUiServices(serviceUrl, serviceLiveHealthPath, serviceReadyHealthPath);

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

            //var sensorsService = this.Container.Resolve<SensorsService>();
            //containerRegistry.RegisterInstance<ISensorsService>(sensorsService);
            containerRegistry.RegisterSingleton<ISensorsService, SensorsService>();

            // WMS Web API services
            RegisterWmsHubs(containerRegistry);

            RegisterWmsProviders(containerRegistry);
        }

        private static void RegisterWmsHubs(IContainerRegistry container)
        {
            var wmsHubPath = ConfigurationManager.AppSettings.Get("WMS:DataService:Hubs:Data:Path");
            var wmsHub = DataServiceFactory.GetService<IDataHubClient>(new Uri(wmsHubPath));
            container.RegisterInstance(wmsHub);
        }

        private static void RegisterWmsProviders(IContainerRegistry container)
        {
            var wmsServiceUrl = new Uri(ConfigurationManager.AppSettings.Get("WMS:DataService:Url"));

            container.RegisterInstance(DataServiceFactory.GetService<IBaysDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IImagesDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IMissionOperationsDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IMissionsDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<ILoadingUnitsDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IItemsDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IItemListsDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IAreasDataService>(wmsServiceUrl));
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception.InnerException != null ? e.Exception.InnerException : e.Exception;
            MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
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

        private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Exception exi = ex.InnerException != null ? ex.InnerException : ex;

            MessageBox.Show(exi.Message + "\n" + exi.StackTrace);
        }

        private void HACK_ForceItalianLanguage()
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("it-IT");
            System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("it-IT");
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var ex = e.Exception.InnerException != null ? e.Exception.InnerException : e.Exception;
            MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
        }

        #endregion
    }
}

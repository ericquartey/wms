using System.Configuration;
using System.Windows;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

namespace Ferretto.VW.App
{
    internal class Bootstrapper : UnityBootstrapper
    {
        #region Methods

        public void BindViewModelToView<TViewModel, TView>()
        {
            ViewModelLocationProvider.Register(typeof(TView).ToString(), () => this.Container.Resolve<TViewModel>());
        }

        protected override void ConfigureContainer()
        {
            var automationServiceUrl = ConfigurationManager.AppSettings.Get("AutomationServiceUrl");

            this.Container.RegisterSingleton<IAuthenticationService, AuthenticationService>();
            this.Container.RegisterInstance(ServiceFactory.Get<IThemeService>());
            this.Container.RegisterInstance(ServiceFactory.Get<ISessionService>());
            this.Container.RegisterInstance<IIdentityService>(new IdentityService(automationServiceUrl));

            var wmsServiceUrl = new System.Uri(ConfigurationManager.AppSettings.Get("WMSServiceAddress"));
            this.Container.RegisterInstance(DataServiceFactory.GetService<IUsersDataService>(wmsServiceUrl));

            this.Container.RegisterType<MainWindowViewModel>();

            this.Container.RegisterSingleton<IMainWindow, MainWindow>();

            base.ConfigureContainer();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }

        protected override DependencyObject CreateShell()
        {
            return this.Container.Resolve<IMainWindow>() as DependencyObject;
        }

        protected override void InitializeShell()
        {
            var mainWindowViewModel = this.Container.Resolve<MainWindowViewModel>();
            mainWindowViewModel.InitializeViewModelAsync(this.Container);

            var application = Application.Current as App;
            application.MainWindow.DataContext = mainWindowViewModel;

            application.MainWindow.Show();
        }

        #endregion
    }
}

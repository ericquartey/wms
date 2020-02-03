using System.Linq;
using System.Windows;
using Ferretto.VW.Simulator.Services;
using Ferretto.VW.Simulator.Services.Interfaces;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

namespace Ferretto.VW.Simulator
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
            this.Container.RegisterInstance(ServiceFactory.Get<IThemeService>());
            this.Container.RegisterInstance(ServiceFactory.Get<IMachineService>());
            //this.Container.RegisterInstance<IIdentityService>(new IdentityService(automationServiceUrl));

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

            var application = Application.Current as App;
            application.MainWindow.DataContext = mainWindowViewModel;

            if (System.Environment.GetCommandLineArgs().Any(a => a.Equals("--configuration", System.StringComparison.InvariantCultureIgnoreCase)))
            {
                var filePath = System.Environment
                    .GetCommandLineArgs()
                    .SkipWhile(a => !a.Equals("--configuration", System.StringComparison.InvariantCultureIgnoreCase))
                    .Skip(1)
                    .FirstOrDefault();

                mainWindowViewModel.LoadConfiguration(filePath);
            }

            if (System.Environment.GetCommandLineArgs().Any(a => a.Equals("--autostart", System.StringComparison.InvariantCultureIgnoreCase)))
            {
                mainWindowViewModel.StartSimulatorCommand.Execute(null);
            }

            application.MainWindow.Show();
        }

        #endregion
    }
}

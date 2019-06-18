using System.Windows;
using Ferretto.VW.InstallationApp;
using Ferretto.VW.OperatorApp.Resources;
using Prism.Events;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

namespace Ferretto.VW.VWApp
{
    internal class Bootstrapper : UnityBootstrapper
    {
        #region Methods

        public void BindViewModelToView<TViewModel, TView>()
        {
            ViewModelLocationProvider.Register(typeof(TView).ToString(), () => this.Container.Resolve<TViewModel>());
        }

        protected override void ConfigureModuleCatalog()
        {
            var catalog = (ModuleCatalog)this.ModuleCatalog;
            catalog.AddModule(typeof(VWAppModule));
            catalog.AddModule(typeof(InstallationAppModule));
            catalog.AddModule(typeof(OperatorAppModule));
        }

        protected override void ConfigureViewModelLocator()
        {
            this.BindViewModelToView<InstallationApp.IMainWindowViewModel, InstallationApp.MainWindow>();

            this.BindViewModelToView<OperatorApp.Interfaces.IMainWindowViewModel, OperatorApp.MainWindow>();
        }

        protected override DependencyObject CreateShell()
        {
            this.InitializeMainWindow();

            return (MainWindow)this.Container.Resolve<IMainWindow>();
        }

        protected override void InitializeShell()
        {
            ((MainWindowViewModel)((App)Application.Current).MainWindow.DataContext).Container = this.Container;
            Application.Current.MainWindow.Show();
        }

        private void InitializeMainWindow()
        {
            var MainWindowVInstance = new MainWindow();
            var MainWindowVMInstance = new MainWindowViewModel(this.Container.Resolve<IEventAggregator>());

            MainWindowVMInstance.InitializeViewModel(this.Container);
            MainWindowVInstance.DataContext = MainWindowVMInstance;
            this.Container.RegisterInstance<IMainWindow>(MainWindowVInstance);
        }

        #endregion
    }
}

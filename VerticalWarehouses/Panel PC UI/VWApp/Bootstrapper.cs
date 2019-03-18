using System.Windows;
using Ferretto.VW.InstallationApp;
using Ferretto.VW.Utils.Source;
using Ferretto.VW.OperatorApp.Resources;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;

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
            this.BindViewModelToView<IBeltBurnishingViewModel, BeltBurnishingView>();
            this.BindViewModelToView<IMainWindowBackToIAPPButtonViewModel, MainWindowBackToIAPPButtonView>();
            this.BindViewModelToView<IResolutionCalibrationVerticalAxisViewModel, ResolutionCalibrationVerticalAxisView>();
            this.BindViewModelToView<IMainWindowNavigationButtonsViewModel, MainWindowNavigationButtonsView>();
            this.BindViewModelToView<ICellsPanelsControlViewModel, CellsPanelsControlView>();
            this.BindViewModelToView<ICellsControlViewModel, CellsControlView>();
            this.BindViewModelToView<IIdleViewModel, IdleView>();
            this.BindViewModelToView<IShutter1ControlViewModel, Shutter1ControlView>();
            this.BindViewModelToView<IShutter2ControlViewModel, Shutter2ControlView>();
            this.BindViewModelToView<IShutter3ControlViewModel, Shutter3ControlView>();
            this.BindViewModelToView<IShutter1HeightControlViewModel, Shutter1HeightControlView>();
            this.BindViewModelToView<IShutter2HeightControlViewModel, Shutter2HeightControlView>();
            this.BindViewModelToView<IShutter3HeightControlViewModel, Shutter3HeightControlView>();
            this.BindViewModelToView<IInstallationStateViewModel, InstallationStateView>();
            this.BindViewModelToView<ILSMTShutterEngineViewModel, LSMTShutterEngineView>();
            this.BindViewModelToView<ILSMTHorizontalEngineViewModel, LSMTHorizontalEngineView>();
            this.BindViewModelToView<ILSMTMainViewModel, LSMTMainView>();
            this.BindViewModelToView<ILSMTNavigationButtonsViewModel, LSMTNavigationButtonsView>();
            this.BindViewModelToView<ILSMTVerticalEngineViewModel, LSMTVerticalEngineView>();
            this.BindViewModelToView<ISSBaysViewModel, SSBaysView>();
            this.BindViewModelToView<ISSBaysViewModel, SSCradleView>();
            this.BindViewModelToView<ISSShutterViewModel, SSShutterView>();
            this.BindViewModelToView<ISSMainViewModel, SSMainView>();
            this.BindViewModelToView<ISSNavigationButtonsViewModel, SSNavigationButtonsView>();
            this.BindViewModelToView<ISSVariousInputsViewModel, SSVariousInputsView>();
            this.BindViewModelToView<ISSVerticalAxisViewModel, SSVerticalAxisView>();
            this.BindViewModelToView<IVerticalAxisCalibrationViewModel, VerticalAxisCalibrationView>();
            this.BindViewModelToView<IVerticalOffsetCalibrationViewModel, VerticalOffsetCalibrationView>();
            this.BindViewModelToView<IWeightControlViewModel, WeightControlView>();
            this.BindViewModelToView<ILSMTCarouselViewModel, LSMTCarouselView>();
            this.BindViewModelToView<ISaveRestoreConfigViewModel, SaveRestoreConfigView>();
            this.BindViewModelToView<InstallationApp.IMainWindowViewModel, InstallationApp.MainWindow>();

            this.BindViewModelToView<OperatorApp.ViewsAndViewModels.Interfaces.IMainWindowViewModel, OperatorApp.MainWindow>();
        }

        protected override DependencyObject CreateShell()
        {
            this.InitializeData();
            this.InitializeMainWindow();

            return (MainWindow)this.Container.Resolve<IMainWindow>();
        }

        protected override void InitializeShell()
        {
            ((MainWindowViewModel)((App)Application.Current).MainWindow.DataContext).Container = this.Container;
            Application.Current.MainWindow.Show();
        }

        private void InitializeData()
        {
            var data = new DataManager();
            this.Container.RegisterInstance<IDataManager>(data);
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

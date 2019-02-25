using System.Windows;
using Ferretto.VW.ActionBlocks.Source.ActionsBasic;
using Ferretto.VW.InstallationApp;
using Ferretto.VW.Navigation;
using Ferretto.VW.Utils.Source;
using Ferretto.VW.VWApp.Interfaces;
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
            this.BindViewModelToView<IGate1ControlViewModel, Gate1ControlView>();
            this.BindViewModelToView<IGate2ControlViewModel, Gate2ControlView>();
            this.BindViewModelToView<IGate3ControlViewModel, Gate3ControlView>();
            this.BindViewModelToView<IGate1HeightControlViewModel, Gate1HeightControlView>();
            this.BindViewModelToView<IGate2HeightControlViewModel, Gate2HeightControlView>();
            this.BindViewModelToView<IGate3HeightControlViewModel, Gate3HeightControlView>();
            this.BindViewModelToView<IInstallationStateViewModel, InstallationStateView>();
            this.BindViewModelToView<ILSMTGateEngineViewModel, LSMTGateEngineView>();
            this.BindViewModelToView<ILSMTHorizontalEngineViewModel, LSMTHorizontalEngineView>();
            this.BindViewModelToView<ILSMTMainViewModel, LSMTMainView>();
            this.BindViewModelToView<ILSMTNavigationButtonsViewModel, LSMTNavigationButtonsView>();
            this.BindViewModelToView<ILSMTVerticalEngineViewModel, LSMTVerticalEngineView>();
            this.BindViewModelToView<ISSBaysViewModel, SSBaysView>();
            this.BindViewModelToView<ISSBaysViewModel, SSCradleView>();
            this.BindViewModelToView<ISSGateViewModel, SSGateView>();
            this.BindViewModelToView<ISSMainViewModel, SSMainView>();
            this.BindViewModelToView<ISSNavigationButtonsViewModel, SSNavigationButtonsView>();
            this.BindViewModelToView<ISSVariousInputsViewModel, SSVariousInputsView>();
            this.BindViewModelToView<ISSVerticalAxisViewModel, SSVerticalAxisView>();
            this.BindViewModelToView<IVerticalAxisCalibrationViewModel, VerticalAxisCalibrationView>();
            this.BindViewModelToView<IVerticalOffsetCalibrationViewModel, VerticalOffsetCalibrationView>();
            this.BindViewModelToView<IWeightControlViewModel, WeightControlView>();
            this.BindViewModelToView<IMainWindowViewModel, InstallationApp.MainWindow>();
        }

        protected override DependencyObject CreateShell()
        {
            NavigationService.InitializeEvents();
            NavigationService.ChangeSkinToDarkEventHandler += (Application.Current as App).ChangeSkin;
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

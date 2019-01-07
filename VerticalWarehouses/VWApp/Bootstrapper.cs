using Microsoft.Practices.Unity;
using Prism.Unity;
using System.Windows;
using Prism.Modularity;
using Ferretto.VW.InstallationApp;
using Ferretto.VW.Navigation;
using Ferretto.VW.Utils.Source;
using Ferretto.VW.InverterDriver;
using Prism.Mvvm;
using Ferretto.VW.ActionBlocks;
using Ferretto.VW.MathLib;
using Ferretto.VW.ActionBlocks.Source.ActionsBasic;

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
            catalog.AddModule(typeof(InstallationAppModule));
            catalog.AddModule(typeof(ActionsModule));
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
            this.InitializeInverter();
            this.InitializeActions();
            this.InitializeMainWindow();

            return (MainWindow)this.Container.Resolve<IMainWindow>();
        }

        protected override void InitializeShell()
        {
            ((MainWindowViewModel)((App)Application.Current).MainWindow.DataContext).Container = this.Container;
            Application.Current.MainWindow.Show();
        }

        private void InitializeActions()
        {
            var positioning = new PositioningDrawer();
            var drawerWeightDetection = new DrawerWeightDetection();
            var converter = new Converter();
            var calibrateVertical = new CalibrateVerticalAxis();

            this.Container.RegisterInstance<IPositioningDrawer>(positioning);
            this.Container.RegisterInstance<IDrawerWeightDetection>(drawerWeightDetection);
            this.Container.RegisterInstance<IConverter>(converter);
            this.Container.RegisterInstance<ICalibrateVerticalAxis>(calibrateVertical);
        }

        private void InitializeData()
        {
            var data = new DataManager();
            this.Container.RegisterInstance<IDataManager>(data);
        }

        private void InitializeInverter()
        {
            var inverter = new InverterDriver.InverterDriver();
            inverter.Initialize();
            this.Container.RegisterInstance<IInverterDriver>(inverter);
        }

        private void InitializeIODevice()
        {
        }

        private void InitializeMainWindow()
        {
            var MainWindowVInstance = new MainWindow();
            var MainWindowVMInstance = new MainWindowViewModel();
            MainWindowVMInstance.InitializeViewModel(this.Container);
            MainWindowVInstance.DataContext = MainWindowVMInstance;
            this.Container.RegisterInstance<IMainWindow>(MainWindowVInstance);
        }

        #endregion Methods
    }
}

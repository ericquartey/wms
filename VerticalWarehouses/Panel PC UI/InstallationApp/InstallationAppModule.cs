using System.Windows;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Views.ErrorDetails;
using Ferretto.VW.App.Installation.HelpWindows;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.App.Installation.ViewsAndViewModels;
using Ferretto.VW.App.Installation.ViewsAndViewModels.LowSpeedMovements;
using Ferretto.VW.App.Installation.ViewsAndViewModels.SensorsState;
using Ferretto.VW.App.Installation.ViewsAndViewModels.ShuttersControl;
using Ferretto.VW.App.Installation.ViewsAndViewModels.ShuttersHeightControl;
using Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Installation
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "This is a container initialization class, so it is ok to be coupled to many types.")]
    public class InstallationAppModule : IModule
    {
        #region Fields

        private readonly IUnityContainer container;

        #endregion

        #region Constructors

        public InstallationAppModule(IUnityContainer container)
        {
            this.container = container;
        }

        #endregion

        #region Methods

        public static void BindViewModelToView<TViewModel, TView>(IContainerProvider containerProvider)
        {
            ViewModelLocationProvider.Register(
                typeof(TView).ToString(),
                () => containerProvider.Resolve<TViewModel>());
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            BindViewModelToView<IMainWindowViewModel, MainWindow>(containerProvider);

            var mainWindow = (MainWindow)containerProvider.Resolve<IMainWindow>();
            var mainWindowViewModel = containerProvider.Resolve<IMainWindowViewModel>();

            mainWindow.DataContext = mainWindowViewModel;
            mainWindowViewModel.OnEnterViewAsync();

            var mainWindowProperty = Application.Current.GetType().GetProperty("InstallationAppMainWindowInstance");
            mainWindowProperty.SetValue(Application.Current, mainWindow);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            this.container.RegisterSingleton<IMainWindow, MainWindow>();

            this.container.RegisterSingleton<IHelpMainWindow, HelpMainWindow>();

            this.container.RegisterInstance<IStatusMessageService>(new StatusMessageService());

            this.container.RegisterSingleton<IMainWindowViewModel, MainWindowViewModel>();
            this.container.RegisterSingleton<INotificationService, NotificationService>();
            this.container.Resolve<INotificationService>(); // HACK this is to force the instantiation of the notification service

            this.container.RegisterSingleton<IFooterViewModel, FooterViewModel>();
            this.container.RegisterSingleton<ErrorDetailsViewModel>();

            this.container.RegisterSingleton<IBeltBurnishingViewModel, BeltBurnishingViewModel>();
            this.container.RegisterSingleton<ICellsControlViewModel, CellsControlViewModel>();
            this.container.RegisterSingleton<ICellsPanelsControlViewModel, CellsPanelsControlViewModel>();
            this.container.RegisterSingleton<IShutter1ControlViewModel, Shutter1ControlViewModel>();
            this.container.RegisterSingleton<IShutter2ControlViewModel, Shutter2ControlViewModel>();
            this.container.RegisterSingleton<IShutter3ControlViewModel, Shutter3ControlViewModel>();
            this.container.RegisterSingleton<IShutter1HeightControlViewModel, Shutter1HeightControlViewModel>();
            this.container.RegisterSingleton<IShutter2HeightControlViewModel, Shutter2HeightControlViewModel>();
            this.container.RegisterSingleton<IShutter3HeightControlViewModel, Shutter3HeightControlViewModel>();
            this.container.RegisterSingleton<IIdleViewModel, IdleViewModel>();
            this.container.RegisterSingleton<IInstallationStateViewModel, InstallationStateViewModel>();
            this.container.RegisterSingleton<ILSMTShutterEngineViewModel, LSMTShutterEngineViewModel>();
            this.container.RegisterSingleton<ILSMTHorizontalEngineViewModel, LSMTHorizontalEngineViewModel>();
            this.container.RegisterSingleton<ILSMTNavigationButtonsViewModel, LSMTNavigationButtonsViewModel>();
            this.container.RegisterSingleton<ILSMTMainViewModel, LSMTMainViewModel>();
            this.container.RegisterSingleton<ILSMTVerticalEngineViewModel, LSMTVerticalEngineViewModel>();
            this.container.RegisterSingleton<IMainWindowNavigationButtonsViewModel, MainWindowNavigationButtonsViewModel>();
            this.container.RegisterSingleton<IResolutionCalibrationVerticalAxisViewModel, ResolutionCalibrationVerticalAxisViewModel>();
            this.container.RegisterSingleton<ISSBaysViewModel, SSBaysViewModel>();
            this.container.RegisterSingleton<ISSCradleViewModel, SSCradleViewModel>();
            this.container.RegisterSingleton<ISSShutterViewModel, SSShutterViewModel>();
            this.container.RegisterSingleton<ISSMainViewModel, SSMainViewModel>();
            this.container.RegisterSingleton<ISSNavigationButtonsViewModel, SSNavigationButtonsViewModel>();
            this.container.RegisterSingleton<ISSVariousInputsViewModel, SSVariousInputsViewModel>();
            this.container.RegisterSingleton<ISSVerticalAxisViewModel, SSVerticalAxisViewModel>();
            this.container.RegisterSingleton<IVerticalAxisCalibrationViewModel, VerticalAxisCalibrationViewModel>();
            this.container.RegisterSingleton<IVerticalOffsetCalibrationViewModel, VerticalOffsetCalibrationViewModel>();
            this.container.RegisterSingleton<IWeightControlViewModel, WeightControlViewModel>();
            this.container.RegisterSingleton<IBayControlViewModel, BayControlViewModel>();
            this.container.RegisterSingleton<ILoadFirstDrawerViewModel, LoadFirstDrawerViewModel>();
            this.container.RegisterSingleton<ILoadingDrawersViewModel, LoadingDrawersViewModel>();
            this.container.RegisterSingleton<ISaveRestoreConfigViewModel, SaveRestoreConfigViewModel>();
            this.container.RegisterSingleton<ICellsSideControlViewModel, CellsSideControlViewModel>();
            this.container.RegisterSingleton<IDrawerLoadingUnloadingTestViewModel, DrawerLoadingUnloadingTestViewModel>();
            this.container.RegisterSingleton<ILSMTCarouselViewModel, LSMTCarouselViewModel>();
            this.container.RegisterSingleton<IDrawerStoreRecallViewModel, DrawerStoreRecallViewModel>();

            this.container.RegisterSingleton<ICustomShutterControlSensorsThreePositionsViewModel, CustomShutterControlSensorsThreePositionsViewModel>();
            this.container.RegisterSingleton<ICustomShutterControlSensorsTwoPositionsViewModel, CustomShutterControlSensorsTwoPositionsViewModel>();
        }

        #endregion
    }
}

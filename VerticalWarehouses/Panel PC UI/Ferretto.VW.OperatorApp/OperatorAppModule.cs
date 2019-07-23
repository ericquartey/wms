using System.Configuration;
using System.Windows;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.OperatorApp;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations.Details;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Other;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.SearchItem;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists.ListDetail;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Operator
{
    public class OperatorAppModule : IModule
    {
        #region Fields

        private readonly string automationServiceUrl = ConfigurationManager.AppSettings.Get("AutomationServiceUrl");

        private readonly IUnityContainer container;

        #endregion

        #region Constructors

        public OperatorAppModule(IUnityContainer container)
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
            mainWindow.DataContext = containerProvider.Resolve<IMainWindowViewModel>();

            var mainWindowProperty = Application.Current.GetType().GetProperty("OperatorAppMainWindowInstance");
            mainWindowProperty.SetValue(Application.Current, mainWindow);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            this.RegisterMachineAutomationServiceWebApis(containerRegistry);

            containerRegistry.RegisterSingleton<IMainWindowViewModel, MainWindowViewModel>();
            containerRegistry.RegisterSingleton<IMainWindow, MainWindow>();
            containerRegistry.RegisterSingleton<IHelpMainWindow, HelpMainWindow>();
            containerRegistry.RegisterSingleton<IBayManager, BayManager>();
            containerRegistry.RegisterSingleton<IStatusMessageService, StatusMessageService>();
            containerRegistry.RegisterSingleton<INavigationService, NavigationService>();

            containerRegistry.RegisterSingleton<IIdleViewModel, IdleViewModel>();
            containerRegistry.RegisterSingleton<IFooterViewModel, FooterViewModel>();
            containerRegistry.RegisterSingleton<IMainWindowNavigationButtonsViewModel, MainWindowNavigationButtonsViewModel>();
            containerRegistry.RegisterSingleton<IDrawerActivityPickingViewModel, DrawerActivityPickingViewModel>();
            containerRegistry.RegisterSingleton<IDrawerActivityPickingDetailViewModel, DrawerActivityPickingDetailViewModel>();
            containerRegistry.RegisterSingleton<IDrawerWaitViewModel, DrawerWaitViewModel>();
            containerRegistry.RegisterSingleton<IListsInWaitViewModel, ListsInWaitViewModel>();
            containerRegistry.RegisterSingleton<IItemSearchViewModel, ItemSearchViewModel>();
            containerRegistry.RegisterSingleton<IItemDetailViewModel, ItemDetailViewModel>();
            containerRegistry.RegisterSingleton<IImmediateDrawerCallViewModel, ImmediateDrawerCallViewModel>();
            containerRegistry.RegisterSingleton<IGeneralInfoViewModel, GeneralInfoViewModel>();
            containerRegistry.RegisterSingleton<IDrawerCompactingViewModel, DrawerCompactingViewModel>();
            containerRegistry.RegisterSingleton<IDrawerCompactingDetailViewModel, DrawerCompactingDetailViewModel>();
            containerRegistry.RegisterSingleton<IOtherNavigationViewModel, OtherNavigationViewModel>();
            containerRegistry.RegisterSingleton<IStatisticsGeneralDataViewModel, StatisticsGeneralDataViewModel>();
            containerRegistry.RegisterSingleton<IDrawerSpaceSaturationViewModel, DrawerSpaceSaturationViewModel>();
            containerRegistry.RegisterSingleton<IDetailListInWaitViewModel, DetailListInWaitViewModel>();
            containerRegistry.RegisterSingleton<ICellsStatisticsViewModel, CellsStatisticsViewModel>();
            containerRegistry.RegisterSingleton<IErrorsStatisticsViewModel, ErrorsStatisticsViewModel>();
            containerRegistry.RegisterSingleton<IMaintenanceDetailViewModel, MaintenanceDetailViewModel>();
            containerRegistry.RegisterSingleton<IMaintenanceMainPageViewModel, MaintenanceMainPageViewModel>();
            containerRegistry.RegisterSingleton<IStatisticsNavigationViewModel, StatisticsNavigationViewModel>();
            containerRegistry.RegisterSingleton<IDrawerActivityRefillingViewModel, DrawerActivityRefillingViewModel>();
            containerRegistry.RegisterSingleton<IDrawerActivityRefillingDetailViewModel, DrawerActivityRefillingDetailViewModel>();
            containerRegistry.RegisterSingleton<IDrawerWeightSaturationViewModel, DrawerWeightSaturationViewModel>();
            containerRegistry.RegisterSingleton<IDrawerActivityInventoryViewModel, DrawerActivityInventoryViewModel>();
            containerRegistry.RegisterSingleton<IDrawerActivityInventoryDetailViewModel, DrawerActivityInventoryDetailViewModel>();
            containerRegistry.RegisterSingleton<IItemSearchViewModel, ItemSearchViewModel>();
            containerRegistry.RegisterSingleton<IMachineStatisticsViewModel, MachineStatisticsViewModel>();

            containerRegistry.Register<ICustomControlArticleDataGridViewModel, CustomControlArticleDataGridViewModel>();
            containerRegistry.Register<ICustomControlCellStatisticsDataGridViewModel, CustomControlCellStatisticsDataGridViewModel>();
            containerRegistry.Register<ICustomControlDrawerSaturationDataGridViewModel, CustomControlDrawerSaturationDataGridViewModel>();
            containerRegistry.Register<ICustomControlDrawerDataGridViewModel, CustomControlDrawerDataGridViewModel>();
            containerRegistry.Register<ICustomControlErrorsDataGridViewModel, CustomControlErrorsDataGridViewModel>();
            containerRegistry.Register<ICustomControlListDataGridViewModel, CustomControlListDataGridViewModel>();
            containerRegistry.Register<ICustomControlMaintenanceDataGridViewModel, CustomControlMaintenanceDataGridViewModel>();
            containerRegistry.Register<ICustomControlDrawerWeightSaturationDataGridViewModel, CustomControlDrawerWeightSaturationDataGridViewModel>();
            containerRegistry.Register<ICustomControlListDetailDataGridViewModel, CustomControlListDetailDataGridViewModel>();
            containerRegistry.Register<ICustomControlMaintenanceDetailDataGridViewModel, CustomControlMaintenanceDetailDataGridViewModel>();
        }

        private void RegisterMachineAutomationServiceWebApis(IContainerRegistry containerRegistry)
        {
            var missionOperationsService = new MissionOperationsService(this.automationServiceUrl);
            containerRegistry.RegisterInstance<IMissionOperationsService>(missionOperationsService);

            var loadingUnitsService = new LoadingUnitsService(this.automationServiceUrl);
            containerRegistry.RegisterInstance<ILoadingUnitsService>(loadingUnitsService);

            var cellsService = new CellsService(this.automationServiceUrl);
            containerRegistry.RegisterInstance<ICellsService>(cellsService);

            var errorsService = new ErrorsService(this.automationServiceUrl);
            containerRegistry.RegisterInstance<IErrorsService>(errorsService);

            var machineStatisticsService = new MachineStatisticsService(this.automationServiceUrl);
            containerRegistry.RegisterInstance<IMachineStatisticsService>(machineStatisticsService);
        }

        #endregion
    }
}

using System.Configuration;
using System.Windows;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Modules.Operator.HelpWindows;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Modules.Operator.ViewsAndViewModels;
using Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.DrawerOperations;
using Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.DrawerOperations.Details;
using Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.Other;
using Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.Other.Statistics;
using Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.SearchItem;
using Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.WaitingLists;
using Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.WaitingLists.ListDetail;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Modules.Operator
{
    [Module(ModuleName = nameof(Utils.Modules.Operator), OnDemand = true)]
    [ModuleDependency(nameof(Utils.Modules.Errors))]
    public class OperatorAppModule : IModule
    {
        #region Fields

        private readonly string automationServiceUrl = ConfigurationManager.AppSettings.Get("AutomationService:Url");

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
            containerRegistry.RegisterSingleton<IMainWindowViewModel, MainWindowViewModel>();
            containerRegistry.RegisterSingleton<IMainWindow, MainWindow>();
            containerRegistry.RegisterSingleton<IHelpMainWindow, HelpMainWindow>();
            containerRegistry.RegisterSingleton<IStatusMessageService, StatusMessageService>();
            containerRegistry.RegisterSingleton<Interfaces.INavigationService, NavigationService>();

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

            containerRegistry.Register<ICustomControlCellStatisticsDataGridViewModel, CustomControlCellStatisticsDataGridViewModel>();
            containerRegistry.Register<ICustomControlDrawerSaturationDataGridViewModel, CustomControlDrawerSaturationDataGridViewModel>();
            containerRegistry.Register<ICustomControlDrawerDataGridViewModel, CustomControlDrawerDataGridViewModel>();
            containerRegistry.Register<ICustomControlErrorsDataGridViewModel, CustomControlErrorsDataGridViewModel>();
            containerRegistry.Register<ICustomControlMaintenanceDataGridViewModel, CustomControlMaintenanceDataGridViewModel>();
            containerRegistry.Register<ICustomControlDrawerWeightSaturationDataGridViewModel, CustomControlDrawerWeightSaturationDataGridViewModel>();
            containerRegistry.Register<ICustomControlListDetailDataGridViewModel, CustomControlListDetailDataGridViewModel>();
            containerRegistry.Register<ICustomControlMaintenanceDetailDataGridViewModel, CustomControlMaintenanceDetailDataGridViewModel>();
        }

        #endregion
    }
}

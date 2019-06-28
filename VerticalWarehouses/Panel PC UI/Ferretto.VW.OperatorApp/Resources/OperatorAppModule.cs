using Ferretto.VW.OperatorApp.Interfaces;
using Unity;
using Prism.Modularity;
using Prism.Events;
using Ferretto.VW.OperatorApp.ViewsAndViewModels;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Other;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations.Details;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists.ListDetail;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.SearchItem;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.CustomControls.Controls;
using System.Configuration;
using Ferretto.VW.OperatorApp.ServiceUtilities;
using Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces;
using Prism.Ioc;
using Ferretto.VW.MAS_AutomationService.Contracts;

namespace Ferretto.VW.OperatorApp.Resources
{
    public class OperatorAppModule : IModule
    {
        #region Fields

        private readonly string automationServiceUrl = ConfigurationManager.AppSettings.Get("AutomationServiceUrl");

        private readonly string operatorHubPath = ConfigurationManager.AppSettings.Get("OperatorHubEndpoint");

        private IUnityContainer container;

        #endregion

        #region Constructors

        public OperatorAppModule(IUnityContainer container)
        {
            this.container = container;

            var navigationServiceInstance = new NavigationService(this.container.Resolve<IEventAggregator>());

            var mainWindowVMInstance = new MainWindowViewModel(this.container.Resolve<IEventAggregator>());
            var mainWindowInstance = new MainWindow(this.container.Resolve<IEventAggregator>());
            var helpMainWindowInstance = new HelpMainWindow(container.Resolve<IEventAggregator>());
            var operatorHubClientInstance = new OperatorHubClient(this.automationServiceUrl, this.operatorHubPath);
            var bayManagerInstance = new BayManager(container.Resolve<IEventAggregator>());
            var operatorService = new OperatorService(this.automationServiceUrl);
            var feedbackNotifier = new FeedbackNotifier();

            var idleVMInstance = new IdleViewModel(container.Resolve<IEventAggregator>());
            var mainWindowBackToOAPPButtonVMInstance = new MainWindowBackToOAPPButtonViewModel(this.container.Resolve<IEventAggregator>());
            var mainWindowNavigationButtonsVMInstance = new MainWindowNavigationButtonsViewModel(this.container.Resolve<IEventAggregator>());
            var drawerActivityPickingVMInstance = new DrawerActivityPickingViewModel(container.Resolve<IEventAggregator>());
            var drawerWaitVMInstance = new DrawerWaitViewModel(container.Resolve<IEventAggregator>());
            var drawerActivityPickingDetailVMInstance = new DrawerActivityPickingDetailViewModel(container.Resolve<IEventAggregator>());
            var listsInWaitVMInstance = new ListsInWaitViewModel(container.Resolve<IEventAggregator>());
            var itemSearchVMInstance = new ItemSearchViewModel(container.Resolve<IEventAggregator>());
            var itemDetailVMInstance = new ItemDetailViewModel(container.Resolve<IEventAggregator>());
            var immediateDrawerCallVMInstance = new ImmediateDrawerCallViewModel(container.Resolve<IEventAggregator>());
            var generalInfoVMInstance = new GeneralInfoViewModel(container.Resolve<IEventAggregator>());
            var drawerCompactingVMInstance = new DrawerCompactingViewModel(container.Resolve<IEventAggregator>());
            var otherNavigationVMInstance = new OtherNavigationViewModel(container.Resolve<IEventAggregator>());
            var statisticsGeneralDataVMInstance = new StatisticsGeneralDataViewModel(container.Resolve<IEventAggregator>());
            var itemStatisticsVMInstance = new ItemStatisticsViewModel(container.Resolve<IEventAggregator>());
            var drawerSpaceSaturationVMInstance = new DrawerSpaceSaturationViewModel(container.Resolve<IEventAggregator>());
            var detailListInWaitVMInstance = new DetailListInWaitViewModel(container.Resolve<IEventAggregator>());
            var cellsStatisticsVMInstance = new CellsStatisticsViewModel(container.Resolve<IEventAggregator>());
            var errorsStatisticsVMInstance = new ErrorsStatisticsViewModel(container.Resolve<IEventAggregator>());
            var maintenanceMainPageVMInstance = new MaintenanceMainPageViewModel(container.Resolve<IEventAggregator>());
            var maintenanceDetailVMInstance = new MaintenanceDetailViewModel(container.Resolve<IEventAggregator>());
            var statisticsNavigationVMInstance = new StatisticsNavigationViewModel(container.Resolve<IEventAggregator>());
            var drawerWeightSaturationVMInstance = new DrawerWeightSaturationViewModel(container.Resolve<IEventAggregator>());
            var drawerActivityRefillingVMInstance = new DrawerActivityRefillingViewModel(container.Resolve<IEventAggregator>());
            var drawerActivityRefillingDetailVMInstance = new DrawerActivityRefillingDetailViewModel(container.Resolve<IEventAggregator>());
            var drawerActivityInventoryVMInstance = new DrawerActivityInventoryViewModel(container.Resolve<IEventAggregator>());
            var drawerActivityInventoryDetailVMInstance = new DrawerActivityInventoryDetailViewModel(container.Resolve<IEventAggregator>());

            this.container.RegisterInstance<IMainWindowViewModel>(mainWindowVMInstance);
            this.container.RegisterInstance<IOperatorHubClient>(operatorHubClientInstance);
            this.container.RegisterInstance<IMainWindow>(mainWindowInstance);
            this.container.RegisterInstance<IHelpMainWindow>(helpMainWindowInstance);
            this.container.RegisterInstance<IBayManager>(bayManagerInstance);
            this.container.RegisterInstance<IOperatorService>(operatorService);
            this.container.RegisterInstance<IFeedbackNotifier>(feedbackNotifier);

            this.container.RegisterInstance<IIdleViewModel>(idleVMInstance);
            this.container.RegisterInstance<IMainWindowBackToOAPPButtonViewModel>(mainWindowBackToOAPPButtonVMInstance);
            this.container.RegisterInstance<IMainWindowNavigationButtonsViewModel>(mainWindowNavigationButtonsVMInstance);
            this.container.RegisterInstance<IDrawerActivityPickingViewModel>(drawerActivityPickingVMInstance);
            this.container.RegisterInstance<IDrawerActivityPickingDetailViewModel>(drawerActivityPickingDetailVMInstance);
            this.container.RegisterInstance<IDrawerWaitViewModel>(drawerWaitVMInstance);
            this.container.RegisterInstance<IListsInWaitViewModel>(listsInWaitVMInstance);
            this.container.RegisterInstance<IItemSearchViewModel>(itemSearchVMInstance);
            this.container.RegisterInstance<IItemDetailViewModel>(itemDetailVMInstance);
            this.container.RegisterInstance<IImmediateDrawerCallViewModel>(immediateDrawerCallVMInstance);
            this.container.RegisterInstance<IGeneralInfoViewModel>(generalInfoVMInstance);
            this.container.RegisterInstance<IDrawerCompactingViewModel>(drawerCompactingVMInstance);
            this.container.RegisterInstance<IOtherNavigationViewModel>(otherNavigationVMInstance);
            this.container.RegisterInstance<IStatisticsGeneralDataViewModel>(statisticsGeneralDataVMInstance);
            this.container.RegisterInstance<IItemStatisticsViewModel>(itemStatisticsVMInstance);
            this.container.RegisterInstance<IDrawerSpaceSaturationViewModel>(drawerSpaceSaturationVMInstance);
            this.container.RegisterInstance<IDetailListInWaitViewModel>(detailListInWaitVMInstance);
            this.container.RegisterInstance<ICellsStatisticsViewModel>(cellsStatisticsVMInstance);
            this.container.RegisterInstance<IErrorsStatisticsViewModel>(errorsStatisticsVMInstance);
            this.container.RegisterInstance<IMaintenanceDetailViewModel>(maintenanceDetailVMInstance);
            this.container.RegisterInstance<IMaintenanceMainPageViewModel>(maintenanceMainPageVMInstance);
            this.container.RegisterInstance<IStatisticsNavigationViewModel>(statisticsNavigationVMInstance);
            this.container.RegisterInstance<IDrawerActivityRefillingViewModel>(drawerActivityRefillingVMInstance);
            this.container.RegisterInstance<IDrawerActivityRefillingDetailViewModel>(drawerActivityRefillingDetailVMInstance);
            this.container.RegisterInstance<IDrawerWeightSaturationViewModel>(drawerWeightSaturationVMInstance);
            this.container.RegisterInstance<IDrawerActivityInventoryViewModel>(drawerActivityInventoryVMInstance);
            this.container.RegisterInstance<IDrawerActivityInventoryDetailViewModel>(drawerActivityInventoryDetailVMInstance);
            this.container.RegisterInstance<IItemSearchViewModel>(itemSearchVMInstance);

            this.container.RegisterType<ICustomControlArticleDataGridViewModel, CustomControlArticleDataGridViewModel>();
            this.container.RegisterType<ICustomControlCellStatisticsDataGridViewModel, CustomControlCellStatisticsDataGridViewModel>();
            this.container.RegisterType<ICustomControlDrawerSaturationDataGridViewModel, CustomControlDrawerSaturationDataGridViewModel>();

            navigationServiceInstance.Initialize(this.container);
            feedbackNotifier.Initialize(this.container);

            mainWindowVMInstance.InitializeViewModel(this.container);
            mainWindowBackToOAPPButtonVMInstance.InitializeViewModel(this.container);
            mainWindowNavigationButtonsVMInstance.InitializeViewModel(this.container);
            drawerWaitVMInstance.InitializeViewModel(this.container);
            drawerActivityPickingVMInstance.InitializeViewModel(this.container);
            drawerActivityRefillingVMInstance.InitializeViewModel(this.container);
            otherNavigationVMInstance.InitializeViewModel(this.container);
            generalInfoVMInstance.InitializeViewModel(this.container);
            statisticsGeneralDataVMInstance.InitializeViewModel(this.container);
            itemSearchVMInstance.InitializeViewModel(this.container);
            itemDetailVMInstance.InitializeViewModel(this.container);
            cellsStatisticsVMInstance.InitializeViewModel(this.container);
            drawerSpaceSaturationVMInstance.InitializeViewModel(this.container);

            bayManagerInstance.Initialize(this.container);
            mainWindowBackToOAPPButtonVMInstance.InitializeButtons();
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            // HACK IModule interface requires the implementation of this method
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // HACK IModule interface requires the implementation of this method
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // HACK IModule interface requires the implementation of this method
        }

        #endregion
    }
}

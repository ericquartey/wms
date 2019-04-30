using Ferretto.VW.OperatorApp.Interfaces;
using Microsoft.Practices.Unity;
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
using Ferretto.VW.OperatorApp.Interfaces;

namespace Ferretto.VW.OperatorApp.Resources
{
    public class OperatorAppModule : IModule
    {
        #region Fields

        private IUnityContainer container;

        #endregion

        #region Constructors

        public OperatorAppModule(IUnityContainer container)
        {
            this.container = container;

            var navigationServiceInstance = new NavigationService(this.container.Resolve<IEventAggregator>());

            var mainWindowVMInstance = new MainWindowViewModel(this.container.Resolve<IEventAggregator>());
            var mainWindowInstance = new MainWindow(this.container.Resolve<IEventAggregator>());
            var idleVMInstance = new IdleViewModel(container.Resolve<IEventAggregator>());
            var mainWindowBackToOAPPButtonVMInstance = new MainWindowBackToOAPPButtonViewModel(this.container.Resolve<IEventAggregator>());
            var mainWindowNavigationButtonsVMInstance = new MainWindowNavigationButtonsViewModel(this.container.Resolve<IEventAggregator>());
            var helpMainWindowInstance = new HelpMainWindow(container.Resolve<IEventAggregator>());
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

            this.container.RegisterInstance<IMainWindowViewModel>(mainWindowVMInstance);
            this.container.RegisterInstance<IMainWindow>(mainWindowInstance);
            this.container.RegisterInstance<IIdleViewModel>(idleVMInstance);
            this.container.RegisterInstance<IMainWindowBackToOAPPButtonViewModel>(mainWindowBackToOAPPButtonVMInstance);
            this.container.RegisterInstance<IMainWindowNavigationButtonsViewModel>(mainWindowNavigationButtonsVMInstance);
            this.container.RegisterInstance<IHelpMainWindow>(helpMainWindowInstance);
            this.container.RegisterInstance<IDrawerActivityPickingViewModel>(drawerActivityPickingVMInstance);
            this.container.RegisterInstance<IDrawerWaitViewModel>(drawerWaitVMInstance);
            this.container.RegisterInstance<IDrawerActivityPickingDetailViewModel>(drawerActivityPickingDetailVMInstance);
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
            this.container.RegisterInstance<IMaintenanceMainPageViewModel>(maintenanceMainPageVMInstance);
            this.container.RegisterInstance<IMaintenanceDetailViewModel>(maintenanceDetailVMInstance);
            this.container.RegisterInstance<IStatisticsNavigationViewModel>(statisticsNavigationVMInstance);
            this.container.RegisterInstance<IDrawerWeightSaturationViewModel>(drawerWeightSaturationVMInstance);

            navigationServiceInstance.Initialize(this.container);

            mainWindowVMInstance.InitializeViewModel(this.container);
            mainWindowBackToOAPPButtonVMInstance.InitializeViewModel(this.container);
            drawerActivityPickingVMInstance.InitializeViewModel(this.container);
            otherNavigationVMInstance.InitializeViewModel(this.container);
            generalInfoVMInstance.InitializeViewModel(this.container);
            statisticsGeneralDataVMInstance.InitializeViewModel(this.container);

            mainWindowBackToOAPPButtonVMInstance.InitializeButtons();
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            // HACK IModule interface requires the implementation of this method
        }

        #endregion
    }
}

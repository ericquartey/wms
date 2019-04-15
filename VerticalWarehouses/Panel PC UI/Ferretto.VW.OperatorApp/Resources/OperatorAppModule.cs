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

            var mainWindowVMInstance = new MainWindowViewModel(this.container.Resolve<IEventAggregator>());
            var mainWindowInstance = new MainWindow(this.container.Resolve<IEventAggregator>());
            var idleVMInstance = new IdleViewModel(container.Resolve<IEventAggregator>());
            var mainWindowBackToOAPPButtonVMInstance = new MainWindowBackToOAPPButtonViewModel(this.container.Resolve<IEventAggregator>());
            var mainWindowNavigationButtonsVMInstance = new MainWindowNavigationButtonsViewModel(this.container.Resolve<IEventAggregator>());
            var helpMainWindowInstance = new HelpMainWindow(container.Resolve<IEventAggregator>());
            var drawerActivityVMInstance = new DrawerActivityViewModel(container.Resolve<IEventAggregator>());
            var drawerWaitVMInstance = new DrawerWaitViewModel(container.Resolve<IEventAggregator>());
            var drawerActivityDetailVMInstance = new DrawerActivityDetailViewModel(container.Resolve<IEventAggregator>());
            var listsInWaitVMInstance = new ListsInWaitViewModel(container.Resolve<IEventAggregator>());
            var itemSearchVMInstance = new ItemSearchViewModel(container.Resolve<IEventAggregator>());
            var itemDetailVMInstance = new ItemDetailViewModel(container.Resolve<IEventAggregator>());
            var immediateDrawerCallVMInstance = new ImmediateDrawerCallViewModel(container.Resolve<IEventAggregator>());
            var generalInfoVMInstance = new GeneralInfoViewModel(container.Resolve<IEventAggregator>());
            var drawerCompactingVMInstance = new DrawerCompactingViewModel(container.Resolve<IEventAggregator>());
            var drawerOperationsMainVMInstance = new DrawerOperationsMainViewModel(container.Resolve<IEventAggregator>());
            var drawerOperationsFooterVMInstance = new DrawerOperationsFooterViewModel(container.Resolve<IEventAggregator>());
            var otherMainVMInstance = new OtherMainViewModel(container.Resolve<IEventAggregator>());
            var otherNavigationVMInstance = new OtherNavigationViewModel(container.Resolve<IEventAggregator>());
            var statisticsGeneralDataVMInstance = new StatisticsGeneralDataViewModel(container.Resolve<IEventAggregator>());
            var statisticMainVMInstance = new StatisticsMainViewModel(container.Resolve<IEventAggregator>());
            var itemStatisticsVMInstance = new ItemStatisticsViewModel(container.Resolve<IEventAggregator>());
            var drawerSpaceSaturationVMInstance = new DrawerSpaceSaturationViewModel(container.Resolve<IEventAggregator>());
            var detailListInWaitVMInstance = new DetailListInWaitViewModel(container.Resolve<IEventAggregator>());
            var cellsStatisticsVMInstance = new CellsStatisticsViewModel(container.Resolve<IEventAggregator>());
            var errorsStatisticsVMInstance = new ErrorsStatisticsViewModel(container.Resolve<IEventAggregator>());
            var maintenanceMainPageVMInstance = new MaintenanceMainPageViewModel(container.Resolve<IEventAggregator>());
            var maintenanceDetailVMInstance = new MaintenanceDetailViewModel(container.Resolve<IEventAggregator>());

            this.container.RegisterInstance<IMainWindowViewModel>(mainWindowVMInstance);
            this.container.RegisterInstance<IMainWindow>(mainWindowInstance);
            this.container.RegisterInstance<IIdleViewModel>(idleVMInstance);
            this.container.RegisterInstance<IMainWindowBackToOAPPButtonViewModel>(mainWindowBackToOAPPButtonVMInstance);
            this.container.RegisterInstance<IMainWindowNavigationButtonsViewModel>(mainWindowNavigationButtonsVMInstance);
            this.container.RegisterInstance<IHelpMainWindow>(helpMainWindowInstance);
            this.container.RegisterInstance<IDrawerActivityViewModel>(drawerActivityVMInstance);
            this.container.RegisterInstance<IDrawerWaitViewModel>(drawerWaitVMInstance);
            this.container.RegisterInstance<IDrawerActivityDetailViewModel>(drawerActivityDetailVMInstance);
            this.container.RegisterInstance<IListsInWaitViewModel>(listsInWaitVMInstance);
            this.container.RegisterInstance<IItemSearchViewModel>(itemSearchVMInstance);
            this.container.RegisterInstance<IItemDetailViewModel>(itemDetailVMInstance);
            this.container.RegisterInstance<IImmediateDrawerCallViewModel>(immediateDrawerCallVMInstance);
            this.container.RegisterInstance<IGeneralInfoViewModel>(generalInfoVMInstance);
            this.container.RegisterInstance<IDrawerCompactingViewModel>(drawerCompactingVMInstance);
            this.container.RegisterInstance<IDrawerOperationsMainViewModel>(drawerOperationsMainVMInstance);
            this.container.RegisterInstance<IDrawerOperationsFooterViewModel>(drawerOperationsFooterVMInstance);
            this.container.RegisterInstance<IOtherMainViewModel>(otherMainVMInstance);
            this.container.RegisterInstance<IOtherNavigationViewModel>(otherNavigationVMInstance);
            this.container.RegisterInstance<IStatisticsGeneralDataViewModel>(statisticsGeneralDataVMInstance);
            this.container.RegisterInstance<IStatisticsMainViewModel>(statisticMainVMInstance);
            this.container.RegisterInstance<IItemStatisticsViewModel>(itemStatisticsVMInstance);
            this.container.RegisterInstance<IDrawerSpaceSaturationViewModel>(drawerSpaceSaturationVMInstance);
            this.container.RegisterInstance<IDetailListInWaitViewModel>(detailListInWaitVMInstance);
            this.container.RegisterInstance<ICellsStatisticsViewModel>(cellsStatisticsVMInstance);
            this.container.RegisterInstance<IErrorsStatisticsViewModel>(errorsStatisticsVMInstance);
            this.container.RegisterInstance<IMaintenanceMainPageViewModel>(maintenanceMainPageVMInstance);
            this.container.RegisterInstance<IMaintenanceDetailViewModel>(maintenanceDetailVMInstance);

            mainWindowVMInstance.InitializeViewModel(this.container);
            mainWindowBackToOAPPButtonVMInstance.InitializeViewModel(this.container);
            drawerOperationsMainVMInstance.InitializeViewModel(this.container);
            drawerActivityVMInstance.InitializeViewModel(this.container);
            drawerOperationsFooterVMInstance.InitializeViewModel(this.container);
            otherMainVMInstance.InitializeViewModel(this.container);
            otherNavigationVMInstance.InitializeViewModel(this.container);
            statisticMainVMInstance.InitializeViewModel(this.container);

            mainWindowBackToOAPPButtonVMInstance.InitializeButtons();
            drawerOperationsFooterVMInstance.InitializeButtons();
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

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
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.Utils.Interfaces;
using Prism.Mvvm;

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
            var helpMainWindowInstance = new HelpMainWindow(container.Resolve<IEventAggregator>());

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
            this.container.RegisterInstance<IMainWindow>(mainWindowInstance);
            this.container.RegisterInstance<IHelpMainWindow>(helpMainWindowInstance);

            this.RegisterInstanceAndBindViewToViewModel<IIdleViewModel, IdleViewModel>(idleVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IMainWindowBackToOAPPButtonViewModel, MainWindowBackToOAPPButtonViewModel>(mainWindowBackToOAPPButtonVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IMainWindowNavigationButtonsViewModel, MainWindowNavigationButtonsViewModel>(mainWindowNavigationButtonsVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IDrawerActivityPickingViewModel, DrawerActivityPickingViewModel>(drawerActivityPickingVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IDrawerActivityPickingDetailViewModel, DrawerActivityPickingDetailViewModel>(drawerActivityPickingDetailVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IDrawerWaitViewModel, DrawerWaitViewModel>(drawerWaitVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IListsInWaitViewModel, ListsInWaitViewModel>(listsInWaitVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IItemSearchViewModel, ItemSearchViewModel>(itemSearchVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IItemDetailViewModel, ItemDetailViewModel>(itemDetailVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IImmediateDrawerCallViewModel, ImmediateDrawerCallViewModel>(immediateDrawerCallVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IGeneralInfoViewModel, GeneralInfoViewModel>(generalInfoVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IDrawerCompactingViewModel, DrawerCompactingViewModel>(drawerCompactingVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IOtherNavigationViewModel, OtherNavigationViewModel>(otherNavigationVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IStatisticsGeneralDataViewModel, StatisticsGeneralDataViewModel>(statisticsGeneralDataVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IItemStatisticsViewModel, ItemStatisticsViewModel>(itemStatisticsVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IDrawerSpaceSaturationViewModel, DrawerSpaceSaturationViewModel>(drawerSpaceSaturationVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IDetailListInWaitViewModel, DetailListInWaitViewModel>(detailListInWaitVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ICellsStatisticsViewModel, CellsStatisticsViewModel>(cellsStatisticsVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IErrorsStatisticsViewModel, ErrorsStatisticsViewModel>(errorsStatisticsVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IMaintenanceDetailViewModel, MaintenanceDetailViewModel>(maintenanceDetailVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IMaintenanceMainPageViewModel, MaintenanceMainPageViewModel>(maintenanceMainPageVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IStatisticsNavigationViewModel, StatisticsNavigationViewModel>(statisticsNavigationVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IDrawerActivityRefillingViewModel, DrawerActivityRefillingViewModel>(drawerActivityRefillingVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IDrawerActivityRefillingDetailViewModel, DrawerActivityRefillingDetailViewModel>(drawerActivityRefillingDetailVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IDrawerWeightSaturationViewModel, DrawerWeightSaturationViewModel>(drawerWeightSaturationVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IDrawerActivityInventoryViewModel, DrawerActivityInventoryViewModel>(drawerActivityInventoryVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IDrawerActivityInventoryDetailViewModel, DrawerActivityInventoryDetailViewModel>(drawerActivityInventoryDetailVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IItemSearchViewModel, ItemSearchViewModel>(itemSearchVMInstance);

            this.RegisterTypeAndBindViewToViewModel<ICustomControlArticleDataGridViewModel, CustomControlArticleDataGridViewModel>();

            navigationServiceInstance.Initialize(this.container);

            mainWindowVMInstance.InitializeViewModel(this.container);
            mainWindowBackToOAPPButtonVMInstance.InitializeViewModel(this.container);
            drawerActivityPickingVMInstance.InitializeViewModel(this.container);
            otherNavigationVMInstance.InitializeViewModel(this.container);
            generalInfoVMInstance.InitializeViewModel(this.container);
            statisticsGeneralDataVMInstance.InitializeViewModel(this.container);
            itemSearchVMInstance.InitializeViewModel(this.container);

            mainWindowBackToOAPPButtonVMInstance.InitializeButtons();
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            // HACK IModule interface requires the implementation of this method
        }

        private void RegisterInstanceAndBindViewToViewModel<I, T>(T instance)
            where T : BindableBase, I
            where I : IViewModel
        {
            this.container.RegisterInstance<I>(instance);
            var view = typeof(T).ToString().Substring(0, typeof(T).ToString().Length - 9);
            ViewModelLocationProvider.Register(view, () => this.container.Resolve<T>());
        }

        private void RegisterTypeAndBindViewToViewModel<I, T>()
            where T : BindableBase, I
            where I : IViewModel
        {
            this.container.RegisterType<I, T>();
            var view = typeof(T).ToString().Substring(0, typeof(T).ToString().Length - 9);
            ViewModelLocationProvider.Register(view, () => this.container.Resolve<T>());
        }

        #endregion
    }
}

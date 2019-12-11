using System.Configuration;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.App.Operator.Views;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Modules.Operator
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Major Code Smell",
    "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
    Justification = "This is a container initialization class, so it is ok to be coupled to many types.")]
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
            containerProvider.UseMachineAutomationHubs();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<OperatorMenuView>();
            containerRegistry.RegisterForNavigation<EmptyView>();
            containerRegistry.RegisterForNavigation<LoadingUnitView>();

            containerRegistry.RegisterForNavigation<ItemOperationWaitView>();
            containerRegistry.RegisterForNavigation<ItemInventoryDetailsView>();
            containerRegistry.RegisterForNavigation<ItemInventoryView>();
            containerRegistry.RegisterForNavigation<ItemPickDetailsView>();
            containerRegistry.RegisterForNavigation<ItemPickView>();
            containerRegistry.RegisterForNavigation<ItemPutDetailsView>();
            containerRegistry.RegisterForNavigation<ItemPutView>();

            containerRegistry.RegisterForNavigation<ItemSearchMainView>();
            containerRegistry.RegisterForNavigation<ItemSearchDetailView>();

            containerRegistry.RegisterForNavigation<WaitingListsView>();
            containerRegistry.RegisterForNavigation<WaitingListDetailView>();

            containerRegistry.RegisterForNavigation<OthersNavigationView>();
            containerRegistry.RegisterForNavigation<ImmediateLoadingUnitCallView>();
            containerRegistry.RegisterForNavigation<DrawerCompactingView>();
            containerRegistry.RegisterForNavigation<DrawerCompactingDetailView>();
            containerRegistry.RegisterForNavigation<StatisticsNavigationView>();
            containerRegistry.RegisterForNavigation<StatisticsCellsView>();
            containerRegistry.RegisterForNavigation<StatisticsDrawersView>();
            containerRegistry.RegisterForNavigation<StatisticsErrorsView>();
            containerRegistry.RegisterForNavigation<StatisticsMachineView>();
            containerRegistry.RegisterForNavigation<StatisticsSpaceSaturationView>();
            containerRegistry.RegisterForNavigation<StatisticsWeightSaturationView>();
            containerRegistry.RegisterForNavigation<MaintenanceView>();
            containerRegistry.RegisterForNavigation<MaintenanceDetailView>();

            containerRegistry.RegisterForNavigation<AlarmView>();
            containerRegistry.RegisterForNavigation<CountersView>();
            containerRegistry.RegisterForNavigation<GeneralView>();
            containerRegistry.RegisterForNavigation<StatisticsView>();
            containerRegistry.RegisterForNavigation<DiagnosticsView>();

            containerRegistry.Register<ICustomControlMaintenanceDataGridViewModel, CustomControlMaintenanceDataGridViewModel>();
            containerRegistry.Register<ICustomControlMaintenanceDetailDataGridViewModel, CustomControlMaintenanceDetailDataGridViewModel>();
            containerRegistry.Register<ICustomControlCellStatisticsDataGridViewModel, CustomControlCellStatisticsDataGridViewModel>();
            containerRegistry.Register<ICustomControlErrorsDataGridViewModel, CustomControlErrorsDataGridViewModel>();
            containerRegistry.Register<ICustomControlDrawerWeightSaturationDataGridViewModel, CustomControlDrawerWeightSaturationDataGridViewModel>();
            containerRegistry.Register<ICustomControlDrawerSaturationDataGridViewModel, CustomControlDrawerSaturationDataGridViewModel>();
        }

        #endregion
    }
}

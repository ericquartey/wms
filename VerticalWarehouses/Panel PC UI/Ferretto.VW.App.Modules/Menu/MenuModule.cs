using Ferretto.VW.App.Menu.Views;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Unity;

namespace Ferretto.VW.App.Modules.Menu
{
    [Module(ModuleName = nameof(Utils.Modules.Menu), OnDemand = true)]
    [ModuleDependency(nameof(Utils.Modules.Errors))]
    [ModuleDependency(nameof(Utils.Modules.Installation))]
    [ModuleDependency(nameof(Utils.Modules.Operator))]
    public class MenuModule : IModule
    {
        #region Fields

        private readonly IUnityContainer container;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        public MenuModule(IUnityContainer container)
        {
            this.container = container;
        }

        #endregion

        #region Methods

        public static void BindViewModelToView<TViewModel, TView>(IContainerProvider containerProvider)
        {
            ViewModelLocationProvider.Register(typeof(TView).ToString(), () => containerProvider.Resolve<TViewModel>());
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.UseMachineAutomationHubs();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<AboutMenuView>();
            containerRegistry.RegisterForNavigation<InstallationMenuView>();
            containerRegistry.RegisterForNavigation<MainMenuView>();
            containerRegistry.RegisterForNavigation<MaintenanceMenuView>();
            containerRegistry.RegisterForNavigation<OperationsMenuView>();

            containerRegistry.RegisterForNavigation<AccessoriesMenuView>();
            containerRegistry.RegisterForNavigation<BaysMenuView>();
            containerRegistry.RegisterForNavigation<CellsMenuView>();
            containerRegistry.RegisterForNavigation<ElevatorMenuView>();
            containerRegistry.RegisterForNavigation<LoadingUnitsMenuView>();
        }

        #endregion
    }
}

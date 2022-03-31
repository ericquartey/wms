using Ferretto.VW.App.Menu.Views;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
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
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<InstallationMenuView>();
            containerRegistry.RegisterForNavigation<OtherMenuView>();
            containerRegistry.RegisterForNavigation<MainMenuView>();
            containerRegistry.RegisterForNavigation<MaintenanceMenuView>();

            containerRegistry.RegisterForNavigation<AccessoriesMenuView>();
            containerRegistry.RegisterForNavigation<BaysMenuView>();
            containerRegistry.RegisterForNavigation<CellsMenuView>();
            containerRegistry.RegisterForNavigation<ElevatorMenuView>();
            containerRegistry.RegisterForNavigation<LoadingUnitsMenuView>();
        }

        #endregion
    }
}

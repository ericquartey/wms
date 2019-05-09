using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Unity;

namespace Ferretto.WMS.Modules.ItemLists
{
    [Module(ModuleName = nameof(Common.Utils.Modules.ItemLists), OnDemand = true)]
    [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
    public class ItemListsModule : IModule
    {
        #region Constructors

        public ItemListsModule(IUnityContainer container, IRegionManager regionManager, INavigationService navigationService)
        {
            this.Container = container;
            this.RegionManager = regionManager;
            this.NavigationService = navigationService;
        }

        #endregion

        #region Properties

        public IUnityContainer Container { get; private set; }

        public INavigationService NavigationService { get; private set; }

        public IRegionManager RegionManager { get; private set; }

        #endregion

        #region Methods

        public void OnInitialized(IContainerProvider containerProvider)
        {
            NLog.LogManager
                .GetCurrentClassLogger()
                .Trace("Module loaded.");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            SplashScreenService.SetMessage(Common.Resources.DesktopApp.InitializingSchedulerModule);

            NLog.LogManager
               .GetCurrentClassLogger()
               .Trace("Loading module ...");

            this.NavigationService.Register<ItemListsView, ItemListsViewModel>();
            this.NavigationService.Register<ItemListDetailsView, ItemListDetailsViewModel>();
            this.NavigationService.Register<ItemListAddDialogView, ItemListAddDialogViewModel>();
            this.NavigationService.Register<ItemListExecuteDialogView, ItemListExecuteDialogViewModel>();

            this.NavigationService.Register<ItemListRowAddDialogView, ItemListRowAddDialogViewModel>();
            this.NavigationService.Register<ItemListRowDetailsView, ItemListRowDetailsViewModel>();
            this.NavigationService.Register<ItemListRowExecuteDialogView, ItemListRowExecuteDialogViewModel>();
        }

        #endregion
    }
}

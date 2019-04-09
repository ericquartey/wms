using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using NLog;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Unity;
#if DEBUG
#else
using Ferretto.Common.BusinessProviders;
#endif

namespace Ferretto.WMS.Modules.MasterData
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "This class associate all Views to related ViewModels")]
    [Module(ModuleName = nameof(Common.Utils.Modules.MasterData), OnDemand = true)]
    [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
    public class MasterDataModule : IModule
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region Constructors

        public MasterDataModule(IUnityContainer container, IRegionManager regionManager, INavigationService navigationService)
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
            this.logger.Trace("Module loaded.");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            SplashScreenService.SetMessage(Common.Resources.DesktopApp.InitializingMasterDataModule);

            this.logger.Trace("Loading module ...");

            this.NavigationService.Register<ItemsView, ItemsViewModel>();
            this.NavigationService.Register<ItemDetailsView, ItemDetailsViewModel>();
            this.NavigationService.Register<ItemAddDialogView, ItemAddDialogViewModel>();
            this.NavigationService.Register<WithdrawDialogView, WithdrawDialogViewModel>();

            this.NavigationService.Register<CellsView, CellsViewModel>();
            this.NavigationService.Register<CellDetailsView, CellDetailsViewModel>();

            this.NavigationService.Register<CompartmentsView, CompartmentsViewModel>();
            this.NavigationService.Register<CompartmentDetailsView, CompartmentDetailsViewModel>();

            this.NavigationService.Register<LoadingUnitsView, LoadingUnitsViewModel>();
            this.NavigationService.Register<LoadingUnitDetailsView, LoadingUnitDetailsViewModel>();
            this.NavigationService.Register<LoadingUnitEditView, LoadingUnitEditViewModel>();
            this.NavigationService.Register<LoadingUnitAddDialogView, LoadingUnitAddDialogViewModel>();

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

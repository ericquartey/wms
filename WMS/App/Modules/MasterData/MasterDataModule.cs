using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using NLog;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Unity;

namespace Ferretto.WMS.Modules.MasterData
{
    [Module(ModuleName = nameof(Common.Utils.Modules.MasterData), OnDemand = true)]
    [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
    public class MasterDataModule : IModule
    {
        #region Fields

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

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

            this.NavigationService.Register<ItemAddView, ItemAddViewModel>();
            this.NavigationService.Register<ItemPickView, ItemPickViewModel>();
            this.NavigationService.Register<ItemPutView, ItemPutViewModel>();

            this.NavigationService.Register<CellsView, CellsViewModel>();
            this.NavigationService.Register<CellDetailsView, CellDetailsViewModel>();

            this.NavigationService.Register<AssociateCompartmentTypesView, AssociateCompartmentTypesViewModel>();
            this.NavigationService.Register<AssociateCompartmentTypesStepsView, AssociateCompartmentTypesStepsViewModel>();
            this.NavigationService.Register<ItemCompartmentTypesToItemStepView, ItemCompartmentTypesToItemStepViewModel>();
            this.NavigationService.Register<ChooseLoadingUnitStepView, ChooseLoadingUnitStepViewModel>();
            this.NavigationService.Register<CompartmentsView, CompartmentsViewModel>();
            this.NavigationService.Register<CompartmentTypesView, CompartmentTypesViewModel>();
            this.NavigationService.Register<CompartmentDetailsView, CompartmentDetailsViewModel>();
            this.NavigationService.Register<CompartmentTypeDetailsView, CompartmentTypeDetailsViewModel>();

            this.NavigationService.Register<LoadingUnitsView, LoadingUnitsViewModel>();
            this.NavigationService.Register<LoadingUnitDetailsView, LoadingUnitDetailsViewModel>();
            this.NavigationService.Register<LoadingUnitEditView, LoadingUnitEditViewModel>();
            this.NavigationService.Register<LoadingUnitAddView, LoadingUnitAddViewModel>();
            this.NavigationService.Register<LoadingUnitWithdrawView, LoadingUnitWithdrawViewModel>();
        }

        #endregion
    }
}

using System.Linq;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.WMS.Modules.MasterData
{
    [Module(ModuleName = nameof(Common.Utils.Modules.MasterData), OnDemand = true)]
    [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
    public class MasterDataModule : IModule
    {
        #region Fields

        private readonly INavigationService navigationService;

        #endregion Fields

        #region Constructors

        public MasterDataModule(IUnityContainer container, IRegionManager regionManager, INavigationService navigationService)
        {
            this.Container = container;
            this.RegionManager = regionManager;
            this.navigationService = navigationService;
        }

        #endregion Constructors

        #region Properties

        public IUnityContainer Container { get; private set; }
        public IRegionManager RegionManager { get; private set; }

        #endregion Properties

        #region Methods

        public void Initialize()
        {
            SplashScreenService.SetMessage("Initializing Master Data module ...");

            this.navigationService.Register<ItemsView, ItemsViewModel>();
            this.navigationService.Register<ItemDetailsView, ItemDetailsViewModel>();
            this.navigationService.Register<ItemsAndDetailsView, ItemsAndDetailsViewModel>();

            this.navigationService.Register<CompartmentsView, CompartmentsViewModel>();
            this.navigationService.Register<CompartmentDetailsView, CompartmentDetailsViewModel>();
            this.navigationService.Register<CompartmentsAndDetailsView, CompartmentsAndDetailsViewModel>();

            this.navigationService.Register<LoadingUnitsView, LoadingUnitsViewModel>();
            this.navigationService.Register<LoadingUnitDetailsView, LoadingUnitDetailsViewModel>();
            this.navigationService.Register<LoadingUnitsAndDetailsView, LoadingUnitsAndDetailsViewModel>();

            SplashScreenService.SetMessage("Initializing Entity Framework ...");

            ServiceLocator.Current.GetInstance<IItemProvider>().GetAll().ToList();

            SplashScreenService.SetMessage("Initializing Entity Framework ... done.");
        }

        #endregion Methods
    }
}

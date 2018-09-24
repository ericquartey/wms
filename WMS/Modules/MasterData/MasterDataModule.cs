using Ferretto.Common.Controls.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.WMS.Modules.MasterData
{
    [Module(ModuleName = nameof(Common.Utils.Modules.MasterData), OnDemand = true)]
    [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
    public class MasterDataModule : IModule
    {
        #region IModule Members

        public IUnityContainer Container { get; private set; }
        public IRegionManager RegionManager { get; private set; }
        private readonly INavigationService navigationService;

        public MasterDataModule(IUnityContainer container, IRegionManager regionManager,
            INavigationService navigationService)
        {
            this.Container = container;
            this.RegionManager = regionManager;
            this.navigationService = navigationService;
        }

        public void Initialize()
        {
            this.navigationService.Register<ItemsView, ItemsViewModel>();
            this.navigationService.Register<ItemDetailsView, ItemDetailsViewModel>();
            this.navigationService.Register<ItemsAndDetailsView, ItemsAndDetailsViewModel>();

            this.navigationService.Register<CompartmentsView, CompartmentsViewModel>();
            this.navigationService.Register<CompartmentDetailsView, CompartmentDetailsViewModel>();
            this.navigationService.Register<CompartmentsAndDetailsView, CompartmentsAndDetailsViewModel>();
        }

        #endregion
    }
}

using Ferretto.Common.Controls.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.WMS.Modules.Compartment
{
    [Module(ModuleName = nameof(Common.Utils.Modules.Compartment), OnDemand = true)]
    [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
    public class CompartmentModule : IModule
    {
        #region Fields

        private readonly INavigationService navigationService;

        #endregion Fields

        #region Constructors

        public CompartmentModule(IUnityContainer container, IRegionManager regionManager,
            INavigationService navigationService)
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
            this.navigationService.Register<CompartmentView, CompartmentViewModel>();
        }

        #endregion Methods
    }
}

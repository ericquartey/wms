using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.WMS.Modules.Machines
{
    [Module(ModuleName = nameof(Common.Utils.Modules.Machines), OnDemand = true)]
    [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
    public class MachinesModule : IModule
    {
        #region Constructors

        public MachinesModule(IUnityContainer container, IRegionManager regionManager, INavigationService navigationService)
        {
            this.Container = container;
            this.RegionManager = regionManager;
            this.NavigationService = navigationService;
        }

        #endregion Constructors

        #region Properties

        public IUnityContainer Container { get; private set; }
        public INavigationService NavigationService { get; private set; }
        public IRegionManager RegionManager { get; private set; }

        #endregion Properties

        #region Methods

        public void Initialize()
        {
            SplashScreenService.SetMessage(Common.Resources.DesktopApp.InitializingMachinesModule);

            this.NavigationService.Register<MachinesView, MachinesViewModel>();
        }

        #endregion Methods
    }
}

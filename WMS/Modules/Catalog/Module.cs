using Ferretto.Common.BLL;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.WMS.Modules.Catalog
{
    public class Module : IModule
    {
        #region IModule Members

        public IUnityContainer Container { get; private set; }
        public IRegionManager RegionManager { get; private set; }

        public Module(IUnityContainer container, IRegionManager regionManager)
        {
            Container = container;
            RegionManager = regionManager;
        }

        public void Initialize()
        {
            var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            regionManager.RegisterViewWithRegion($"{nameof(Modules.Catalog)}.{nameof(Common.BLL.Modules.Catalog.ItemsAndDetails)}", typeof(ItemsAndDetailsView));

        }

        #endregion
    }
}

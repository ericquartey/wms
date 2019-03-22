using System.Configuration;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Providers;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Ioc;
using Prism.Modularity;
using Unity;

namespace Ferretto.WMS.App.Modules.BLL
{
    [Module(ModuleName = nameof(Common.Utils.Modules.BusinessLogic))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "This class register services into container")]
    public class BusinessLogicModule : IModule
    {
        #region Constructors

        public BusinessLogicModule(IUnityContainer container)
        {
            this.Container = container;
        }

        #endregion

        #region Properties

        public IUnityContainer Container { get; private set; }

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
            NLog.LogManager
                .GetCurrentClassLogger()
                .Trace("Loading module ...");

            RegisterBusinessProviders(containerRegistry);
            RegisterDataServiceEndpoints(containerRegistry);
        }

        private static void RegisterBusinessProviders(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IAbcClassProvider, AbcClassProvider>();
            containerRegistry.Register<IAisleProvider, AisleProvider>();
            containerRegistry.Register<IAreaProvider, AreaProvider>();
            containerRegistry.Register<IBayProvider, BayProvider>();
            containerRegistry.Register<ICellPositionProvider, CellPositionProvider>();
            containerRegistry.Register<ICellProvider, CellProvider>();
            containerRegistry.Register<ICellStatusProvider, CellStatusProvider>();
            containerRegistry.Register<ICellTypeProvider, CellTypeProvider>();
            containerRegistry.Register<ICompartmentProvider, CompartmentProvider>();
            containerRegistry.Register<ICompartmentStatusProvider, CompartmentStatusProvider>();
            containerRegistry.Register<ICompartmentTypeProvider, CompartmentTypeProvider>();
            containerRegistry.Register<IDataSourceService, DataSourceService>();
            containerRegistry.Register<IItemCategoryProvider, ItemCategoryProvider>();
            containerRegistry.Register<IItemListProvider, ItemListProvider>();
            containerRegistry.Register<IItemListRowProvider, ItemListRowProvider>();
            containerRegistry.Register<IItemProvider, ItemProvider>();
            containerRegistry.Register<ILoadingUnitProvider, LoadingUnitProvider>();
            containerRegistry.Register<ILoadingUnitStatusProvider, LoadingUnitStatusProvider>();
            containerRegistry.Register<ILoadingUnitTypeProvider, LoadingUnitTypeProvider>();
            containerRegistry.Register<IMachineProvider, MachineProvider>();
            containerRegistry.Register<IMaterialStatusProvider, MaterialStatusProvider>();
            containerRegistry.Register<IMeasureUnitProvider, MeasureUnitProvider>();
            containerRegistry.Register<IMissionProvider, MissionProvider>();
            containerRegistry.Register<IPackageTypeProvider, PackageTypeProvider>();
            containerRegistry.Register<ISchedulerRequestProvider, SchedulerRequestProvider>();
            containerRegistry.Register<IUserProvider, UserProvider>();
            containerRegistry.Register<IImageFileProvider, ImageFileProvider>();
        }

        private static void RegisterDataServiceEndpoints(IContainerRegistry containerRegistry)
        {
            var serviceEndPoint = new System.Uri(ConfigurationManager.AppSettings["DataServiceEndpoint"]);

            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IAbcClassesDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IAislesDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IAreasDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IBaysDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ICellPositionsDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ICellStatusesDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ICellTypesDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ICellsDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ICompartmentStatusesDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ICompartmentTypesDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ICompartmentsDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IItemCategoriesDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IItemCompartmentTypesDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IItemListsDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IItemListRowsDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IItemsDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ILoadingUnitsDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ILoadingUnitStatusesDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ILoadingUnitTypesDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IMachinesDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IMaterialStatusesDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IMeasureUnitsDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IMissionsDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IPackageTypesDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ISchedulerRequestsDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IUsersDataService>(serviceEndPoint));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IImageFileDataService>(serviceEndPoint));
        }

        #endregion
    }
}

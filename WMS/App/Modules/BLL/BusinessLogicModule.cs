using System.Configuration;
using System.Net.Http;
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
            RegisterDataServiceEndpoints(containerRegistry, this.Container);
        }

        private static AutoMapper.IMapper GetMapper(IUnityContainer container)
        {
            var mapperProvider = container.Resolve<MapperProvider>();
            return mapperProvider.GetMapper();
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
            containerRegistry.Register<IFileProvider, ImageProvider>();
            containerRegistry.Register<IGlobalSettingsProvider, GlobalSettingsProvider>();
        }

        private static void RegisterDataServiceEndpoints(IContainerRegistry containerRegistry, IUnityContainer container)
        {
            var serviceEndPoint = new System.Uri(ConfigurationManager.AppSettings["DataService:Url"]);

            var httpClient = new HttpClient();
            containerRegistry.RegisterInstance(httpClient);

            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IAislesDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IAreasDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IBaysDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ICellPositionsDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ICellStatusesDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ICellTypesDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ICellsDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ICompartmentStatusesDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ICompartmentTypesDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ICompartmentsDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IImagesDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IItemCategoriesDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IItemListRowsDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IItemListsDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IItemsDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ILoadingUnitStatusesDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ILoadingUnitTypesDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ILoadingUnitsDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IMachinesDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IMaterialStatusesDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IMeasureUnitsDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IMissionsDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IPackageTypesDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ISchedulerRequestsDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IUsersDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IAbcClassesDataService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<ILocalizationService>(serviceEndPoint, httpClient));
            containerRegistry.RegisterInstance(DataServiceFactory.GetService<IGlobalSettingsDataService>(serviceEndPoint, httpClient));

            containerRegistry.RegisterInstance(new MapperProvider(container));
            containerRegistry.RegisterInstance(GetMapper(container));
        }

        #endregion
    }
}

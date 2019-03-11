using System.Configuration;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Modules.BLL.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Practices.Unity;
using Prism.Modularity;

namespace Ferretto.Common.Modules.BLL
{
    [Module(ModuleName = nameof(Utils.Modules.BusinessLogic))]
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

        public void Initialize()
        {
            NLog.LogManager
               .GetCurrentClassLogger()
               .Trace("Loading module ...");

            this.RegisterBusinessProviders();

            this.RegisterDataServiceEndpoints();

            NLog.LogManager
               .GetCurrentClassLogger()
               .Trace("Module loaded.");
        }

        private void RegisterBusinessProviders()
        {
            this.Container.RegisterType<IAbcClassProvider, AbcClassProvider>();
            this.Container.RegisterType<IAisleProvider, AisleProvider>();
            this.Container.RegisterType<IAreaProvider, AreaProvider>();
            this.Container.RegisterType<IBayProvider, BayProvider>();
            this.Container.RegisterType<ICellPositionProvider, CellPositionProvider>();
            this.Container.RegisterType<ICellProvider, CellProvider>();
            this.Container.RegisterType<ICellStatusProvider, CellStatusProvider>();
            this.Container.RegisterType<ICellTypeProvider, CellTypeProvider>();
            this.Container.RegisterType<ICompartmentProvider, CompartmentProvider>();
            this.Container.RegisterType<ICompartmentStatusProvider, CompartmentStatusProvider>();
            this.Container.RegisterType<ICompartmentTypeProvider, CompartmentTypeProvider>();
            this.Container.RegisterType<IDataSourceService, DataSourceService>();
            this.Container.RegisterType<IItemCategoryProvider, ItemCategoryProvider>();
            this.Container.RegisterType<IItemListProvider, ItemListProvider>();
            this.Container.RegisterType<IItemListRowProvider, ItemListRowProvider>();
            this.Container.RegisterType<IItemProvider, ItemProvider>();
            this.Container.RegisterType<ILoadingUnitProvider, LoadingUnitProvider>();
            this.Container.RegisterType<ILoadingUnitStatusProvider, LoadingUnitStatusProvider>();
            this.Container.RegisterType<ILoadingUnitTypeProvider, LoadingUnitTypeProvider>();
            this.Container.RegisterType<IMachineProvider, MachineProvider>();
            this.Container.RegisterType<IMaterialStatusProvider, MaterialStatusProvider>();
            this.Container.RegisterType<IMeasureUnitProvider, MeasureUnitProvider>();
            this.Container.RegisterType<IMissionProvider, MissionProvider>();
            this.Container.RegisterType<IPackageTypeProvider, PackageTypeProvider>();
            this.Container.RegisterType<ISchedulerRequestProvider, SchedulerRequestProvider>();
            this.Container.RegisterType<IUserProvider, UserProvider>();
        }

        private void RegisterDataServiceEndpoints()
        {
            var serviceEndPoint = new System.Uri(ConfigurationManager.AppSettings["DataServiceEndpoint"]);

            this.Container.RegisterInstance(DataServiceFactory.GetService<IAbcClassesDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<IAislesDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<IAreasDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<IBaysDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<ICellPositionsDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<ICellStatusesDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<ICellTypesDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<ICellsDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<ICompartmentStatusesDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<ICompartmentTypesDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<ICompartmentsDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<IItemCategoriesDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<IItemCompartmentTypesDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<IItemListsDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<IItemListRowsDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<IItemsDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<ILoadingUnitsDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<ILoadingUnitStatusesDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<ILoadingUnitTypesDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<IMachinesDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<IMaterialStatusesDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<IMeasureUnitsDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<IMissionsDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<IPackageTypesDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<ISchedulerRequestsDataService>(serviceEndPoint));
            this.Container.RegisterInstance(DataServiceFactory.GetService<IUsersDataService>(serviceEndPoint));
        }

        #endregion
    }
}

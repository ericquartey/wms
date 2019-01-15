using System.Configuration;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.EF;
using Ferretto.Common.Modules.BLL.Services;
using Ferretto.WMS.Scheduler.WebAPI.Contracts;
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

        #endregion Constructors

        #region Properties

        public IUnityContainer Container { get; private set; }

        #endregion Properties

        #region Methods

        public void Initialize()
        {
            NLog.LogManager
               .GetCurrentClassLogger()
               .Trace("Loading module ...");

            this.Container.RegisterType<IAreaProvider, AreaProvider>();
            this.Container.RegisterType<IBayProvider, BayProvider>();
            this.Container.RegisterType<ICellProvider, CellProvider>();
            this.Container.RegisterType<ICompartmentProvider, CompartmentProvider>();
            this.Container.RegisterType<IDataSourceService, DataSourceService>();
            this.Container.RegisterType<IEventService, EventService>();
            this.Container.RegisterType<IImageProvider, ImageProvider>();
            this.Container.RegisterType<IItemProvider, ItemProvider>();
            this.Container.RegisterType<IItemListProvider, ItemListProvider>();
            this.Container.RegisterType<IItemListRowProvider, ItemListRowProvider>();
            this.Container.RegisterType<ILoadingUnitProvider, LoadingUnitProvider>();
            this.Container.RegisterType<IMachineProvider, MachineProvider>();
            this.Container.RegisterType<IUserProvider, UserProvider>();
            this.Container.RegisterType<IMissionProvider, MissionProvider>();
            this.Container.RegisterType<ISchedulerRequestProvider, SchedulerRequestProvider>();

            this.Container.RegisterType<IDatabaseContextService, DatabaseContextService>();

            var schedulerServiceEndPoint = ConfigurationManager.AppSettings["SchedulerServiceEndpoint"];
            var dataServiceEndPoint = ConfigurationManager.AppSettings["DataServiceEndpoint"];
            this.Container.RegisterType<IItemsService, ItemsService>(new InjectionConstructor(schedulerServiceEndPoint));
            this.Container.RegisterType<IMissionsService, MissionsService>(new InjectionConstructor(dataServiceEndPoint));
            this.Container.RegisterType<IItemListsService, ItemListsService>(new InjectionConstructor(schedulerServiceEndPoint));
            this.Container.RegisterType<IItemListRowsService, ItemListRowsService>(new InjectionConstructor(schedulerServiceEndPoint));

            this.Container.RegisterType<DatabaseContext, DatabaseContext>(new InjectionConstructor());
            this.Container.RegisterType<EnumerationProvider, EnumerationProvider>(new InjectionConstructor(new DatabaseContext()));

            NLog.LogManager
               .GetCurrentClassLogger()
               .Trace("Module loaded.");
        }

        #endregion Methods
    }
}

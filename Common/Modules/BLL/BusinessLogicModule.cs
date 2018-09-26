using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.DataAccess;
using Ferretto.Common.Modules.BLL.Services;
using Microsoft.Practices.Unity;
using Prism.Modularity;

namespace Ferretto.Common.Modules.BLL
{
    [Module(ModuleName = nameof(Utils.Modules.BusinessLogic))]
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
            this.Container.RegisterType<IDataService, DataService>();
            this.Container.RegisterType<IEventService, EventService>();
            this.Container.RegisterType<IDataSourceService, DataSourceService>();
            this.Container.RegisterType<IImageService, ImageService>();

            // TODO: in the future we may need to so something more complex http://docs.automapper.org/en/stable/Dependency-injection.html
            Mapper.Initialize(config => config.AddProfile<BusinessLogicAutoMapperProfile>());
            Mapper.Configuration.CompileMappings();
        }

        #endregion Methods
    }
}

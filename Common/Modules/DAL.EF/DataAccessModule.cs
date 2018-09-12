using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Modularity;

namespace Ferretto.Common.Modules.DAL.EF
{
    [Module(ModuleName = nameof(Utils.Modules.DataAccess))]
    public class DataAccessModule : IModule
    {
        #region Constructors

        public DataAccessModule(IUnityContainer container)
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
            var dbContext = ServiceLocator.Current.GetInstance<DatabaseContext>();
            dbContext.Database.EnsureCreated();

            // TODO: review this call to ensure we do a proper initialization of the entity framework
            dbContext.Set<Common.DAL.Models.Item>().Find(5);
        }

        #endregion Methods
    }
}

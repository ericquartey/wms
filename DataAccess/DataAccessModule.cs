using Microsoft.Practices.Unity;
using Prism.Modularity;

namespace Ferretto.Common.DataAccess
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
            this.Container.RegisterType<IDataService, DataService>();
        }

        #endregion Methods
    }
}

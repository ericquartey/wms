using Microsoft.Practices.Unity;
using Prism.Modularity;
using Ferretto.Common.DAL.Interfaces;

namespace Ferretto.Common.Modules.DAL.EF
{
  [Module(ModuleName = nameof(Utils.Modules.DataAccess))]
  public class DataAccessModule : IModule
  {
    public IUnityContainer Container { get; private set; }

    public DataAccessModule(IUnityContainer container)
    {
      this.Container = container;
    }

    public void Initialize()
    {
      this.Container.RegisterType<IUnitOfWork, UnitOfWork>();
      this.Container.RegisterType<IItemsRepository, ItemsRepository>();
      this.Container.RegisterType<IImageRepository, ImageFileRepository>();
    }
  }
}

using Microsoft.Practices.Unity;
using Prism.Modularity;

namespace Ferretto.Common.BLL
{
  [Module(ModuleName = nameof(Configuration.Modules.BusinessLogic))]
  [ModuleDependency(nameof(Configuration.Modules.DataAccess))]
  public class Module : IModule
  {
    public IUnityContainer Container { get; private set; }

    public Module(IUnityContainer container)
    {
      Container = container;
    }

    public void Initialize()
    {
      Container.RegisterType<Common.BLL.Interfaces.IItemsService, Common.BLL.Services.ItemsService>();
      Container.RegisterType<Common.BLL.Interfaces.IEventService, Common.BLL.Services.EventService();
    }
  }
}

using Microsoft.Practices.Unity;
using Prism.Modularity;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Modules.BLL.Services
{
  [Module(ModuleName = nameof(Configuration.Modules.BusinessLogic))]
  [ModuleDependency(nameof(Configuration.Modules.DataAccess))]
  public class BusinessLogicModule : IModule
  {
    public IUnityContainer Container { get; private set; }

    public BusinessLogicModule(IUnityContainer container)
    {
      this.Container = container;
    }

    public void Initialize()
    {
      this.Container.RegisterType<IItemsService, Services.ItemsService>();
      this.Container.RegisterType<IImageService, Services.ImageService>();
      this.Container.RegisterType<IEventService, Services.EventService>();
    }
  }
}

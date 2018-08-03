using Microsoft.Practices.Unity;
using Prism.Modularity;

namespace Ferretto.Common.BLL
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
      this.Container.RegisterType<Interfaces.IItemsService, Services.ItemsService>();
      this.Container.RegisterType<Interfaces.IImageService, Services.ImageService>();
      this.Container.RegisterType<Interfaces.IEventService, Services.EventService>();
    }
  }
}

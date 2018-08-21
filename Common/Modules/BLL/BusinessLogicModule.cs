using Microsoft.Practices.Unity;
using Prism.Modularity;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Modules.BLL.Services
{
  [Module(ModuleName = nameof(Utils.Modules.BusinessLogic))]
  [ModuleDependency(nameof(Utils.Modules.DataAccess))]
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
      this.Container.RegisterType<IItemsService, ItemsService>();
      this.Container.RegisterType<IImageService, ImageService>();
      this.Container.RegisterType<IEventService, EventService>();
    }
  }
}

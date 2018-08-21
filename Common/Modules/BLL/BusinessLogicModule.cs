using Microsoft.Practices.Unity;
using Prism.Modularity;
using Ferretto.Common.BLL.Interfaces;
using AutoMapper;

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
      this.Container.RegisterType<IItemsService, ItemsService>();
      this.Container.RegisterType<IImageService, ImageService>();
      this.Container.RegisterType<IEventService, EventService>();

      // TODO: in the future we may need to so something more complex http://docs.automapper.org/en/stable/Dependency-injection.html
      Mapper.Initialize(config => config.AddProfile<BusinessLogicAutoMapperProfile>());
      Mapper.Configuration.CompileMappings();
    }
  }
}

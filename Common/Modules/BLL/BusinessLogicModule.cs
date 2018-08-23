using AutoMapper;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Microsoft.Practices.Unity;
using Prism.Modularity;

namespace Ferretto.Common.Modules.BLL.Services
{
  [Module(ModuleName = nameof(Utils.Modules.BusinessLogic))]
  [ModuleDependency(nameof(Utils.Modules.DataAccess))]
  public class BusinessLogicModule : IModule
  {
    public IUnityContainer Container { get; private set; }

    public BusinessLogicModule(IUnityContainer container)
    {
      Container = container;
    }

    public void Initialize()
    {
      Container.RegisterType<ItemsService>(new ContainerControlledLifetimeManager())
        .RegisterType<IItemsService, ItemsService>()
        .RegisterType<IEntityService<IItem, int>, ItemsService>();
      Container.RegisterType<IImageService, ImageService>();
      Container.RegisterType<IEventService, EventService>();


      // TODO: in the future we may need to so something more complex http://docs.automapper.org/en/stable/Dependency-injection.html
      Mapper.Initialize(config => config.AddProfile<BusinessLogicAutoMapperProfile>());
      Mapper.Configuration.CompileMappings();
    }
  }
}

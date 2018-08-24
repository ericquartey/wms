using System.Linq;
using AutoMapper;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Microsoft.Practices.ServiceLocation;
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
      this.Container = container;
    }

    public void Initialize()
    {
      this.Container.RegisterType<ItemsService>(new ContainerControlledLifetimeManager())
        .RegisterType<IItemsService, ItemsService>()
        .RegisterType<IEntityService<IItem, int>, ItemsService>();
      this.Container.RegisterType<IImageService, ImageService>();
      this.Container.RegisterType<IEventService, EventService>();

      // TODO: in the future we may need to so something more complex http://docs.automapper.org/en/stable/Dependency-injection.html
      Mapper.Initialize(config => config.AddProfile<BusinessLogicAutoMapperProfile>());
      Mapper.Configuration.CompileMappings();

      // TODO: review this call to ensure we do a proper initialization of the entity framework
      var x = ServiceLocator.Current.GetInstance<ItemsService>().GetAll().First().Id;
    }
  }
}

using Ferretto.Common.BLL.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Modularity;

namespace Ferretto.Common.Modules.BLL.Services
{
  [Module(ModuleName = nameof(Utils.Modules.BusinessLogic))]
  [ModuleDependency(nameof(Utils.Modules.DataAccess))]
  public class BusinessLogicModule : IModule
  {
    #region Constructors

    public BusinessLogicModule(IUnityContainer container)
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
      this.Container.RegisterType<IImageService, ImageService>();
      this.Container.RegisterType<IEventService, EventService>();
      this.Container.RegisterType<IFilterService, FilterService>();
    }

    #endregion Methods
  }
}

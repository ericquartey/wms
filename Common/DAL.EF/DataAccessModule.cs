using Ferretto.Common.Models;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.DAL.EF
{
  [Module(ModuleName = nameof(Configuration.Modules.DataAccess))]
  public class DataAccessModule : IModule
  {
    public IUnityContainer Container { get; private set; }

    public DataAccessModule(IUnityContainer container)
    {
      this.Container = container;
    }

    public void Initialize()
    {
      this.Container.RegisterType<Interfaces.IUnitOfWork, UnitOfWork>();
      this.Container.RegisterType<Interfaces.IItemsRepository, ItemsRepository>();
      this.Container.RegisterType<Interfaces.IImageRepository, ImageFileRepository>();
    }
  }
}

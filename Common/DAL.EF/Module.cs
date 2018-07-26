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
  public class Module : IModule
  {
    public IUnityContainer Container { get; private set; }

    public Module(IUnityContainer container)
    {
      Container = container;
    }

    public void Initialize()
    {
      Container.RegisterType<Interfaces.IItemsRepository, ItemsRepository>();
    }
  }
}
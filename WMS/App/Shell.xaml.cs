using Ferretto.Common.Configuration;
using Prism.Modularity;
using Prism.Regions;
using System.Windows;

namespace Ferretto.WMS.App
{
  public partial class Shell : Window
  {
    private IModuleManager moduleManager;
    private IRegionManager regionManager;

    public Shell()
    {
      InitializeComponent();
    }

    public Shell(IModuleManager moduleManager, IRegionManager regionManager)
      : this()
    {
      this.moduleManager = moduleManager;
      this.regionManager = regionManager;

      this.moduleManager.LoadModule(nameof(Modules.DataAccess));      // TODO: remove the static LoadModules below when we will have the dynamic loading 
      this.moduleManager.LoadModule(nameof(Modules.BusinessLogic));   // TODO: remove the static LoadModules below when we will have the dynamic loading 

      this.moduleManager.LoadModule(nameof(Modules.Layout));
    
      this.moduleManager.LoadModule(nameof(Modules.Catalog));         // TODO: remove the static LoadModules below when we will have the dynamic loading    

    }
  }
}

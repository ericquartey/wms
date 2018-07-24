using Ferretto.Common.BLL;
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
      this.moduleManager.LoadModule(nameof(Modules.Layout)); 
      this.moduleManager.LoadModule(nameof(Modules.Catalog)); // To be removed when dinamically loaded from layout module.      
    }
  }
}

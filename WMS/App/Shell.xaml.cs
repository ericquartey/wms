using DevExpress.Xpf.Core;
using Ferretto.Common.Configuration;
using Prism.Modularity;
using Prism.Regions;
using System.Windows;

namespace Ferretto.WMS.App
{
  public partial class Shell : DXWindow
  {
    private readonly IModuleManager moduleManager;
    private readonly IRegionManager regionManager;

    public Shell()
    {
      InitializeComponent();
    }

    public Shell(IModuleManager moduleManager, IRegionManager regionManager)
      : this()
    {
      this.moduleManager = moduleManager;
      this.regionManager = regionManager;

      // Load dependency modyles
      // TODO this needs to be removed when we will be loading modules dynamically
      this.moduleManager.LoadModule(nameof(Modules.Catalog));

      // Load the root module of the application
      this.moduleManager.LoadModule(nameof(Modules.Layout));
   
    }
  }
}

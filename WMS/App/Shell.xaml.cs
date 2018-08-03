using DevExpress.Xpf.Core;
using Ferretto.Common.Configuration.Modules;
using Prism.Modularity;
using Prism.Regions;

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

      // Load the root module of the application
      this.moduleManager.LoadModule(nameof(Layout));
   
    }
  }
}

using DevExpress.Xpf.Core;
using Ferretto.Common.Configuration;
using Prism.Modularity;
using Prism.Regions;
using System.Windows;

namespace Ferretto.WMS.App
{
  public partial class Shell : DXWindow
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

      // Load the root module of the application
      this.moduleManager.LoadModule(nameof(Modules.Layout));
    }
  }
}

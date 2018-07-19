using Ferretto.Common.BLL;
using Prism.Modularity;
using Prism.Regions;
using System.Windows;

namespace Ferretto.WMS.App
{
  /// <summary>
  /// Logica di interazione per Shell.xaml
  /// </summary>
  public partial class Shell : Window
  {
    public IModuleManager moduleManager;
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
      this.moduleManager.LoadModule(nameof(Modules.Catalog));
      this.moduleManager.LoadModule(nameof(Modules.Layout));
    }
  }
}

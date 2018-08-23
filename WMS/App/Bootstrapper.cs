using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Prism;
using Microsoft.Practices.ServiceLocation;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using System.Configuration;
using System.Windows;

namespace Ferretto.WMS.App
{
  public class Bootstrapper : UnityBootstrapper
  {
    protected override DependencyObject CreateShell()
    {
      return this.Container.TryResolve<Shell>();
    }

    protected override IModuleCatalog CreateModuleCatalog()
    {
      return new DirectoryModuleCatalog { ModulePath = ConfigurationManager.AppSettings["PrismModulesPath"]  };
    }

    protected override void ConfigureModuleCatalog()
    {
      base.ConfigureModuleCatalog();

      (this.ModuleCatalog as DirectoryModuleCatalog)?.Load();
    }

    protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
    {
      var mappings = base.ConfigureRegionAdapterMappings();
      if (mappings != null)
      {
        var factory = ServiceLocator.Current.GetInstance<IRegionBehaviorFactory>();
        mappings.RegisterMapping(typeof(LayoutPanel), AdapterFactory.Make<RegionAdapterBase<LayoutPanel>>(factory));
      }
      return mappings;
    }

    protected override void InitializeShell()
    {
      base.InitializeShell();

      Application.Current.MainWindow = (Window)this.Shell;
      Application.Current.MainWindow.Show();
    }
  }
}

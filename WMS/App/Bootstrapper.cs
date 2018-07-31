using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Prism;
using Microsoft.Practices.ServiceLocation;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Windows;

namespace Ferretto.WMS.App
{
  public class Bootstrapper : UnityBootstrapper
  {
    protected override DependencyObject CreateShell()
    {
      return Container.TryResolve<Shell>();
    }

    protected override IModuleCatalog CreateModuleCatalog()
    {
      return new DirectoryModuleCatalog() { ModulePath = @".\" };
    }

    protected override void ConfigureModuleCatalog()
    {
      base.ConfigureModuleCatalog();

      (ModuleCatalog as DirectoryModuleCatalog)?.Load();
    }

    protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
    {
      RegionAdapterMappings mappings = base.ConfigureRegionAdapterMappings();
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

      App.Current.MainWindow = (Window)Shell;
      App.Current.MainWindow.Show();
    }
  }
}

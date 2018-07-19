using Prism.Modularity;
using Prism.Unity;
using System;
using System.Windows;

namespace Ferretto.WMS.App
{
  public class Bootstrapper : UnityBootstrapper
  {
    protected override DependencyObject CreateShell()
    {
      return  this.Container.TryResolve<Shell>();
    }

    protected override void ConfigureModuleCatalog()
    {
      base.ConfigureModuleCatalog();

      ModuleCatalog moduleCatalog = (ModuleCatalog)this.ModuleCatalog;

      #region Catalog
      moduleCatalog.AddModule(new ModuleInfo()
      {
        ModuleName = "Catalog",        
        ModuleType = "Ferretto.WMS.Comp.Catalog.Catalog,Ferretto.WMS.Comp.Catalog, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null",
        InitializationMode = InitializationMode.OnDemand
      });
      moduleCatalog.AddModule(new ModuleInfo()
      {
        ModuleName = "Layout",
        ModuleType = "Ferretto.WMS.Comp.Layout.Layout, Ferretto.WMS.Comp.Layout, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null",
        InitializationMode = InitializationMode.OnDemand
      });
      #endregion

    }

    protected override void InitializeShell()
    {
      base.InitializeShell();

      App.Current.MainWindow = (Window)this.Shell;
      App.Current.MainWindow.Show();
    }
  }
}

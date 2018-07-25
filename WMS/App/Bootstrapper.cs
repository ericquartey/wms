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
      return this.Container.TryResolve<Shell>();
    }

    protected override void ConfigureModuleCatalog()
    {
      base.ConfigureModuleCatalog();

      ModuleCatalog moduleCatalog = (ModuleCatalog)this.ModuleCatalog;

      #region Module Catalog
      moduleCatalog.AddModule(new ModuleInfo()
      {
        ModuleName = "Catalog",
        ModuleType = "Ferretto.WMS.Modules.Catalog.Module, Ferretto.WMS.Modules.Catalog, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null",
        InitializationMode = InitializationMode.OnDemand
      });
      moduleCatalog.AddModule(new ModuleInfo()
      {
        ModuleName = "Layout",
        ModuleType = "Ferretto.WMS.Modules.Layout.Module, Ferretto.WMS.Modules.Layout, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null",
        InitializationMode = InitializationMode.OnDemand
      });
      moduleCatalog.AddModule(new ModuleInfo()
      {
        ModuleName = nameof(Common.Configuration.Modules.DataAccess),
        ModuleType = "Ferretto.Common.DAL.EF.Module, Ferretto.Common.DAL.EF, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null",
        InitializationMode = InitializationMode.OnDemand
      });
      moduleCatalog.AddModule(new ModuleInfo()
      {
        ModuleName = nameof(Common.Configuration.Modules.BusinessLogic),
        ModuleType = "Ferretto.Common.BLL.Module, Ferretto.Common.BLL, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null",
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

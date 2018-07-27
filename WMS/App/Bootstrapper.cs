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

    protected override IModuleCatalog CreateModuleCatalog()
    {
      return new DirectoryModuleCatalog() { ModulePath = @".\" };
    }

    protected override void ConfigureModuleCatalog()
    {
      base.ConfigureModuleCatalog();

      (this.ModuleCatalog as DirectoryModuleCatalog)?.Load();
    }

    protected override void InitializeShell()
    {
      base.InitializeShell();

      App.Current.MainWindow = (Window)this.Shell;
      App.Current.MainWindow.Show();
    }
  }
}

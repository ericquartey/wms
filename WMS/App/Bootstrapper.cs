using System.Windows;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Prism;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;

namespace Ferretto.WMS.App
{
    public class Bootstrapper : UnityBootstrapper
    {
        #region Methods

        protected override void ConfigureModuleCatalog()
        {
            SplashScreenService.SetMessage(Common.Resources.DesktopApp.ConfiguringPrismModuleCatalog);

            base.ConfigureModuleCatalog();

            (this.ModuleCatalog as ModuleCatalog)?.Load();
        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            var mappings = base.ConfigureRegionAdapterMappings();
            if (mappings != null)
            {
                var factory = ServiceLocator.Current.GetInstance<IRegionBehaviorFactory>();
                mappings.RegisterMapping(typeof(LayoutPanel),
                    AdapterFactory.Make<RegionAdapterBase<LayoutPanel>>(factory));
            }

            return mappings;
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            SplashScreenService.SetMessage(Common.Resources.DesktopApp.LoadingPrismModuleCatalog);

            return new ConfigurationModuleCatalog();
        }

        protected override DependencyObject CreateShell()
        {
            return this.Container.TryResolve<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            SplashScreenService.SetMessage(Common.Resources.DesktopApp.OpeningMainWindow);

            Application.Current.MainWindow = (Window)this.Shell;
            Application.Current.MainWindow.Show();
        }

        #endregion Methods
    }
}

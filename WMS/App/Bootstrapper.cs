using System.Configuration;
using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Prism;
using Microsoft.Practices.ServiceLocation;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;

namespace Ferretto.WMS.App
{
    public class Bootstrapper : UnityBootstrapper
    {
        #region Constructors

        public Bootstrapper()
        {
        }

        #endregion Constructors

        #region Methods

        protected override void ConfigureModuleCatalog()
        {
            DXSplashScreen.SetState("Configuring Prism module catalog ...");

            base.ConfigureModuleCatalog();

            (this.ModuleCatalog as DirectoryModuleCatalog)?.Load();
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
            DXSplashScreen.SetState("Loading Prism module catalog ...");

            return new DirectoryModuleCatalog { ModulePath = ConfigurationManager.AppSettings["PrismModulesPath"] };
        }

        protected override DependencyObject CreateShell()
        {
            DXSplashScreen.SetState("Creating shell ...");

            return this.Container.TryResolve<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            DXSplashScreen.SetState("Opening main window ...");

            Application.Current.MainWindow = (Window)this.Shell;
            Application.Current.MainWindow.Show();
        }

        #endregion Methods
    }
}

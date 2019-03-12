using System.Windows;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Prism;
using Microsoft.Practices.ServiceLocation;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;

namespace Ferretto.WMS.App.Tests
{
    public class Bootstrapper : UnityBootstrapper
    {
        #region Methods

        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();

            (this.ModuleCatalog as ModuleCatalog)?.Load();
        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            var mappings = base.ConfigureRegionAdapterMappings();
            if (mappings != null)
            {
                DevExpress.Xpf.Core.ApplicationThemeHelper.UseLegacyDefaultTheme = true;
                var factory = ServiceLocator.Current.GetInstance<IRegionBehaviorFactory>();
                mappings.RegisterMapping(
                    typeof(LayoutPanel),
                    AdapterFactory.Make<RegionAdapterBase<LayoutPanel>>(factory));
            }

            return mappings;
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }

        protected override DependencyObject CreateShell()
        {
            return this.Container.TryResolve<Shell>();
        }

        #endregion
    }
}

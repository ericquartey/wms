using System.Configuration;
using Ferretto.VW.App.Accessories;
using Ferretto.VW.App.Modules.Login.Views;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.BarcodeReader.Newland;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.VW.App.Modules.Login
{
    [Module(ModuleName = nameof(Utils.Modules.Login), OnDemand = true)]
    public class LoginModule : IModule
    {
        #region Fields

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        #region Methods

        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.UseUiServices();

            containerProvider
                .Resolve<IRegionManager>()?
                .RegisterViewWithRegion(
                    $"{Utils.Modules.Layout.REGION_MAINCONTENT}",
                    typeof(LoaderView));

            this.logger.Trace(Resources.LoadLogin.ModuleLoaded);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ConfigureBarcodeReader(containerRegistry);

            containerRegistry.RegisterForNavigation<LoginView>();

            containerRegistry.RegisterForNavigation<LoaderView>(Utils.Modules.Login.LOADER);
        }

        private static void ConfigureBarcodeReader(IContainerRegistry containerRegistry)
        {
            containerRegistry.ConfigureBarcodeReaderUiServices();
            containerRegistry.ConfigureNewlandBarcodeReader();

            // var barcodes = new[] { "1234", "5678" };
            // containerRegistry.ConfigureMockBarcodeReader(barcodes, 10000);
        }

        #endregion
    }
}

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

            this.logger.Trace(Resources.Localized.Get("LoadLogin.ModuleLoaded"));
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

            // containerRegistry.ConfigureNewlandBarcodeReader();

            var barcodes = new[]
            {
                // user tokens
                // "1234",
                // "5678",

                // item actions
                // "ITEM_B8450BDD_FILTER",
                // "ITEM_ABC_FILTER",
                // "ITEM_B8450BDD_QTY5_PICK",

                // list actions
                // "LIST_100_EXEC",
                // "LIST_01_EXEC",
                // "LIST_0001_FILTER",

                // operations
                "B8450BDD",
                "FE8AFA5A",
                "B8450BDD_LOT_ABC_SN_123",
            };
            containerRegistry.ConfigureMockBarcodeReader(barcodes, 5000);
        }

        #endregion
    }
}

using Ferretto.VW.App.Accessories;
using Ferretto.VW.App.Modules.Login.Views;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
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
            containerProvider.UseMachineAutomationHubs();

            containerProvider
                .Resolve<IRegionManager>()?
                .RegisterViewWithRegion(
                    $"{Utils.Modules.Layout.REGION_MAINCONTENT}",
                    typeof(LoaderView));

            this.logger.Trace(Resources.Localized.Get("LoadLogin.ModuleLoaded"));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.ConfigureAlphaNumericBarUiServices();
            containerRegistry.ConfigureWeightingScaleUiServices();
            containerRegistry.ConfigureLaserPointerUiServices();
            containerRegistry.ConfigureCardReaderUiServices();
            containerRegistry.ConfigureBarcodeReaderUiServices();
            containerRegistry.ConfigureTokenReaderUiServices();

            containerRegistry.RegisterForNavigation<LoginView>();

            containerRegistry.RegisterForNavigation<LoaderView>(Utils.Modules.Login.LOADER);
        }

        #endregion
    }
}

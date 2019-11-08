using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Layout.Presentation;
using Ferretto.VW.App.Modules.Layout.Views;
using Ferretto.VW.App.Services;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.VW.App.Modules.Layout
{
    [Module(ModuleName = nameof(Utils.Modules.Layout), OnDemand = true)]
    public class LayoutModule : IModule
    {
        #region Fields

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        #region Methods

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();

            regionManager.RegisterViewWithRegion(
                            $"{Utils.Common.MAIN_REGION}",
                            typeof(LayoutView));

            regionManager.RegisterViewWithRegion(
                            $"{Utils.Modules.Layout.REGION_HEADER}",
                            typeof(HeaderView));

            regionManager.RegisterViewWithRegion(
                            $"{Utils.Modules.Layout.REGION_FOOTER}",
                            typeof(FooterView));

            this.logger.Trace("Module loaded.");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<Installation.ViewsAndViewModels.SingleViews.DiagnosticDetailsViewModel>();

            containerRegistry.Register<PresentationError>();
            containerRegistry.Register<PresentationMachinePowerSwitch>();
            containerRegistry.Register<PresentationMachineModeSwitch>();
            containerRegistry.Register<PresentationLogged>();
            containerRegistry.Register<PresentationHelp>();

            containerRegistry.Register<PresentationTheme>();
            containerRegistry.Register<PresentationShutdown>();

            containerRegistry.Register<PresentationNavigationStep>();
            containerRegistry.Register<PresentationAbort>();
            containerRegistry.Register<PresentationBack>();
            containerRegistry.Register<PresentationDebug>();
        }

        #endregion
    }
}

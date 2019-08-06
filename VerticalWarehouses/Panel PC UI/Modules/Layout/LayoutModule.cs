using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Layout.Presentation;
using Ferretto.VW.App.Modules.Layout.Views;
using Ferretto.VW.App.Services.Interfaces;
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

            //.RequestNavigate($"{Utils.Modules.Layout.REGION_MAINCONTENT}", "LoginView");

            this.logger.Trace("Module loaded.");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterForNavigation<LayoutView>(Utils.Modules.Layout.LAYOUT);
            //containerRegistry.Register<IView, LayoutView>(Utils.Modules.Layout.LAYOUT);

            containerRegistry.Register<IPresentation, PresentationTheme>(nameof(PresentationTheme));
            containerRegistry.Register<IPresentation, PresentationSwitch>(nameof(PresentationSwitch));
            containerRegistry.Register<IPresentation, PresentationBack>(nameof(PresentationBack));
        }

        #endregion
    }
}

﻿using Ferretto.VW.App.Modules.Login.Views;
using Ferretto.VW.App.Services;
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

            this.logger.Trace("Module loaded.");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<LoginView>();

            containerRegistry.RegisterForNavigation<LoaderView>(Utils.Modules.Login.LOADER);
        }

        #endregion
    }
}

using Ferretto.VW.App.Modules.Errors.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace Ferretto.VW.App.Modules.Errors
{
    [Module(ModuleName = nameof(Utils.Modules.Errors), OnDemand = true)]
    public class ErrorsModule : IModule
    {
        #region Fields

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        #region Methods

        public void OnInitialized(IContainerProvider containerProvider)
        {
            this.logger.Trace("Module loaded.");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ErrorDetailsView>();
            containerRegistry.RegisterForNavigation<ErrorInverterFaultView>();
            containerRegistry.RegisterForNavigation<ErrorLoadunitMissingView>();
            containerRegistry.RegisterForNavigation<ErrorLoadUnitErrorsView>();
            containerRegistry.RegisterForNavigation<ErrorZeroSensorView>();
        }

        #endregion
    }
}

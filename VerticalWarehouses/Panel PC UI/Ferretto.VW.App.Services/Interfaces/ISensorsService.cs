using Prism.Regions;

namespace Ferretto.VW.App.Services
{
    public interface ISensorsService : INavigationAware
    {
        #region Methods

        void EndMonitoring();

        void StartMonitoring();

        #endregion
    }
}

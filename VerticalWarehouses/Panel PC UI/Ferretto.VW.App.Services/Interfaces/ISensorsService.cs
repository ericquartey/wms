using System.Threading.Tasks;
using Prism.Regions;

namespace Ferretto.VW.App.Services
{
    public interface ISensorsService : INavigationAware
    {
        #region Methods

        void EndMonitoring();

        Task StartMonitoring();

        #endregion
    }
}

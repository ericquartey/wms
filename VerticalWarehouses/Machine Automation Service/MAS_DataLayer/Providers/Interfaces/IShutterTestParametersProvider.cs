using Ferretto.VW.MAS.DataLayer.Providers.Models;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IShutterTestParametersProvider
    {
        #region Methods

        ShutterTestParameters Get();

        #endregion
    }
}

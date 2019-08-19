using Ferretto.VW.MAS.DataLayer.Providers.Models;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface ISetupStatusProvider
    {
        #region Methods

        void CompleteBeltBurnishing();

        void CompleteVerticalOffset();

        void CompleteVerticalOrigin();

        void CompleteVerticalResolution();

        SetupStatusCapabilities Get();

        #endregion
    }
}

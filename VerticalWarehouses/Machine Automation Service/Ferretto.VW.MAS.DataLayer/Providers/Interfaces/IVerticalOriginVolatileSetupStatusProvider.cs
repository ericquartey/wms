using Ferretto.VW.MAS.DataLayer.Providers.Models;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IVerticalOriginVolatileSetupStatusProvider
    {
        #region Methods

        void Complete();

        SetupStepStatus Get();

        #endregion
    }
}

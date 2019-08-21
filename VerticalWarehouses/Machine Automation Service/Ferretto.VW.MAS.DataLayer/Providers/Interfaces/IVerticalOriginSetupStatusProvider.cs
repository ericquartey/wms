using Ferretto.VW.MAS.DataLayer.Providers.Models;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IVerticalOriginSetupStatusProvider
    {
        #region Methods

        void Complete();

        SetupStepStatus Get();

        #endregion
    }
}

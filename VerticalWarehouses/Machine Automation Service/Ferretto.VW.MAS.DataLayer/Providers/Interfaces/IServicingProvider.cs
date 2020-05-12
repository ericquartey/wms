using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IServicingProvider
    {
        #region Methods

        ServicingInfo GetInfo();
        void SetInstallationDate();

        #endregion
    }
}

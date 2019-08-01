using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IServicingProvider
    {
        #region Methods

        ServicingInfo GetInfo();

        #endregion
    }
}

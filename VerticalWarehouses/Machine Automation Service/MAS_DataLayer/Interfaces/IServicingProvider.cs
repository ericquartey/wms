using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IServicingProvider
    {
        #region Methods

        ServicingInfo GetInfo();

        #endregion
    }
}

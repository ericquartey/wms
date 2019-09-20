using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    public interface ISensorsProvider
    {
        #region Methods

        bool[] GetAll(BayNumber requestingBay);

        #endregion
    }
}

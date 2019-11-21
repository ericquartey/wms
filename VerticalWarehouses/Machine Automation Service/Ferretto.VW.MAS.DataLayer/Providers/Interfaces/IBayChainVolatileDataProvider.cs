using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal interface IBayChainVolatileDataProvider
    {
        #region Methods

        double GetPositionByBayNumber(BayNumber bayNumber);

        void SetPosition(BayNumber bayNumber, double position);

        #endregion
    }
}

using Ferretto.VW.MAS.DataModels.Errors;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IErrorStatisticsDataLayer
    {
        #region Methods

        ErrorStatisticsSummary GetErrorStatistics();

        #endregion
    }
}

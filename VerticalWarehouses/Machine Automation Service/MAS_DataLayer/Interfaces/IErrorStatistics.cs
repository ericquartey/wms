using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IErrorStatisticsDataLayer
    {
        #region Methods

        ErrorStatisticsSummary GetErrorStatistics();

        #endregion
    }
}

using Ferretto.VW.MAS.DataModels.Errors;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IErrorStatisticsProvider
    {
        #region Methods

        ErrorStatisticsSummary GetErrorStatistics();

        #endregion
    }
}

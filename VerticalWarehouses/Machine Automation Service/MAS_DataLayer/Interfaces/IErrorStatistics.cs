using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IErrorStatistics
    {
        #region Methods

        ErrorStatisticsSummary GetErrorStatistics();

        #endregion
    }
}

using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels.LoadingUnits;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface ILoadingUnitStatisticsDataLayer
    {
        #region Methods

        IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics();

        IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics();

        #endregion
    }
}

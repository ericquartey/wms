using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels.LoadingUnits;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface ILoadingUnitStatisticsProvider
    {
        #region Methods

        IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics();

        IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics();

        #endregion
    }
}

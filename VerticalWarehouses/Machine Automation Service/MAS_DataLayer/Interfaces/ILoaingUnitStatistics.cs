using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels.LoadingUnit;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface ILoadingUnitStatistics
    {
        #region Methods

        IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics();

        IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics();

        #endregion
    }
}

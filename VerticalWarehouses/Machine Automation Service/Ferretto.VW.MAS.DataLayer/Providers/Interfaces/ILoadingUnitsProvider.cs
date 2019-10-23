using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ILoadingUnitsProvider
    {
        #region Methods

        IEnumerable<LoadingUnit> GetAll();

        LoadingUnit GetById(int id);

        IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics();

        IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics();

        void SetHeight(int loadingUnitId, double height);

        void SetWeight(int loadingUnitId, double loadingUnitGrossWeight);

        #endregion
    }
}

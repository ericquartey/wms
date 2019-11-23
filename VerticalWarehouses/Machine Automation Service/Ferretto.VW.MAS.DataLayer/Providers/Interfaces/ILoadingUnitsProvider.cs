using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ILoadingUnitsProvider
    {
        #region Methods

        void Add(IEnumerable<LoadingUnit> loadingUnits);

        IEnumerable<LoadingUnit> GetAll();

        LoadingUnit GetById(int id);

        IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics();

        IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics();

        void Import(IEnumerable<LoadingUnit> loadingUnits);

        void Insert(int loadingUnitsId);

        void SetHeight(int loadingUnitId, double height);

        void SetWeight(int loadingUnitId, double loadingUnitGrossWeight);

        void UpdateRange(IEnumerable<LoadingUnit> loadingUnits);

        #endregion
    }
}

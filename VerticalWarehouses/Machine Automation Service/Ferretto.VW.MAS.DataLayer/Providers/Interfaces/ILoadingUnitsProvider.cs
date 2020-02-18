using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ILoadingUnitsDataProvider
    {
        #region Methods

        void Add(IEnumerable<LoadingUnit> loadingUnits);

        MachineErrorCode CheckWeight(int id);

        IEnumerable<LoadingUnit> GetAll();

        LoadingUnit GetById(int id);

        IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics();

        IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics();

        void Import(IEnumerable<LoadingUnit> loadingUnits, DataLayerContext context);

        void Insert(int loadingUnitsId);
        void Remove(int loadingUnitsId);
        void SetHeight(int loadingUnitId, double height);

        void SetWeight(int loadingUnitId, double loadingUnitGrossWeight);

        void UpdateRange(IEnumerable<LoadingUnit> loadingUnits, DataLayerContext dataContext);

        void UpdateWeightStatistics();

        #endregion
    }
}

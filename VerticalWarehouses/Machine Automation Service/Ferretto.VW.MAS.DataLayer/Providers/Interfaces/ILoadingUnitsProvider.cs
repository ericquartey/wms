using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ILoadingUnitsDataProvider
    {
        #region Methods

        void Add(IEnumerable<LoadingUnit> loadingUnits);

        IEnumerable<LoadingUnit> GetAll();

        LoadingUnit GetById(int id);

        IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics();

        IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics();

        void Import(IEnumerable<LoadingUnit> loadingUnits, DataLayerContext context);

        void Insert(int loadingUnitsId);

        void SetHeight(int loadingUnitId, double height);

        void SetWeight(int loadingUnitId, double loadingUnitGrossWeight);

        void UpdateRange(IEnumerable<LoadingUnit> loadingUnits, DataLayerContext dataContext);

        #endregion
    }
}

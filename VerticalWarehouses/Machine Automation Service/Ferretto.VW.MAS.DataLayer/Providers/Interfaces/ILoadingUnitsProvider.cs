using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface ILoadingUnitsProvider
    {
        #region Methods

        IEnumerable<LoadingUnit> GetAll();

        LoadingUnit GetById(int id);

        IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics();

        IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics();

        Task LoadFromAsync(string fileNamePath);
        void SetWeight(int loadingUnitId, double? loadingUnitGrossWeight);

        #endregion
    }
}

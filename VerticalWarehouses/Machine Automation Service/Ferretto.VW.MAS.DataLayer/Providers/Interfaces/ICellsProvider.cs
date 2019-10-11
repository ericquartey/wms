using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ICellsProvider
    {
        #region Methods

        IEnumerable<Cell> GetAll();

        CellStatisticsSummary GetStatistics();

        void LoadLoadingUnit(int? loadingUnitId, int? cellId);

        void UnloadLoadingUnit(int? cellId);

        Cell UpdateHeight(int cellId, double height);

        #endregion
    }
}

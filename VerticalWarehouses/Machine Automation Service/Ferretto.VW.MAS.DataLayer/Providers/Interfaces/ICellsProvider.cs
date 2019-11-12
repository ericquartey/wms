using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Cell = Ferretto.VW.MAS.DataModels.Cell;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ICellsProvider
    {
        #region Methods

        IEnumerable<Cell> GetAll();

        Cell GetByHeight(double cellHeight, double tolerance, WarehouseSide machineSide);

        Cell GetById(int cellId);

        Cell GetByLoadingUnitId(int loadingUnitId);

        CellStatisticsSummary GetStatistics();

        void LoadLoadingUnit(int loadingUnitId, int cellId);

        void UnloadLoadingUnit(int cellId);

        Cell UpdateHeight(int cellId, double height);

        IEnumerable<Cell> UpdateHeights(int fromCellId, int toCellId, WarehouseSide side, double height);

        #endregion
    }
}

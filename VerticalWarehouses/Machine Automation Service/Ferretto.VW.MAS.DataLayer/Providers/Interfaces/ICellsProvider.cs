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

        Cell GetCellByHeight(double cellHeight, double tolerance, WarehouseSide machineSide);

        Cell GetCellById(int cellId);

        Cell GetCellByLoadingUnit(int loadingUnitId);

        CellStatisticsSummary GetStatistics();

        void LoadLoadingUnit(int loadingUnitId, int cellId);

        void UnloadLoadingUnit(int cellId);

        Cell UpdateHeight(int cellId, double height);

        IEnumerable<Cell> UpdatesHeight(int fromCellId, int toCellId, WarehouseSide side, double height);

        #endregion
    }
}

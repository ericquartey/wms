using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Cell = Ferretto.VW.MAS.DataModels.Cell;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ICellsProvider
    {
        #region Methods

        bool CanFitLoadingUnit(int cellId, int loadingUnitId, bool isCellTest = false);

        int CleanUnderWeightCells();

        int FindDownCell(LoadingUnit loadingUnit);

        int FindEmptyCell(int loadingUnitId, CompactingType compactingType = CompactingType.NoCompacting, bool isCellTest = false);

        IEnumerable<Cell> GetAll();

        Cell GetById(int cellId);

        Cell GetByLoadingUnitId(int loadingUnitId);

        CellStatisticsSummary GetStatistics();

        bool IsCellToTest();

        void Save(Cell cell);

        int SetCellsToTest(BayNumber bayNumber);

        void SetLoadingUnit(int cellId, int? loadingUnitId);

        IEnumerable<Cell> UpdateHeights(int fromCellId, int toCellId, WarehouseSide side, double height);

        Cell UpdatePosition(int cellId, double height);

        #endregion
    }
}

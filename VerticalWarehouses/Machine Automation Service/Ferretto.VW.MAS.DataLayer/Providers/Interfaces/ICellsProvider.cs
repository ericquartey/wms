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

        int FindEmptyCell(int loadingUnitId, CompactingType compactingType = CompactingType.NoCompacting, bool isCellTest = false, bool randomCells = false);

        int FindTopCell(LoadingUnit loadingUnit);

        IEnumerable<Cell> GetAll();

        Cell GetById(int cellId);

        Cell GetByLoadingUnitId(int loadingUnitId);

        CellStatisticsSummary GetStatistics();

        bool IsCellToTest();

        bool IsTopCellAvailable(WarehouseSide side);

        void Save(Cell cell);

        void SaveCells(IEnumerable<Cell> cells);

        int SetCellsToTest(BayNumber bayNumber, double height);

        void SetLoadingUnit(int cellId, int? loadingUnitId);

        void SetRotationClass();

        void SetRotationClassFromUI(int cellId, string rotationClass);

        IEnumerable<Cell> UpdateHeights(int fromCellId, int toCellId, WarehouseSide side, double height);

        Cell UpdatePosition(int cellId, double height);

        #endregion
    }
}

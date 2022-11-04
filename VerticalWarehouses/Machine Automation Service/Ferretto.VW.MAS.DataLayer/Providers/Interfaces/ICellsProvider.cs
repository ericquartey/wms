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

        bool CanFitLoadingUnit(int cellId, int loadingUnitId, bool isCellTest, out string reason);

        int CleanUnderWeightCells();

        int FindDownCell(LoadingUnit loadingUnit);

        /// <summary>
        /// Try to find an empty cell for the LoadUnit passed.
        /// Store-in logic:
        ///     <br/>
        ///     - LU weight must not exceed total machine weight (this should be already controlled by weight check)
        ///     <br/>
        ///     - if LU height is not defined (conventionally: zero) set max height
        ///     <br/>
        ///     - try to select side with less weight
        ///     <br/>
        ///     - select only cells with enough space
        ///     <br/>
        ///     - for each free cell sort by:
        ///     <br/>
        ///     --rotation class distance - if enabled -
        ///     <br/>
        ///     --free space - try to fill the hole
        ///     <br/>
        ///     --priority
        ///     <br/>
        ///     - the priority field corresponds to the position, but it can be used to sort cells starting from bay positions, if bays are at a high level
        ///     <br/>
        ///     - Very heavy load units need one additional cell - if enabled -
        ///     <br/>
        ///     - if it does not find a cell it throws an exception
        ///     <br/>
        /// </summary>
        /// <param name="loadingUnitId"></param>
        /// <param name="compactingType">
        ///     - ExactMatchCompacting: The side is fixed and the space is not more than load unit height - NOT USED -
        ///     <br/>
        ///     - AnySpaceCompacting: The side is fixed
        ///     <br/>
        ///     - RotationCompacting: The destination cell must have a different rotation class than the load unit cell
        ///     </param>
        /// <param name="isCellTest">finds a cell marked for the first load unit test</param>
        /// <param name="randomCells">The available cells are not sorted, but randomly selected</param>
        /// <returns>the preferred cellId that fits the LoadUnit</returns>
        int FindEmptyCell(int loadingUnitId, CompactingType compactingType = CompactingType.NoCompacting, bool isCellTest = false, bool randomCells = false);

        int FindTopCell(LoadingUnit loadingUnit);
        void FreeReservedCells(LoadingUnit luDb);
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

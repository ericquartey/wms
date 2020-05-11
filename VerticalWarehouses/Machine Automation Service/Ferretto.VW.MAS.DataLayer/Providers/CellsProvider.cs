using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Cell = Ferretto.VW.MAS.DataModels.Cell;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class CellsProvider : ICellsProvider
    {
        #region Fields

        private const double CellHeight = 25;

        private const double VerticalPositionTolerance = 12.5;

        private static readonly Func<DataLayerContext, IEnumerable<Cell>> GetAllCompile =
            EF.CompileQuery((DataLayerContext context) =>
                context.Cells.AsNoTracking()
                .Include(c => c.Panel));

        private static readonly Func<DataLayerContext, int, Cell> GetByIdCompile =
                    EF.CompileQuery((DataLayerContext context, int cellId) =>
                context.Cells.AsNoTracking()
                .Include(c => c.Panel)
                .Include(c => c.LoadingUnit)
                .SingleOrDefault(c => c.Id == cellId));

        private readonly DataLayerContext dataContext;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILogger<DataLayerContext> logger;

        private readonly IMachineProvider machineProvider;

        #endregion

        #region Constructors

        public CellsProvider(DataLayerContext dataContext,
            IElevatorDataProvider elevatorDataProvider,
            IMachineProvider machineProvider,
            ILogger<DataLayerContext> logger)
        {
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.machineProvider = machineProvider ?? throw new System.ArgumentNullException(nameof(machineProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public bool CanFitLoadingUnit(int cellId, int loadingUnitId, bool isCellTest = false)
        {
            var cell = this.GetById(cellId);

            if (cell.LoadingUnit != null)
            {
                return false;
            }

            if (cell.BlockLevel != BlockLevel.None
                && (!isCellTest || cell.BlockLevel != BlockLevel.NeedsTest)
                )
            {
                return false;
            }

            var loadingUnit = this.dataContext.LoadingUnits
                .AsNoTracking()
                .Include(i => i.Cell)
                    .ThenInclude(c => c.Panel)
                .SingleOrDefault(l => l.Id == loadingUnitId);
            if (loadingUnit is null)
            {
                throw new EntityNotFoundException(loadingUnitId);
            }

            var cellsInRange = this.dataContext.Cells.Where(c => c.Panel.Side == cell.Side
                    && c.Position >= cell.Position
                    && c.Position <= cell.Position + loadingUnit.Height + VerticalPositionTolerance
                    );
            if (!cellsInRange.Any())
            {
                return false;
            }

            var availableSpace = cellsInRange.Last().Position - cellsInRange.First().Position + CellHeight;
            if (availableSpace < loadingUnit.Height + VerticalPositionTolerance)
            {
                return false;
            }

            if (loadingUnit.Cell != null)
            {
                // in cell-to-cell movements we check only the cells not presently occupied by this load unit
                var cellsFrom = this.dataContext.Cells.Where(c => c.Panel.Side == loadingUnit.Cell.Side
                    && c.Position >= loadingUnit.Cell.Position
                    && c.Position <= loadingUnit.Cell.Position + loadingUnit.Height + VerticalPositionTolerance);
                var lastPosition = cellsFrom.LastOrDefault().Position;
                return !cellsInRange.Any(c => (!c.IsFree && c.Position < loadingUnit.Cell.Position)
                    || (!c.IsFree && c.Position > lastPosition)
                    || c.BlockLevel == BlockLevel.Blocked
                    || (!isCellTest && c.BlockLevel == BlockLevel.NeedsTest)
                    );
            }

            return !cellsInRange.Any(c => !c.IsFree
                || c.BlockLevel == BlockLevel.Blocked
                || (!isCellTest && c.BlockLevel == BlockLevel.NeedsTest)
                );
        }

        public int FindDownCell(LoadingUnit loadingUnit)
        {
            if (loadingUnit.Cell is null)
            {
                this.logger.LogError($"FindEmptyCell for compacting: LU {loadingUnit.Id} not in cell! ");
                throw new EntityNotFoundException();
            }
            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);

            // load all cells below load unit
            var cells = this.GetAll(x => x.Position >= verticalAxis.LowerBound
                         && x.Position < verticalAxis.UpperBound
                         && x.Side == loadingUnit.Cell.Side
                         && x.Position < loadingUnit.Cell.Position)
                .OrderByDescending(o => o.Position)
                .ToList();
            int cellId = -1;
            foreach (var cell in cells)
            {
                if (cell.BlockLevel == BlockLevel.Blocked
                    || cell.BlockLevel == BlockLevel.NeedsTest
                    || !cell.IsFree
                    )
                {
                    break;
                }
                cellId = cell.Id;
            }
            if (cellId < 0)
            {
                throw new InvalidOperationException(Resources.Cells.NoEmptyCellsAvailable);
            }
            this.logger.LogInformation($"FindDownCell: found Cell {cellId} for LU {loadingUnit.Id}; from cell {loadingUnit.Cell.Id}");
            return cellId;
        }

        /// <summary>
        /// Try to find an empty cell for the LoadUnit passed.
        /// Store-in logic:
        ///     . LU weight must not exceed total machine weight (this should be already controlled by weight check)
        ///     . if LU height is not defined (conventionally: zero) set max height
        ///     . try to select side with less weight
        ///     . for each free cell measure available space:
        ///         select only cells with enough space and sort by priority
        ///     . the priority field corresponds to the position, but it can be used to sort cells starting from bay positions, if bays are at a high level
        /// if it does not find a cell it throws an exception
        /// </summary>
        /// <param name="loadingUnitId"></param>
        /// <param name="compactingType">
        ///     ExactMatchCompacting: The side is fixed and the space is not more than load unit height
        ///     AnySpaceCompacting: The side is fixed
        ///     </param>
        ///     <param name="isCellTest">finds a cell marked for the first load unit test</param>
        /// <returns>the preferred cellId that fits the LoadUnit</returns>
        public int FindEmptyCell(int loadingUnitId, CompactingType compactingType = CompactingType.NoCompacting, bool isCellTest = false)
        {
            var loadUnit = this.dataContext.LoadingUnits
                .AsNoTracking()
                .Include(i => i.Cell)
                    .ThenInclude(c => c.Panel)
                .SingleOrDefault(l => l.Id == loadingUnitId);
            if (loadUnit is null)
            {
                throw new EntityNotFoundException(loadingUnitId);
            }
            if (compactingType != CompactingType.NoCompacting && loadUnit.Cell is null)
            {
                this.logger.LogError($"FindEmptyCell for compacting: LU {loadingUnitId} not in cell! ");
                throw new EntityNotFoundException(loadingUnitId);
            }
            var machineStatistics = this.machineProvider.GetStatistics();
            if (machineStatistics is null)
            {
                throw new EntityNotFoundException();
            }
            var machine = this.machineProvider.Get();
            if (machine is null)
            {
                throw new EntityNotFoundException();
            }
            if (machineStatistics.TotalWeightFront + machineStatistics.TotalWeightBack + loadUnit.GrossWeight > machine.MaxGrossWeight)
            {
                this.logger.LogError($"FindEmptyCell: total weight exceeded for LU {loadingUnitId}; weight {loadUnit.GrossWeight:0.00}; " +
                    $"TotalWeightFront {machineStatistics.TotalWeightFront}; " +
                    $"TotalWeightBack {machineStatistics.TotalWeightBack}; " +
                    $"MaxGrossWeight {machine.MaxGrossWeight} ");
                throw new InvalidOperationException(Resources.Cells.NoEmptyCellsAvailable);
            }
            var preferredSide = WarehouseSide.NotSpecified;
            if (machineStatistics.TotalWeightFront + loadUnit.GrossWeight < machineStatistics.TotalWeightBack)
            {
                preferredSide = WarehouseSide.Front;
            }
            else if (machineStatistics.TotalWeightBack + loadUnit.GrossWeight < machineStatistics.TotalWeightFront)
            {
                preferredSide = WarehouseSide.Back;
            }

            double loadUnitHeight;
            if (loadUnit.Height == 0)
            {
                if (machine.LoadUnitMaxHeight == 0)
                {
                    throw new InvalidOperationException(Resources.Bays.TheBayLoadingMaxHeightNotValid);
                }
                loadUnitHeight = (isCellTest ? machine.LoadUnitMinHeight : machine.LoadUnitMaxHeight);
                this.logger.LogInformation($"FindEmptyCell: height is not defined for LU {loadingUnitId}; height is {loadUnitHeight} (as configured for {(isCellTest ? "min" : "max")});");
            }
            else
            {
                loadUnitHeight = loadUnit.Height;
            }

            using (var availableCell = new BlockingCollection<AvailableCell>())
            {
                var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);

                // load all cells
                var cells = this.GetAll(x => x.Position >= verticalAxis.LowerBound
                             && (compactingType == CompactingType.NoCompacting || x.Side == loadUnit.Cell.Side)
                             && (compactingType == CompactingType.NoCompacting || x.Position < loadUnit.Cell.Position))
                    .OrderBy(o => o.Position)
                    .ToList();
                // for each available cell we check if there is space for the requested height
                Parallel.ForEach(cells.Where(c => c.IsFree
                    && (isCellTest ? c.BlockLevel == BlockLevel.NeedsTest : c.BlockLevel == BlockLevel.None)
                    ), (cell) =>
                {
                    // load all cells following the selected cell
                    var cellsFollowing = cells.Where(c => c.Panel.Side == cell.Side
                        && c.Position >= cell.Position);

                    if (cellsFollowing.Any())
                    {
                        // measure available space
                        var lastCellPosition = cellsFollowing.Last().Position;
                        if (cellsFollowing.Count() > 1)
                        {
                            var firstUnavailable = cellsFollowing.FirstOrDefault(c => !c.IsFree || c.BlockLevel == BlockLevel.Blocked || c.BlockLevel == BlockLevel.NeedsTest);
                            if (firstUnavailable != null)
                            {
                                lastCellPosition = cellsFollowing.LastOrDefault(c => c.Position < firstUnavailable.Position)?.Position ?? lastCellPosition;
                            }
                        }
                        var availableSpace = lastCellPosition - cellsFollowing.First().Position + CellHeight;
                        var firstFree = cell.Id;
                        if (compactingType == CompactingType.ExactMatchCompacting || compactingType == CompactingType.AnySpaceCompacting)
                        {
                            // in these compacting types the cell must be the first empty cell
                            firstFree = cells.FirstOrDefault(c => c.Panel.Side == cell.Side
                                && c.Position < cell.Position
                                && c.IsFree
                                && c.BlockLevel == BlockLevel.None)?.Id ?? -1;
                        }
                        // check if load unit fits in available space
                        if (availableSpace >= loadUnitHeight + VerticalPositionTolerance
                            && (compactingType != CompactingType.AnySpaceCompacting
                                || firstFree == cell.Id
                                )
                            && (compactingType != CompactingType.ExactMatchCompacting
                                || (availableSpace < loadUnitHeight + (4 * VerticalPositionTolerance)
                                    && firstFree == cell.Id
                                    )
                                )
                            )
                        {
                            availableCell.Add(new AvailableCell(cell, availableSpace));
                        }
                    }
                });

                if (!availableCell.Any())
                {
                    if (isCellTest)
                    {
                        this.logger.LogError($"FindEmptyCell: cell to test not found for LU {loadingUnitId}; Height {loadUnitHeight:0.00}; total cells {cells.Count}; ");
                    }
                    else if (compactingType == CompactingType.NoCompacting)
                    {
                        this.logger.LogError($"FindEmptyCell: cell not found for LU {loadingUnitId}; Height {loadUnitHeight:0.00}; total cells {cells.Count}; ");
                    }
                    else
                    {
                        this.logger.LogTrace($"FindEmptyCell: cell not found for LU {loadingUnitId}; Height {loadUnitHeight:0.00}; side {loadUnit.Cell.Side}; position {loadUnit.Cell.Position}; total cells {cells.Count}; ");
                    }
                    throw new InvalidOperationException(Resources.Cells.NoEmptyCellsAvailable);
                }

                // start from lower cells
                var foundCell = availableCell.OrderBy(o => (preferredSide != WarehouseSide.NotSpecified && o.Cell.Side == preferredSide) ? 0 : 1).ThenBy(t => t.Cell.Priority).First();
                var cellId = foundCell.Cell.Id;
                this.logger.LogInformation($"FindEmptyCell: found Cell {cellId} for LU {loadingUnitId}; " +
                    $"Height {loadUnitHeight:0.00}; " +
                    $"Weight {loadUnit.GrossWeight:0.00}; " +
                    $"preferredSide {preferredSide}; " +
                    $"total cells {cells.Count}; " +
                    $"available cells {availableCell.Count}; " +
                    $"available space {foundCell.Height}; " +
                    $"TotalWeightFront {machineStatistics.TotalWeightFront:0.00}; " +
                    $"TotalWeightBack {machineStatistics.TotalWeightBack:0.00}");
                return cellId;
            }
        }

        public IEnumerable<Cell> GetAll()
        {
            lock (this.dataContext)
            {
                return GetAllCompile(this.dataContext).ToArray();
            }
        }

        public IEnumerable<Cell> GetAll(Func<Cell, bool> predicate)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Cells
                    .AsNoTracking()
                    .Include(c => c.Panel)
                    .Where(predicate)
                    .ToArray();
            }
        }

        public Cell GetById(int cellId)
        {
            return GetByIdCompile(this.dataContext, cellId);
        }

        public Cell GetByLoadingUnitId(int loadingUnitId)
        {
            return this.dataContext.Cells
                .Include(c => c.LoadingUnit)
                .Include(c => c.Panel)
                .SingleOrDefault(c => c.LoadingUnit.Id == loadingUnitId);
        }

        public CellStatisticsSummary GetStatistics()
        {
            lock (this.dataContext)
            {
                var totalCells = this.dataContext.Cells.Count();

                var cellsWithSide = this.dataContext.Cells.Include(c => c.Panel).ToArray();

                var cellStatusStatistics = cellsWithSide
                    .GroupBy(c => c.IsFree)
                    .Select(g =>
                        new CellStatusStatistics
                        {
                            IsFree = g.Key,
                            TotalFrontCells = g.Count(c => c.Side == WarehouseSide.Front),
                            TotalBackCells = g.Count(c => c.Side == WarehouseSide.Back),
                            RatioFrontCells = g.Count(c => c.Side == WarehouseSide.Front) / (double)totalCells,
                            RatioBackCells = g.Count(c => c.Side == WarehouseSide.Back) / (double)totalCells,
                        });

                var occupiedOrUnusableCellsCount = this.dataContext.Cells
                    .Count(c => !c.IsFree || c.BlockLevel == BlockLevel.Blocked || c.BlockLevel == BlockLevel.Undefined);

                var cellStatistics = new CellStatisticsSummary()
                {
                    CellStatusStatistics = cellStatusStatistics,
                    TotalCells = totalCells,
                    TotalFrontCells = cellsWithSide.Count(c => c.Side == WarehouseSide.Front),
                    TotalBackCells = cellsWithSide.Count(c => c.Side == WarehouseSide.Front),
                    CellOccupationPercentage = 100.0 * occupiedOrUnusableCellsCount / totalCells
                };
                var machine = this.machineProvider.Get();
                var minSpace = machine.LoadUnitMinHeight / 25;
                var freeBlocksFront = this.FreeBlocks(cellsWithSide, WarehouseSide.Front, out var freeCellsFront);
                freeCellsFront /= minSpace;
                var freeBlocksBack = this.FreeBlocks(cellsWithSide, WarehouseSide.Back, out var freeCellsBack);
                freeCellsBack /= minSpace;
                cellStatistics.FragmentFrontPercent = (freeBlocksFront > 0 && freeBlocksFront <= freeCellsFront) ? (freeBlocksFront / freeCellsFront) * 100 : 100;
                cellStatistics.FragmentBackPercent = (freeBlocksBack > 0 && freeBlocksBack <= freeCellsBack) ? (freeBlocksBack / freeCellsBack) * 100 : 100;
                if (cellStatistics.FragmentFrontPercent == 100 || cellStatistics.FragmentBackPercent == 100)
                {
                    cellStatistics.FragmentTotalPercent = (cellStatistics.FragmentFrontPercent + cellStatistics.FragmentBackPercent) / 2;
                }
                else
                {
                    cellStatistics.FragmentTotalPercent = (freeBlocksFront + freeBlocksBack > 0 && freeBlocksFront + freeBlocksBack <= freeCellsFront + freeCellsBack) ? ((freeBlocksFront + freeBlocksBack) / (freeCellsFront + freeCellsBack)) * 100 : 100;
                }

                return cellStatistics;
            }
        }

        public bool IsCellToTest()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Cells.Any(c => c.BlockLevel == BlockLevel.NeedsTest);
            }
        }

        public void Save(Cell cell)
        {
            lock (this.dataContext)
            {
                this.dataContext.AddOrUpdate(cell, f => f.Id);
                this.dataContext.SaveChanges();
            }
        }

        public int SetCellsToTest()
        {
            lock (this.dataContext)
            {
                var cells = this.dataContext.Cells
                    .Include(c => c.Panel)
                    .ToArray()                      // this ToArray is needed to sort correctly
                    .OrderBy(o => o.Side)
                    .ThenBy(t => t.Position)
                    .ToArray();

                // remove all cells to test remained by other tests
                cells.ForEach(c => { if (c.BlockLevel == BlockLevel.NeedsTest) { c.BlockLevel = BlockLevel.None; } });

                var count = 0;

                // find all border cells: the first and last cell by side and cells near not available cells
                for (int iCell = 0; iCell < cells.Length; iCell++)
                {
                    var cell = cells[iCell];
                    if (cell.BlockLevel == BlockLevel.None)
                    {
                        Cell next = null;
                        if (iCell < cells.Length - 1
                            && cells[iCell + 1].Side == cell.Side
                            )
                        {
                            next = cells[iCell + 1];
                        }
                        if (next == null
                            || (next.BlockLevel != BlockLevel.None && next.BlockLevel != BlockLevel.NeedsTest)
                            )
                        {
                            cell.BlockLevel = BlockLevel.NeedsTest;
                            count++;
                        }
                        else
                        {
                            Cell prev = null;
                            if (iCell > 0
                                && cells[iCell - 1].Side == cell.Side
                                )
                            {
                                prev = cells[iCell - 1];
                            }
                            if (prev == null
                                || (prev.BlockLevel != BlockLevel.None && prev.BlockLevel != BlockLevel.NeedsTest)
                                )
                            {
                                cell.BlockLevel = BlockLevel.NeedsTest;
                                count++;
                            }
                        }
                    }
                }
                this.dataContext.SaveChanges();
                return count;
            }
        }

        /// <summary>
        /// it frees or occupies the cells starting from cellId depending on loadingUnitId is null or not
        /// </summary>
        /// <param name="cellId"></param>
        /// <param name="loadingUnitId"></param>
        public void SetLoadingUnit(int cellId, int? loadingUnitId)
        {
            lock (this.dataContext)
            {
                var cell = this.dataContext.Cells
                .Include(c => c.LoadingUnit)
                .Include(c => c.Panel)
                .SingleOrDefault(c => c.Id == cellId);

                if (cell is null)
                {
                    throw new EntityNotFoundException(cellId);
                }

                var statistics = this.dataContext.MachineStatistics.FirstOrDefault();

                if (loadingUnitId is null)
                {
                    if (cell.LoadingUnit is null)
                    {
                        return;
                    }
                    var loadingUnit = cell.LoadingUnit;
                    loadingUnit.IsIntoMachine = false;

                    var occupiedCells = this.dataContext.Cells
                        .Include(c => c.LoadingUnit)
                        .Where(c =>
                            c.Panel.Side == cell.Side
                            &&
                            c.Position >= cell.Position
                            &&
                            c.Position <= cell.Position + loadingUnit.Height + VerticalPositionTolerance)
                        .ToArray();

                    var weight = loadingUnit.GrossWeight;
                    if (cell.Side is WarehouseSide.Front)
                    {
                        statistics.TotalWeightFront -= weight;
                        if (statistics.TotalWeightFront < 0)
                        {
                            statistics.TotalWeightFront = 0;
                        }
                    }
                    else
                    {
                        statistics.TotalWeightBack -= weight;
                        if (statistics.TotalWeightBack < 0)
                        {
                            statistics.TotalWeightBack = 0;
                        }
                    }

                    foreach (var occupiedCell in occupiedCells)
                    {
                        if (occupiedCell.LoadingUnit != null && occupiedCell.LoadingUnit.Id != loadingUnit.Id)
                        {
                            throw new InvalidOperationException(Resources.Cells.TheCellUnexpectedlyContainsAnotherLoadingUnit);
                        }

                        if (occupiedCell.IsFree)
                        {
                            throw new InvalidOperationException(Resources.Cells.TheCellIsUnexpectedlyFree);
                        }

                        occupiedCell.IsFree = true;
                        occupiedCell.LoadingUnit = null;
                    }
                }
                else
                {
                    if (cell.BlockLevel == BlockLevel.SpaceOnly)
                    {
                        throw new InvalidOperationException(Resources.Cells.TheTargetCellIsSpaceOnly);
                    }

                    if (cell.BlockLevel == BlockLevel.Blocked)
                    {
                        throw new InvalidOperationException(Resources.Cells.TheTargetCellIsBlocked);
                    }

                    if (cell.LoadingUnit != null)
                    {
                        throw new InvalidOperationException(Resources.Cells.TheCellAlreadyContainsAnotherLoadingUnit);
                    }

                    if (cell.BlockLevel == BlockLevel.NeedsTest)
                    {
                        cell.BlockLevel = BlockLevel.None;
                    }

                    var loadingUnit = this.dataContext.LoadingUnits
                        .SingleOrDefault(l => l.Id == loadingUnitId);
                    if (loadingUnit is null)
                    {
                        throw new EntityNotFoundException(loadingUnitId.Value);
                    }

                    if (loadingUnit.CellId != null)
                    {
                        throw new InvalidOperationException(Resources.Cells.TheLoadingUnitIsAlreadyLocatedInAnotherCell);
                    }

                    var freeCells = this.dataContext.Cells
                       .Include(c => c.LoadingUnit)
                       .Where(c =>
                           c.Panel.Side == cell.Side
                           &&
                           c.Position >= cell.Position
                           &&
                           c.Position <= cell.Position + loadingUnit.Height + VerticalPositionTolerance)
                       .ToArray();

                    foreach (var freeCell in freeCells)
                    {
                        freeCell.IsFree = false;
                        if (freeCell.LoadingUnit != null)
                        {
                            throw new InvalidOperationException(Resources.Cells.TheCellUnexpectedlyContainsAnotherLoadingUnit);
                        }

                        if (freeCell.BlockLevel == BlockLevel.Blocked)
                        {
                            throw new InvalidOperationException(Resources.Cells.TheLoadingCannotOccupyABlockedCell);
                        }
                    }

                    loadingUnit.IsIntoMachine = true;
                    loadingUnit.Status = DataModels.Enumerations.LoadingUnitStatus.InLocation;
                    cell.LoadingUnit = loadingUnit;

                    var weight = loadingUnit.GrossWeight;
                    if (cell.Side == WarehouseSide.Front)
                    {
                        statistics.TotalWeightFront += weight;
                    }
                    else
                    {
                        statistics.TotalWeightBack += weight;
                    }
                }
                var machine = this.machineProvider.Get();
                if (machine != null
                    && machine.MaxGrossWeight != 0
                    )
                {
                    statistics.WeightCapacityPercentage = ((statistics.TotalWeightFront + statistics.TotalWeightBack) / machine.MaxGrossWeight) * 100;
                }
                this.dataContext.SaveChanges();
            }
        }

        public IEnumerable<Cell> UpdateHeights(int fromCellId, int toCellId, WarehouseSide side, double height)
        {
            lock (this.dataContext)
            {
                var res = new List<Cell>();
                for (var cellId = fromCellId; cellId <= toCellId; cellId++)
                {
                    var cell = this.dataContext.Cells
                        .Include(c => c.Panel)
                        .SingleOrDefault(c => c.Id == cellId);
                    if (cell != null && cell.Side == side)
                    {
                        cell.Position += height;

                        this.dataContext.Cells.Update(cell);
                        this.dataContext.SaveChanges();

                        res.Add(this.dataContext.Cells
                                    .Include(c => c.Panel)
                                    .SingleOrDefault(c => c.Id == cellId));
                    }
                }

                return res;
            }
        }

        public Cell UpdatePosition(int cellId, double position)
        {
            lock (this.dataContext)
            {
                var cell = this.dataContext.Cells
                    .Include(c => c.Panel)
                    .SingleOrDefault(c => c.Id == cellId);

                if (cell is null)
                {
                    throw new EntityNotFoundException(cellId);
                }

                var cellsOnSameSide = this.dataContext.Cells
                    .Where(c => c.Side == cell.Side)
                    .OrderBy(c => c.Position);

                var higherCell = cellsOnSameSide.FirstOrDefault(c => c.Position > cell.Position);
                var lowerCell = cellsOnSameSide.FirstOrDefault(c => c.Position < cell.Position);

                if ((higherCell == null
                    ||
                    higherCell.Position > position)
                    &&
                    (lowerCell == null
                    ||
                    lowerCell.Position < position))
                {
                    cell.Position = position;

                    this.dataContext.Cells.Update(cell);
                    this.dataContext.SaveChanges();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(
                        Resources.Cells.TheSpecifiedHeightIsNotBetweenTheAdjacentCellsHeights);
                }

                return this.dataContext.Cells
                    .Include(c => c.Panel)
                    .SingleOrDefault(c => c.Id == cellId);
            }
        }

        private int FreeBlocks(Cell[] cellsWithSide, WarehouseSide side, out double freeCells)
        {
            int count = 0;
            freeCells = 0;
            var cellsBySide = cellsWithSide.Where(c => c.Side == side)
                .OrderBy(o => o.Position)
                .ToArray();

            for (int i = 0; i < cellsBySide.Length; i++)
            {
                if (cellsBySide[i].IsFree
                    && cellsBySide[i].BlockLevel == BlockLevel.None
                    && (
                        (i == 0)
                        || !cellsBySide[i - 1].IsFree
                        || cellsBySide[i - 1].BlockLevel != BlockLevel.None
                        )
                    )
                {
                    count++;
                }
            }
            if (count < 1)
            {
                return 0;
            }
            freeCells = cellsBySide.Count(c => c.IsFree && (c.BlockLevel == BlockLevel.None || c.BlockLevel == BlockLevel.SpaceOnly));
            if (freeCells == 0)
            {
                return 0;
            }
            return count;
        }

        #endregion

        /*
        public Cell GetByHeight(double cellHeight, double tolerance, WarehouseSide machineSide)
        {
            return this.dataContext.Cells
                .Include(c => c.LoadingUnit)
                .Include(c => c.Panel)
                .SingleOrDefault(c => c.Position < cellHeight + tolerance && c.Position > cellHeight - tolerance && c.Panel.Side == machineSide);
        }
        */
    }
}

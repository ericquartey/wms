using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Cell = Ferretto.VW.MAS.DataModels.Cell;
using CellStatus = Ferretto.VW.MAS.DataModels.CellStatus;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class CellsProvider : ICellsProvider
    {
        #region Fields

        private const double CellHeight = 25;

        private const double VerticalPositionTolerance = 12.5;

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

        public bool CanFitLoadingUnit(int cellId, int loadingUnitId)
        {
            var cell = this.GetById(cellId);

            if (cell.LoadingUnit != null)
            {
                return false;
            }

            if (cell.IsUnusable || cell.IsDeactivated)
            {
                return false;
            }

            var loadingUnit = this.dataContext.LoadingUnits.SingleOrDefault(l => l.Id == loadingUnitId);
            if (loadingUnit is null)
            {
                throw new EntityNotFoundException(loadingUnitId);
            }

            var cellsInRange = this.dataContext.Cells.Where(
                c =>
                    c.Panel.Side == cell.Side
                    &&
                    c.Position >= cell.Position
                    &&
                    c.Position <= cell.Position + loadingUnit.Height + VerticalPositionTolerance);

            return !cellsInRange.Any(c => c.Status == CellStatus.Occupied || c.IsUnusable);
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
                if (cell.IsUnusable
                    || cell.IsDeactivated
                    || cell.Status == CellStatus.Occupied
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
        /// <param name="isCompacting">The side is fixed</param>
        /// <returns>the preferred cellId that fits the LoadUnit</returns>
        public int FindEmptyCell(int loadingUnitId, bool isCompacting = false)
        {
            var loadingUnit = this.dataContext.LoadingUnits
                .AsNoTracking()
                .Include(i => i.Cell)
                .SingleOrDefault(l => l.Id == loadingUnitId);
            if (loadingUnit is null)
            {
                throw new EntityNotFoundException(loadingUnitId);
            }
            if (isCompacting && loadingUnit.Cell is null)
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
            if (machineStatistics.TotalWeightFront + machineStatistics.TotalWeightBack + loadingUnit.GrossWeight > machine.MaxGrossWeight)
            {
                this.logger.LogError($"FindEmptyCell: total weight exceeded for LU {loadingUnitId}; weight {loadingUnit.GrossWeight}; " +
                    $"TotalWeightFront {machineStatistics.TotalWeightFront}; " +
                    $"TotalWeightBack {machineStatistics.TotalWeightBack}; " +
                    $"MaxGrossWeight {machine.MaxGrossWeight} ");
                throw new InvalidOperationException(Resources.Cells.NoEmptyCellsAvailable);
            }
            var preferredSide = WarehouseSide.NotSpecified;
            if (machineStatistics.TotalWeightFront + loadingUnit.GrossWeight < machineStatistics.TotalWeightBack)
            {
                preferredSide = WarehouseSide.Front;
            }
            else if (machineStatistics.TotalWeightBack + loadingUnit.GrossWeight < machineStatistics.TotalWeightFront)
            {
                preferredSide = WarehouseSide.Back;
            }
            if (loadingUnit.Height == 0)
            {
                if (machine.LoadUnitMaxHeight == 0)
                {
                    throw new InvalidOperationException("LoadUnitMaxHeight is not valid");
                }
                loadingUnit.Height = machine.LoadUnitMaxHeight;
                this.logger.LogInformation($"FindEmptyCell: height is not defined for LU {loadingUnitId}; new height is {loadingUnit.Height} (as configured for max);");
            }
            using (var availableCell = new BlockingCollection<AvailableCell>())
            {
                var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);

                // load all cells
                var cells = this.GetAll(x => x.Position >= verticalAxis.LowerBound
                             && x.Position < verticalAxis.UpperBound
                             && (!isCompacting || x.Side == loadingUnit.Cell.Side)
                             && (!isCompacting || x.Position < loadingUnit.Cell.Position))
                    .OrderBy(o => o.Position)
                    .ToList();
                // for each available cell we check if there is space for the requested height
                Parallel.ForEach(cells.Where(c => c.Status != CellStatus.Occupied && !c.IsUnusable), (cell) =>
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
                            var firstUnavailable = cellsFollowing.FirstOrDefault(c => c.Status == CellStatus.Occupied || c.IsUnusable);
                            if (firstUnavailable != null)
                            {
                                lastCellPosition = cellsFollowing.LastOrDefault(c => c.Position < firstUnavailable.Position)?.Position ?? lastCellPosition;
                            }
                        }
                        var availableSpace = lastCellPosition - cellsFollowing.First().Position + CellHeight;

                        // check if load unit fits in available space
                        if (availableSpace >= loadingUnit.Height + VerticalPositionTolerance)
                        {
                            availableCell.Add(new AvailableCell(cell, availableSpace));
                        }
                    }
                });

                if (!availableCell.Any())
                {
                    if (!isCompacting)
                    {
                        this.logger.LogError($"FindEmptyCell: cell not found for LU {loadingUnitId}; Height {loadingUnit.Height}; total cells {cells.Count}; ");
                    }
                    throw new InvalidOperationException(Resources.Cells.NoEmptyCellsAvailable);
                }

                // start from lower cells
                var cellId = availableCell.OrderBy(o => (preferredSide != WarehouseSide.NotSpecified && o.Cell.Side == preferredSide) ? 0 : 1).ThenBy(t => t.Cell.Priority).First().Cell.Id;
                this.logger.LogDebug($"FindEmptyCell: found Cell {cellId} for LU {loadingUnitId}; " +
                    $"Height {loadingUnit.Height}; " +
                    $"Weight {loadingUnit.GrossWeight}; " +
                    $"preferredSide {preferredSide}; " +
                    $"total cells {cells.Count}; " +
                    $"available cells {availableCell.Count}; " +
                    $"TotalWeightFront {machineStatistics.TotalWeightFront}; " +
                    $"TotalWeightBack {machineStatistics.TotalWeightBack}");
                return cellId;
            }
        }

        public IEnumerable<Cell> GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Cells
                    .Include(c => c.Panel)
                    .ToArray();
            }
        }

        public IEnumerable<Cell> GetAll(Func<Cell, bool> predicate)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Cells
                    .Include(c => c.Panel)
                    .Where(predicate)
                    .ToArray();
            }
        }

        /*
        public Cell GetByHeight(double cellHeight, double tolerance, WarehouseSide machineSide)
        {
            return this.dataContext.Cells
                .Include(c => c.LoadingUnit)
                .Include(c => c.Panel)
                .SingleOrDefault(c => c.Position < cellHeight + tolerance && c.Position > cellHeight - tolerance && c.Panel.Side == machineSide);
        }
        */

        public Cell GetById(int cellId)
        {
            return this.dataContext.Cells
                .Include(c => c.Panel)
                .Include(c => c.LoadingUnit)
                .SingleOrDefault(c => c.Id == cellId);
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

                var cellsWithSide = this.dataContext.Cells.Include(c => c.Panel);

                var cellStatusStatistics = cellsWithSide
                    .GroupBy(c => c.Status)
                    .Select(g =>
                        new CellStatusStatistics
                        {
                            Status = g.Key,
                            TotalFrontCells = g.Count(c => c.Side == WarehouseSide.Front),
                            TotalBackCells = g.Count(c => c.Side == WarehouseSide.Back),
                            RatioFrontCells = g.Count(c => c.Side == WarehouseSide.Front) / (double)totalCells,
                            RatioBackCells = g.Count(c => c.Side == WarehouseSide.Back) / (double)totalCells,
                        });

                var occupiedOrUnusableCellsCount = this.dataContext.Cells
                    .Count(c => c.Status == CellStatus.Occupied || c.IsUnusable);

                var cellStatistics = new CellStatisticsSummary()
                {
                    CellStatusStatistics = cellStatusStatistics,
                    TotalCells = totalCells,
                    TotalFrontCells = cellsWithSide.Count(c => c.Side == WarehouseSide.Front),
                    TotalBackCells = cellsWithSide.Count(c => c.Side == WarehouseSide.Front),
                    CellOccupationPercentage = 100.0 * occupiedOrUnusableCellsCount / totalCells,
                };

                return cellStatistics;
            }
        }

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

                    var occupiedCells = this.dataContext.Cells
                        .Include(c => c.LoadingUnit)
                        .Where(c =>
                            c.Panel.Side == cell.Side
                            &&
                            c.Position >= cell.Position
                            &&
                            c.Position <= cell.Position + cell.LoadingUnit.Height + VerticalPositionTolerance)
                        .ToArray();

                    double weight = cell.LoadingUnit.GrossWeight;
                    if (cell.Side == WarehouseSide.Front)
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
                        if (occupiedCell.LoadingUnit != null && occupiedCell.LoadingUnit.Id != cell.LoadingUnit.Id)
                        {
                            throw new InvalidOperationException(Resources.Cells.TheCellUnexpectedlyContainsAnotherLoadingUnit);
                        }

                        //if (occupiedCell.Status != CellStatus.Occupied)
                        //{
                        //    throw new InvalidOperationException(Resources.Cells.TheCellIsUnexpectedlyFree);
                        //}

                        occupiedCell.Status = CellStatus.Free;
                        occupiedCell.LoadingUnit = null;
                    }
                }
                else
                {
                    if (cell.IsDeactivated)
                    {
                        throw new InvalidOperationException(Resources.Cells.TheTargetCellIsDeactivated);
                    }

                    if (cell.IsUnusable)
                    {
                        throw new InvalidOperationException(Resources.Cells.TheTargetCellIsUnusable);
                    }

                    if (cell.LoadingUnit != null)
                    {
                        throw new InvalidOperationException(Resources.Cells.TheCellAlreadyContainsAnotherLoadingUnit);
                    }

                    var loadingUnit = this.dataContext.LoadingUnits
                        .AsNoTracking()
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
                        freeCell.Status = CellStatus.Occupied;
                        if (freeCell.LoadingUnit != null)
                        {
                            throw new InvalidOperationException(Resources.Cells.TheCellUnexpectedlyContainsAnotherLoadingUnit);
                        }

                        if (freeCell.IsDeactivated)
                        {
                            throw new InvalidOperationException(Resources.Cells.TheLoadingCannotBePlacedOppositeADeactivatedCell);
                        }
                    }
                    // TODO check if this could be done better
                    cell.LoadingUnit = this.dataContext.LoadingUnits.SingleOrDefault(l => l.Id == loadingUnitId);

                    double weight = loadingUnit.GrossWeight;
                    if (cell.Side == WarehouseSide.Front)
                    {
                        statistics.TotalWeightFront += weight;
                    }
                    else
                    {
                        statistics.TotalWeightBack += weight;
                    }
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

        #endregion
    }
}

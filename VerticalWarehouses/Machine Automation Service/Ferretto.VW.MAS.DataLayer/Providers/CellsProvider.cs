using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Migrations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Cell = Ferretto.VW.MAS.DataModels.Cell;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class CellsProvider : ICellsProvider
    {
        #region Fields

        private const double CellHeight = 25;

        private const int MinimumLU_HeightForTopCells = 174;

        private const string ROTATION_CLASS_A = "A";

        private const string ROTATION_CLASS_B = "B";

        private const string ROTATION_CLASS_C = "C";

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

        private readonly IErrorsProvider errorsProvider;

        private readonly ILogger<DataLayerContext> logger;

        private readonly IMachineProvider machineProvider;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        #endregion

        #region Constructors

        public CellsProvider(DataLayerContext dataContext,
            IErrorsProvider errorsProvider,
            IElevatorDataProvider elevatorDataProvider,
            IMachineProvider machineProvider,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            ILogger<DataLayerContext> logger)
        {
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.machineProvider = machineProvider ?? throw new System.ArgumentNullException(nameof(machineProvider));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new System.ArgumentNullException(nameof(machineVolatileDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public bool CanFitLoadingUnit(int cellId, int loadingUnitId, bool isCellTest, out string reason)
        {
            var cell = this.GetById(cellId);
            reason = string.Empty;

            if (cell.LoadingUnit != null)
            {
                reason = Resources.Cells.ResourceManager.GetString("TheCellIsNotFree", CommonUtils.Culture.Actual);
                return false;
            }

            if (cell.BlockLevel != BlockLevel.None
                && (!isCellTest || cell.BlockLevel != BlockLevel.NeedsTest)
                )
            {
                reason = Resources.Cells.ResourceManager.GetString("TheTargetCellIsBlocked", CommonUtils.Culture.Actual);
                return false;
            }

            var loadUnit = this.dataContext.LoadingUnits
                .AsNoTracking()
                .Include(i => i.Cell)
                    .ThenInclude(c => c.Panel)
                .SingleOrDefault(l => l.Id == loadingUnitId);
            if (loadUnit is null)
            {
                throw new EntityNotFoundException(loadingUnitId);
            }

            var machine = this.machineProvider.GetMinMaxHeight();
            if (machine is null)
            {
                throw new EntityNotFoundException();
            }
            if (machine.LoadUnitMaxHeight == 0)
            {
                throw new InvalidOperationException(Resources.Bays.ResourceManager.GetString("TheBayLoadingMaxHeightNotValid", CommonUtils.Culture.Actual));
            }

            double loadUnitHeight;
            if (loadUnit.Height == 0 || loadUnit.Height > machine.LoadUnitMaxHeight)
            {
                loadUnitHeight = machine.LoadUnitMaxHeight;
                this.logger.LogInformation($"CanFitLoadingUnit: height is not defined for LU {loadingUnitId}; height is {loadUnitHeight} (as configured for max);");
            }
            else
            {
                loadUnitHeight = loadUnit.Height;
            }

            var cellsInRange = this.dataContext.Cells.Where(c => c.Panel.Side == cell.Side
                    && (c.Position >= cell.Position - (loadUnit.IsVeryHeavy(machine.LoadUnitVeryHeavyPercent) ? CellHeight : 0))
                    && c.Position <= cell.Position + loadUnitHeight + VerticalPositionTolerance)
                .ToList();
            if (!cellsInRange.Any())
            {
                reason = Resources.Cells.ResourceManager.GetString("TheSpecifiedCellCannotFitTheLoadUnit", CommonUtils.Culture.Actual);
                return false;
            }

            var availableSpace = cellsInRange.Last().Position - cellsInRange.First().Position + CellHeight;
            if (availableSpace < loadUnitHeight)
            {
                reason = Resources.Cells.ResourceManager.GetString("TheSpecifiedCellCannotFitTheLoadUnit", CommonUtils.Culture.Actual);
                return false;
            }

            if (loadUnit.IsCellFixed
                && loadUnit.FixedCell.HasValue
                && loadUnit.FixedCell.Value != cell.Id
                && !cellsInRange.Any(c => !c.IsFree
                    || c.IsNotAvailableFixed
                    || (!isCellTest && c.BlockLevel == BlockLevel.NeedsTest)
                    ))
            {
                reason = Resources.Cells.ResourceManager.GetString("TheLoadUnitHasFixedCell", CommonUtils.Culture.Actual);
                return false;
            }

            reason = Resources.Elevator.ResourceManager.GetString("TheLoadingUnitDoesNotFitInTheSpecifiedCell", CommonUtils.Culture.Actual);
            if (loadUnit.Cell != null)
            {
                // in cell-to-cell movements we check only the cells not presently occupied by this load unit
                var cellsFrom = this.dataContext.Cells.Where(c => c.Panel.Side == loadUnit.Cell.Side
                    && c.Position >= loadUnit.Cell.Position
                    && c.Position <= loadUnit.Cell.Position + loadUnitHeight + VerticalPositionTolerance)
                    .ToList();
                var lastPosition = cellsFrom.LastOrDefault().Position;
                return !cellsInRange.Any(c => (!c.IsFree && c.Position < loadUnit.Cell.Position - (loadUnit.IsVeryHeavy(machine.LoadUnitVeryHeavyPercent) ? CellHeight : 0))
                    || (!c.IsFree && c.Position > lastPosition)
                    || c.BlockLevel == BlockLevel.Blocked
                    || (!isCellTest && c.BlockLevel == BlockLevel.NeedsTest)
                    );
            }

            return !cellsInRange.Any(c => !c.IsFree
                || c.IsNotAvailable
                || (!isCellTest && c.BlockLevel == BlockLevel.NeedsTest)
                );
        }

        public int CleanUnderWeightCells()
        {
            lock (this.dataContext)
            {
                var machine = this.machineProvider.GetMinMaxHeight();
                if (machine is null)
                {
                    throw new EntityNotFoundException();
                }

                if (machine.LoadUnitVeryHeavyPercent == 0)
                {
                    var cells = this.GetAll(x => x.BlockLevel == BlockLevel.UnderWeight || x.BlockLevel == BlockLevel.NeedsTest)
                        .ToArray();

                    if (cells.Any())
                    {
                        for (var cellId = 0; cellId < cells.Length; cellId++)
                        {
                            cells[cellId].BlockLevel = BlockLevel.None;
                            cells[cellId].IsFree = true;
                            this.dataContext.Cells.Update(cells[cellId]);
                        }
                        this.dataContext.SaveChanges();
                        return cells.Length;
                    }
                }
                return 0;
            }
        }

        public int FindDownCell(LoadingUnit loadingUnit)
        {
            if (loadingUnit.Cell is null)
            {
                this.logger.LogError($"FindDownCell for compacting: LU {loadingUnit.Id} not in cell! ");
                throw new EntityNotFoundException();
            }
            var machine = this.machineProvider.GetMinMaxHeight();
            if (machine is null)
            {
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
            var cellId = -1;
            foreach (var cell in cells)
            {
                if (loadingUnit.IsVeryHeavy(machine.LoadUnitVeryHeavyPercent))
                {
                    var prev = cells.FirstOrDefault(c => c.Position < cell.Position);
                    if (prev is null
                        || prev.IsNotAvailable
                        || prev.BlockLevel == BlockLevel.NeedsTest
                        || cell.BlockLevel == BlockLevel.SpaceOnly
                        || !prev.IsFree
                        )
                    {
                        break;
                    }
                }
                else if (cell.IsNotAvailable
                    || cell.BlockLevel == BlockLevel.NeedsTest
                    || cell.BlockLevel == BlockLevel.SpaceOnly
                    || !cell.IsFree
                    || (this.machineVolatileDataProvider.IsOptimizeRotationClass
                        && !string.IsNullOrEmpty(cell.RotationClass)
                        && !string.IsNullOrEmpty(loadingUnit.RotationClass)
                        && cell.RotationClass[0] < loadingUnit.RotationClass[0])
                    )
                {
                    break;
                }
                cellId = cell.Id;
            }
            if (cellId < 0)
            {
                throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("NoEmptyCellsAvailable", CommonUtils.Culture.Actual));
            }
            // load units in space only positions are not moved by compacting down
            cells = this.GetAll(x => x.Position >= verticalAxis.LowerBound
                         && x.Side == loadingUnit.Cell.Side)
                .OrderBy(o => o.Position)
                .ToList();
            if (cells.Any(c => c.Side == loadingUnit.Cell.Side && c.Position > loadingUnit.Cell.Position && c.Position < loadingUnit.Cell.Position + loadingUnit.Height && c.BlockLevel == BlockLevel.SpaceOnly)
                && cells.Count(c => c.IsFree) < cells.Count * 0.4
            )
            {
                this.logger.LogError($"FindDownCell: do not move LU {loadingUnit.Id} from space only positions; Height {loadingUnit.Height:0.00}; total cells {cells.Count}; ");
                throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("NoEmptyCellsAvailable", CommonUtils.Culture.Actual));
            }
            this.logger.LogInformation($"FindDownCell: found Cell {cellId} for LU {loadingUnit.Id}; from cell {loadingUnit.Cell.Id}");
            return cellId;
        }

        public int FindEmptyCell(int loadingUnitId, CompactingType compactingType = CompactingType.NoCompacting, bool isCellTest = false, bool randomCells = false)
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
            var machineStatistics = this.machineProvider.GetPresentStatistics();
            if (machineStatistics is null)
            {
                throw new EntityNotFoundException();
            }
            var machine = this.machineProvider.GetMinMaxHeight();
            if (machine is null)
            {
                throw new EntityNotFoundException();
            }
            if (!machine.IsRotationClass && this.machineVolatileDataProvider.IsOptimizeRotationClass)
            {
                this.machineVolatileDataProvider.IsOptimizeRotationClass = false;
            }
            if (machineStatistics.TotalWeightFront + machineStatistics.TotalWeightBack + loadUnit.GrossWeight > machine.MaxGrossWeight)
            {
                this.logger.LogError($"FindEmptyCell: total weight exceeded for LU {loadingUnitId}; weight {loadUnit.GrossWeight:0.00}; " +
                    $"TotalWeightFront {machineStatistics.TotalWeightFront}; " +
                    $"TotalWeightBack {machineStatistics.TotalWeightBack}; " +
                    $"MaxGrossWeight {machine.MaxGrossWeight} ");
                throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("NoEmptyCellsAvailable", CommonUtils.Culture.Actual));
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
            if (loadUnit.Height == 0 || loadUnit.Height > machine.LoadUnitMaxHeight)
            {
                if (machine.LoadUnitMaxHeight == 0)
                {
                    throw new InvalidOperationException(Resources.Bays.ResourceManager.GetString("TheBayLoadingMaxHeightNotValid", CommonUtils.Culture.Actual));
                }
                loadUnitHeight = (isCellTest ? machine.LoadUnitMinHeight : machine.LoadUnitMaxHeight);
                this.logger.LogInformation($"FindEmptyCell: height is not defined for LU {loadingUnitId}; height is {loadUnitHeight} (as configured for {(isCellTest ? "min" : "max")});");
            }
            else
            {
                loadUnitHeight = loadUnit.Height;
            }

            if (!isCellTest
                && loadUnit.IsCellFixed
                && loadUnit.FixedCell.HasValue)
            {
                return this.FindEmptyFixedCell(loadUnit, machine, loadUnitHeight);
            }

            if (loadUnit.IsVeryHeavy(machine.LoadUnitVeryHeavyPercent))
            {
                loadUnitHeight += CellHeight;
                if (compactingType != CompactingType.NoCompacting)
                {
                    this.logger.LogDebug($"FindEmptyCell: height extended to {loadUnitHeight} for heavy LU {loadingUnitId}, weight {loadUnit.NetWeight}");
                }
            }

            using (var availableCell = new BlockingCollection<AvailableCell>())
            {
                var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);

                // load all cells
                var cells = this.GetAll(x => x.Position >= verticalAxis.LowerBound
                             && (compactingType == CompactingType.NoCompacting || compactingType == CompactingType.RotationCompacting || x.Side == loadUnit.Cell.Side))
                    .OrderBy(o => o.Position)
                    .ToList();

                var freeCells = cells.Count(c => c.IsFree);
                // load units in space only positions are not moved by compacting
                if ((compactingType != CompactingType.NoCompacting && compactingType != CompactingType.RotationCompacting)
                    //&& loadUnitHeight > MinimumLU_HeightForTopCells
                    && loadUnit.NetWeight < machine.LoadUnitMaxNetWeight * 0.6
                    && freeCells < cells.Count * 0.4
                    && cells.Any(c => c.Side == loadUnit.Cell.Side && c.Position > loadUnit.Cell.Position && c.Position < loadUnit.Cell.Position + loadUnitHeight && c.BlockLevel == BlockLevel.SpaceOnly)
                    )
                {
                    this.logger.LogTrace($"FindEmptyCell: do not move LU {loadingUnitId} from space only positions; Height {loadUnitHeight:0.00}; total cells {cells.Count}; ");
                    throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("NoEmptyCellsAvailable", CommonUtils.Culture.Actual));
                }

                // for each available cell we check if there is space for the requested height
                Parallel.ForEach(cells.Where(c => c.IsFree
                    && (isCellTest ? c.BlockLevel == BlockLevel.NeedsTest : c.BlockLevel == BlockLevel.None)
                    && (compactingType == CompactingType.NoCompacting || compactingType == CompactingType.RotationCompacting || c.Position < loadUnit.Cell.Position)
                    ), (cell) =>
                {
                    // load 1 meter of cells following the selected cell
                    var cellsFollowing = cells.Where(c => c.Panel.Side == cell.Side
                        && c.Position >= cell.Position
                        && c.Position < cell.Position + 1500);

                    // load previous cell
                    var prev = cells.LastOrDefault(c => c.Side == cell.Side
                        && c.Position < cell.Position);

                    // don't want floating cells: previous cell is free and available
                    var isFloating = (prev != null && prev.IsFree && prev.BlockLevel == BlockLevel.None && !randomCells);

                    // SpaceOnly cells can be occupied by high load units
                    if (isFloating
                        && (compactingType == CompactingType.NoCompacting || compactingType == CompactingType.RotationCompacting)
                        && loadUnitHeight >= MinimumLU_HeightForTopCells
                        && loadUnit.NetWeight < machine.LoadUnitMaxNetWeight * 0.6
                        && freeCells < cells.Count * 0.4
                        && cellsFollowing.Any(c => c.IsFree && c.Position > cell.Position && c.Position <= cell.Position + loadUnitHeight + CellHeight && c.BlockLevel == BlockLevel.SpaceOnly)
                        )
                    {
                        isFloating = false;
                    }

                    if (cellsFollowing.Any()
                        && (!isFloating || isCellTest)
                        && (!this.machineVolatileDataProvider.IsOptimizeRotationClass
                            || compactingType == CompactingType.NoCompacting
                            || compactingType == CompactingType.RotationCompacting
                            || cell.RotationClass == loadUnit.RotationClass))
                    {
                        // measure available space
                        var lastCellPosition = cellsFollowing.Last().Position;
                        if (cellsFollowing.Count() > 1)
                        {
                            var firstUnavailable = cellsFollowing.FirstOrDefault(c => !c.IsFree || c.IsNotAvailable || (c.BlockLevel == BlockLevel.NeedsTest && c.Position > cell.Position));
                            if (firstUnavailable != null)
                            {
                                lastCellPosition = cellsFollowing.LastOrDefault(c => c.Position < firstUnavailable.Position)?.Position ?? lastCellPosition;
                            }
                        }
                        var availableSpace = lastCellPosition - cell.Position + CellHeight;
                        var firstFree = true;
                        if (compactingType == CompactingType.ExactMatchCompacting || compactingType == CompactingType.AnySpaceCompacting)
                        {
                            // in these compacting types the cell must be the first empty cell
                            if (cells.Any(c => c.Panel.Side == cell.Side
                                && c.Position < cell.Position
                                && c.IsFree
                                && c.BlockLevel == BlockLevel.None)
                            )
                            {
                                firstFree = false;
                            }
                        }
                        // check if load unit fits in available space
                        if (availableSpace >= loadUnitHeight + VerticalPositionTolerance
                            && (compactingType != CompactingType.AnySpaceCompacting
                                || firstFree
                                )
                            && (compactingType != CompactingType.ExactMatchCompacting
                                || (availableSpace < loadUnitHeight + (4 * VerticalPositionTolerance)
                                    && firstFree
                                    )
                                )
                            )
                        {
                            if (compactingType != CompactingType.RotationCompacting
                                || string.IsNullOrEmpty(cell.RotationClass)
                                || string.IsNullOrEmpty(loadUnit.Cell?.RotationClass)
                                || loadUnit.Cell.RotationClass[0] < cell.RotationClass[0]
                                )
                            {
                                availableCell.Add(new AvailableCell(cell, availableSpace));
                            }
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
                    throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("NoEmptyCellsAvailable", CommonUtils.Culture.Actual));
                }

                var foundCell = default(AvailableCell);

                if (!randomCells)
                {
                    // sort cells from bottom to top, optimizing free space
                    foundCell = availableCell.OrderBy(o => (preferredSide != WarehouseSide.NotSpecified && o.Cell.Side == preferredSide) ? 0 : 1)
                        .ThenBy(t => (machine.IsRotationClass && !string.IsNullOrEmpty(t.Cell.RotationClass) && !string.IsNullOrEmpty(loadUnit.RotationClass)) ? Math.Abs(t.Cell.RotationClass[0] - loadUnit.RotationClass[0]) : 0)
                        .ThenBy(t => (isCellTest) ? 0 : t.Height)          // minimize free space
                        .ThenBy(t => t.Cell.Priority)   // start from bottom to top
                        .First();
                }
                else
                {
                    var index = new Random().Next(availableCell.Count);
                    foundCell = availableCell.ElementAt(index);
                }

                var cellId = foundCell.Cell.Id;
                if (loadUnit.IsVeryHeavy(machine.LoadUnitVeryHeavyPercent))
                {
                    // return second cell for heavy LU
                    cellId = cells.First(c => c.Side == foundCell.Cell.Side && c.Position > foundCell.Cell.Position).Id;
                }

                this.logger.LogInformation($"FindEmptyCell: found Cell {cellId} for LU {loadingUnitId} {loadUnit.RotationClass}; " +
                    $"Height {loadUnitHeight:0.00}; " +
                    $"Weight {loadUnit.GrossWeight:0.00}; " +
                    $"preferredSide {preferredSide}; " +
                    $"{compactingType}; rotation {this.machineVolatileDataProvider.IsOptimizeRotationClass}; " +
                    $"total cells {cells.Count}; " +
                    $"available cells {availableCell.Count}; " +
                    $"available space {foundCell.Height}; " +
                    $"random cells {randomCells}; " +
                    $"free cells {freeCells}; " +
                    $"TotalWeightFront {machineStatistics.TotalWeightFront:0.00}; " +
                    $"TotalWeightBack {machineStatistics.TotalWeightBack:0.00}");
                return cellId;
            }
        }

        public int FindTopCell(LoadingUnit loadingUnit)
        {
            if (loadingUnit.Cell is null)
            {
                this.logger.LogError($"FindTopCell for compacting: LU {loadingUnit.Id} not in cell! ");
                throw new EntityNotFoundException();
            }
            //if (loadingUnit.Height <= MinimumLU_HeightForTopCells)
            //{
            //    throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("NoEmptyCellsAvailable", CommonUtils.Culture.Actual));
            //}
            var machine = this.machineProvider.GetMinMaxHeight();
            if (machine is null)
            {
                throw new EntityNotFoundException();
            }
            if (loadingUnit.NetWeight >= machine.LoadUnitMaxNetWeight * 0.6)
            {
                throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("NoEmptyCellsAvailable", CommonUtils.Culture.Actual));
            }
            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);

            // load all top cells
            var cells = this.GetAll(x => x.Position >= verticalAxis.UpperBound - 1000
                         && x.Side == loadingUnit.Cell.Side)
                .OrderByDescending(o => o.Position)
                .ToList();
            var lastCellPosition = cells.First().Position;
            foreach (var cell in cells.Where(c => c.BlockLevel == BlockLevel.None
                    && c.IsFree))
            {
                if (!machine.IsRotationClass
                    || !string.IsNullOrEmpty(cell.RotationClass)
                    || !string.IsNullOrEmpty(loadingUnit.RotationClass)
                    || cell.RotationClass[0] >= loadingUnit.RotationClass[0])
                {
                    if (cells.Any(c => c.Position > cell.Position
                        && c.Position < cell.Position + loadingUnit.Height + CellHeight
                        && c.BlockLevel == BlockLevel.SpaceOnly)
                        )
                    {
                        // measure available space
                        var cellsFollowing = cells.Where(c => c.Position >= cell.Position).OrderBy(o => o.Position);
                        if (cellsFollowing.Any())
                        {
                            var firstUnavailable = cellsFollowing.FirstOrDefault(c => !c.IsFree || c.IsNotAvailable || (c.BlockLevel == BlockLevel.NeedsTest && c.Position > cell.Position));
                            if (firstUnavailable != null)
                            {
                                lastCellPosition = cellsFollowing.LastOrDefault(c => c.Position < firstUnavailable.Position)?.Position ?? lastCellPosition;
                            }
                        }
                        var availableSpace = lastCellPosition - cell.Position + CellHeight;
                        if (availableSpace >= loadingUnit.Height + VerticalPositionTolerance)
                        {
                            this.logger.LogInformation($"FindTopCell: found Cell {cell.Id} for LU {loadingUnit.Id}; from cell {loadingUnit.Cell.Id}");
                            return cell.Id;
                        }
                    }
                }
            }
            throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("NoEmptyCellsAvailable", CommonUtils.Culture.Actual));
        }

        public void FreeReservedCells(LoadingUnit luDb)
        {
            if (luDb.FixedCell is null)
            {
                return;
            }
            lock (this.dataContext)
            {
                var cell = this.dataContext.Cells.Include(i => i.Panel).FirstOrDefault(c => c.Id == luDb.FixedCell);
                if (cell != null)
                {
                    var machine = this.machineProvider.GetMinMaxHeight();
                    var cells = this.dataContext.Cells.Where(c => c.BlockLevel == BlockLevel.Reserved
                                && c.Panel.Side == cell.Side
                                && (c.Position >= cell.Position - (luDb.IsVeryHeavy(machine.LoadUnitVeryHeavyPercent) ? CellHeight : 0))
                                && c.Position <= cell.Position + luDb.FixedHeight + VerticalPositionTolerance)
                                .ToList();
                    if (cells.Count > 0)
                    {
                        cells.ForEach(cell => cell.BlockLevel = BlockLevel.None);
                        this.dataContext.SaveChanges();
                    }
                }
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

                var occupiedOrUnusableCellsCount = cellsWithSide.Count(c => !c.IsFree || c.IsNotAvailable);

                var cellStatistics = new CellStatisticsSummary()
                {
                    CellStatusStatistics = cellStatusStatistics,
                    TotalCells = totalCells,
                    TotalFrontCells = cellsWithSide.Count(c => c.Side == WarehouseSide.Front),
                    TotalBackCells = cellsWithSide.Count(c => c.Side == WarehouseSide.Front),
                    CellOccupationPercentage = 100.0 * occupiedOrUnusableCellsCount / totalCells,
                    MaxSolidSpace = this.FindMaxSolidSpace(cellsWithSide)
                };
                var machine = this.machineProvider.GetMinMaxHeight();
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

        public bool IsTopCellAvailable(WarehouseSide side)
        {
            var cells = this.GetAll(x => x.Side == side)
                .OrderByDescending(o => o.Position)
                .ToList();
            return cells.Count(c => c.IsFree) < cells.Count * 0.4
                && (cells.FirstOrDefault(x => x.BlockLevel != BlockLevel.SpaceOnly)?.IsFree ?? false);
        }

        public void Save(Cell cell)
        {
            lock (this.dataContext)
            {
                this.dataContext.AddOrUpdate(cell, f => f.Id);
                this.dataContext.SaveChanges();
            }
        }

        public void SaveCells(IEnumerable<Cell> cells)
        {
            lock (this.dataContext)
            {
                this.dataContext.Cells.UpdateRange(cells);
                this.logger.LogDebug($"Change back drawers position. {cells.Count()} cells updated");
                this.dataContext.SaveChanges();
            }
        }

        public int SetCellsToTest(BayNumber bayNumber, double height)
        {
            lock (this.dataContext)
            {
                var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);

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
                for (var iCell = 0; iCell < cells.Length; iCell++)
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
                            if (cell.Position > verticalAxis.UpperBound)
                            {
                                this.errorsProvider.RecordNew(MachineErrorCode.DestinationOverUpperBound, bayNumber);
                                count = 0;
                                break;
                            }
                            else if (cell.Position < verticalAxis.LowerBound)
                            {
                                this.errorsProvider.RecordNew(MachineErrorCode.DestinationBelowLowerBound, bayNumber);
                                count = 0;
                                break;
                            }
                            else
                            {
                                cell = this.FindLowerCellByHeight(cells, iCell, height);
                                if (cell != null)
                                {
                                    cell.BlockLevel = BlockLevel.NeedsTest;
                                    this.dataContext.Cells.Update(cell);

                                    count++;
                                }
                            }
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
                                if (cell.Position > verticalAxis.UpperBound)
                                {
                                    this.errorsProvider.RecordNew(MachineErrorCode.DestinationOverUpperBound, bayNumber);
                                    count = 0;
                                    break;
                                }
                                else if (cell.Position < verticalAxis.LowerBound)
                                {
                                    this.errorsProvider.RecordNew(MachineErrorCode.DestinationBelowLowerBound, bayNumber);
                                    count = 0;
                                    break;
                                }
                                else
                                {
                                    cell = this.FindLowerCellByHeight(cells, iCell, height);
                                    if (cell != null)
                                    {
                                        cell.BlockLevel = BlockLevel.NeedsTest;
                                        this.dataContext.Cells.Update(cell);

                                        count++;
                                    }
                                }
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
                var machine = this.machineProvider.GetMinMaxHeight();
                if (machine is null)
                {
                    throw new EntityNotFoundException();
                }

                var statistics = this.dataContext.MachineStatistics.ToArray().LastOrDefault();

                if (loadingUnitId is null)
                {
                    // set occupied cells to free
                    if (cell.LoadingUnit is null)
                    {
                        return;
                    }
                    var loadingUnit = cell.LoadingUnit;
                    //loadingUnit.IsIntoMachine = false;

                    var occupiedCells = this.dataContext.Cells
                        .Include(c => c.LoadingUnit)
                        .Include(c => c.Panel)
                        .Where(c =>
                            c.Panel.Side == cell.Side
                            &&
                            (c.Position >= cell.Position - (loadingUnit.IsVeryHeavy(machine.LoadUnitVeryHeavyPercent) ? CellHeight : 0))
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

                    foreach (var occupiedCell in occupiedCells.Where(c => c.Position >= cell.Position || c.BlockLevel == BlockLevel.UnderWeight))
                    {
                        if (occupiedCell.LoadingUnit != null
                            && occupiedCell.LoadingUnit.Id != loadingUnit.Id
                            )
                        {
                            throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("TheCellUnexpectedlyContainsAnotherLoadingUnit", CommonUtils.Culture.Actual));
                        }

                        if (occupiedCell.IsFree)
                        {
                            throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("TheCellIsUnexpectedlyFree", CommonUtils.Culture.Actual));
                        }

                        occupiedCell.IsFree = true;
                        occupiedCell.LoadingUnit = null;
                        if (occupiedCell.BlockLevel == BlockLevel.UnderWeight)
                        {
                            occupiedCell.BlockLevel = BlockLevel.None;
                        }

                        if (loadingUnit.IsCellFixed
                            && occupiedCell.BlockLevel == BlockLevel.None)
                        {
                            occupiedCell.BlockLevel = BlockLevel.Reserved;
                        }
                        else if (!loadingUnit.IsCellFixed
                            && occupiedCell.BlockLevel == BlockLevel.Reserved)
                        {
                            occupiedCell.BlockLevel = BlockLevel.None;
                        }
                    }
                }
                else
                {
                    // set free cells to occupied
                    if (cell.BlockLevel == BlockLevel.SpaceOnly)
                    {
                        throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("TheTargetCellIsSpaceOnly", CommonUtils.Culture.Actual));
                    }

                    if (cell.BlockLevel == BlockLevel.Blocked)
                    {
                        throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("TheTargetCellIsBlocked", CommonUtils.Culture.Actual));
                    }

                    if (cell.LoadingUnit != null)
                    {
                        throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("TheCellAlreadyContainsAnotherLoadingUnit", CommonUtils.Culture.Actual));
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
                        throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("TheLoadingUnitIsAlreadyLocatedInAnotherCell", CommonUtils.Culture.Actual));
                    }

                    var freeCells = this.dataContext.Cells
                       .Include(c => c.LoadingUnit)
                       .Include(c => c.Panel)
                       .Where(c =>
                           c.Panel.Side == cell.Side
                           &&
                           (c.Position >= cell.Position - (loadingUnit.IsVeryHeavy(machine.LoadUnitVeryHeavyPercent) ? CellHeight : 0))
                           &&
                           c.Position <= cell.Position + loadingUnit.Height + VerticalPositionTolerance)
                       .ToArray();

                    foreach (var freeCell in freeCells)
                    {
                        freeCell.IsFree = false;
                        if (freeCell.LoadingUnit != null)
                        {
                            throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("TheCellUnexpectedlyContainsAnotherLoadingUnit", CommonUtils.Culture.Actual));
                        }

                        if (freeCell.BlockLevel == BlockLevel.Blocked)
                        {
                            throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("TheLoadingCannotOccupyABlockedCell", CommonUtils.Culture.Actual));
                        }
                        if (freeCell.Position < cell.Position)
                        {
                            freeCell.BlockLevel = BlockLevel.UnderWeight;
                        }
                    }

                    loadingUnit.Status = LoadingUnitStatus.InLocation;
                    //this.dataContext.SaveChanges();
                    //loadingUnit = this.dataContext.LoadingUnits
                    //    .SingleOrDefault(l => l.Id == loadingUnitId);

                    cell.LoadingUnit = loadingUnit;
                    loadingUnit.Cell = cell;

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
                if (machine.MaxGrossWeight != 0)
                {
                    statistics.WeightCapacityPercentage = ((statistics.TotalWeightFront + statistics.TotalWeightBack) / machine.MaxGrossWeight) * 100;
                }
                this.dataContext.SaveChanges();

                // TODO - THIS IS A GOOD POINT TO MAKE A DATABASE BACKUP
            }
        }

        public void SetRotationClass()
        {
            lock (this.dataContext)
            {
                var loadUnits = this.dataContext
                    .LoadingUnits
                    .Where(l => l.Status != LoadingUnitStatus.Blocked && !string.IsNullOrEmpty(l.RotationClass));

                var cellsOccupied_A = (int)loadUnits.Sum(l => Math.Round(l.RotationClass == ROTATION_CLASS_A ? (l.Height / CellHeight) + 1 : 0));
                var bay = this.dataContext.Bays
                    .Include(i => i.Positions)
                    .Include(i => i.Carousel)
                    .FirstOrDefault(b => b.RotationClass == ROTATION_CLASS_A);
                var position = bay?.Positions.FirstOrDefault(p => p.IsPreferred is true);

                if (cellsOccupied_A > 0 && position != null)
                {
                    var cells = this.dataContext.Cells.ToArray();

                    var cellsOccupied_B = (int)loadUnits.Sum(l => Math.Round(l.RotationClass == ROTATION_CLASS_B ? (l.Height / CellHeight) + 1 : 0));
                    var cellsCount = 0;
                    foreach (var cell in cells.Where(c => !c.IsNotAvailable).OrderBy(o => Math.Abs(o.Position - position.Height)))
                    {
                        if (cellsCount <= cellsOccupied_A)
                        {
                            cell.RotationClass = ROTATION_CLASS_A;
                        }
                        else if (cellsCount <= cellsOccupied_A + cellsOccupied_B)
                        {
                            cell.RotationClass = ROTATION_CLASS_B;
                        }
                        else
                        {
                            cell.RotationClass = ROTATION_CLASS_C;
                        }
                        cellsCount++;
                    }

                    this.dataContext.SaveChanges();
                }
            }
        }

        public void SetRotationClassFromUI(int cellId, string rotationClass)
        {
            lock (this.dataContext)
            {
                var cell = this.dataContext
                    .Cells
                    .SingleOrDefault(l => l.Id == cellId);

                cell.RotationClass = rotationClass;

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
                        Resources.Cells.ResourceManager.GetString("TheSpecifiedHeightIsNotBetweenTheAdjacentCellsHeights", CommonUtils.Culture.Actual));
                }

                return this.dataContext.Cells
                    .Include(c => c.Panel)
                    .SingleOrDefault(c => c.Id == cellId);
            }
        }

        private int FindEmptyFixedCell(LoadingUnit loadUnit, Machine machine, double loadUnitHeight)
        {
            var cell = this.GetById(loadUnit.FixedCell.Value);
            var cellsInRange = this.dataContext.Cells.Where(c => c.Panel.Side == cell.Side
                    && (c.Position >= cell.Position - (loadUnit.IsVeryHeavy(machine.LoadUnitVeryHeavyPercent) ? CellHeight : 0))
                    && c.Position <= cell.Position + loadUnitHeight + VerticalPositionTolerance)
                .ToList();
            if (!cellsInRange.Any())
            {
                throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("NoEmptyCellsAvailable", CommonUtils.Culture.Actual));
            }

            var availableSpace = cellsInRange.Last().Position - cellsInRange.First().Position + CellHeight;
            if (availableSpace < loadUnitHeight)
            {
                throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("NoEmptyCellsAvailable", CommonUtils.Culture.Actual));
            }

            if (loadUnit.IsCellFixed
                && loadUnit.FixedCell.HasValue
                && loadUnit.FixedCell.Value != cell.Id
                && !cellsInRange.Any(c => !c.IsFree
                    || c.IsNotAvailableFixed
                    || (c.BlockLevel == BlockLevel.NeedsTest)
                    ))
            {
                throw new InvalidOperationException(Resources.Cells.ResourceManager.GetString("NoEmptyCellsAvailable", CommonUtils.Culture.Actual));
            }
            return loadUnit.FixedCell.Value;
        }

        private Cell FindLowerCellByHeight(Cell[] cells, int iCell, double height)
        {
            if (cells[iCell + 1].BlockLevel == BlockLevel.Blocked || cells[iCell + 1].BlockLevel == BlockLevel.SpaceOnly)
            {
                var topSpace = cells[iCell + 1].Position;
                for (var iTop = 2; iCell + iTop < cells.Length && cells[iCell + iTop].BlockLevel != BlockLevel.Blocked && cells[iCell + iTop].Side == cells[iCell].Side; iTop++)
                {
                    topSpace = cells[iCell + iTop].Position + CellHeight;
                }

                for (var iFind = iCell; iFind > 0 && cells[iFind].IsAvailable; iFind--)
                {
                    if (cells[iFind].BlockLevel == BlockLevel.None
                        && topSpace - cells[iFind].Position > height + CellHeight)
                    {
                        return cells[iFind];
                    }
                }
                return null;
            }
            return cells[iCell];
        }

        private Dictionary<WarehouseSide, double> FindMaxSolidSpace(Cell[] cellsWithSide)
        {
            var maxSolidSpace = new Dictionary<WarehouseSide, double>() { { WarehouseSide.Back, 0 }, { WarehouseSide.Front, 0 } };
            var startCell = new Dictionary<WarehouseSide, double>() { { WarehouseSide.Back, 0 }, { WarehouseSide.Front, 0 } };
            var prevCell = new Cell();
            foreach (var cell in cellsWithSide.OrderBy(c => c.Side).ThenBy(d => d.Position))
            {
                if (cell.IsFree && cell.BlockLevel == BlockLevel.None
                    && startCell[cell.Side] == 0
                    )
                {
                    startCell[cell.Side] = cell.Position;
                }
                else if (prevCell.Side != WarehouseSide.NotSpecified && startCell[prevCell.Side] != 0)
                {
                    maxSolidSpace[prevCell.Side] = Math.Max(maxSolidSpace[prevCell.Side], (prevCell.Position - startCell[prevCell.Side] + 25));
                    if (cell.IsNotAvailable || !cell.IsFree)
                    {
                        startCell[cell.Side] = 0;
                    }
                }
                prevCell = cell;
            }
            return maxSolidSpace;
        }

        private int FreeBlocks(Cell[] cellsWithSide, WarehouseSide side, out double freeCells)
        {
            var count = 0;
            freeCells = 0;
            var cellsBySide = cellsWithSide.Where(c => c.Side == side)
                .OrderBy(o => o.Position)
                .ToArray();

            for (var i = 0; i < cellsBySide.Length; i++)
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
            freeCells = cellsBySide.Count(c => c.IsAvailable);
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

using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly DataLayerContext dataContext;

        private readonly ILogger<CellsProvider> logger;

        #endregion

        #region Constructors

        public CellsProvider(DataLayerContext dataContext, ILogger<CellsProvider> logger)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
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
                    c.Position >= cell.Position);

            return !cellsInRange.Any(c => c.Status == CellStatus.Occupied || c.IsUnusable);
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
            var cell = this.dataContext.Cells
                .Include(c => c.LoadingUnit)
                .Include(c => c.Panel)
                .SingleOrDefault(c => c.Id == cellId);

            if (cell is null)
            {
                throw new EntityNotFoundException(cellId);
            }

            if (loadingUnitId is null)
            {
                if (cell.LoadingUnit is null)
                {
                    return;
                }

                var occupiedCells = this.dataContext.Cells
                    .Include(c => c.LoadingUnit)
                    .Where(c =>
                        c.Side == cell.Side
                        &&
                        c.Position >= cell.Position
                        &&
                        c.Position <= cell.Position + cell.LoadingUnit.Height)
                    .ToArray();

                foreach (var occupiedCell in occupiedCells)
                {
                    if (occupiedCell.LoadingUnit != null && occupiedCell.LoadingUnit.Id != cell.LoadingUnit.Id)
                    {
                        throw new InvalidOperationException(Resources.Cells.TheCellUnexpectedlyContainsAnotherLoadingUnit);
                    }

                    if (occupiedCell.Status != CellStatus.Occupied)
                    {
                        throw new InvalidOperationException(Resources.Cells.TheCellIsUnexpectedlyFree);
                    }

                    occupiedCell.Status = CellStatus.Free;
                    occupiedCell.LoadingUnit = null;
                }
            }
            else
            {
                if (cell.IsDeactivated || cell.IsUnusable)
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

                var loadingUnit = this.dataContext.LoadingUnits.SingleOrDefault(l => l.Id == loadingUnitId);
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
                       c.Position <= cell.Position + loadingUnit.Height)
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

                cell.LoadingUnit = loadingUnit;
            }

            this.dataContext.SaveChanges();
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

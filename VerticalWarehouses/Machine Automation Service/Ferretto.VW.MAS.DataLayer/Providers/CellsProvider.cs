using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            if (dataContext is null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.dataContext = dataContext;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public IEnumerable<Cell> GetAll()
        {
            return this.dataContext.Cells
                .Include(c => c.Panel)
                .ToArray();
        }

        public Cell GetCellById(int cellId)
        {
            return this.dataContext.Cells
                .Include(c => c.Panel)
                .SingleOrDefault(c => c.Id == cellId);
        }

        public Cell GetCellByLoadingUnit(int loadingUnitId)
        {
            return this.dataContext.Cells
                .Include(c => c.LoadingUnit)
                .Include(c => c.Panel)
                .SingleOrDefault(c => c.LoadingUnit.Id == loadingUnitId);
        }

        public CellStatisticsSummary GetStatistics()
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
                .Count(c => c.Status == CellStatus.Occupied || c.Status == CellStatus.Unusable);

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

        public void LoadLoadingUnit(int loadingUnitId, int cellId)
        {
            throw new NotImplementedException();
        }

        public void UnloadLoadingUnit(int cellId)
        {
            throw new NotImplementedException();
        }

        public Cell UpdateHeight(int cellId, double height)
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
                higherCell.Position > height)
                &&
                (lowerCell == null
                ||
                lowerCell.Position < height))
            {
                cell.Position = height;

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

        #endregion
    }
}

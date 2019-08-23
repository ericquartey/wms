using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class CellsProvider : Interfaces.ICellsProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public CellsProvider(DataLayerContext dataContext)
        {
            if (dataContext == null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public IEnumerable<Cell> GetAll()
        {
            return this.dataContext.Cells.ToArray();
        }

        public CellStatisticsSummary GetStatistics()
        {
            var totalCells = this.dataContext.Cells.Count();
            var cellStatusStatistics = this.dataContext.Cells
                .GroupBy(c => c.Status)
                .Select(g =>
                    new CellStatusStatistics
                    {
                        Status = g.Key,
                        TotalFrontCells = g.Count(c => c.Side == CellSide.Front),
                        TotalBackCells = g.Count(c => c.Side == CellSide.Back),
                        RatioFrontCells = g.Count(c => c.Side == CellSide.Front) / (double)totalCells,
                        RatioBackCells = g.Count(c => c.Side == CellSide.Back) / (double)totalCells,
                    });

            var cellStatistics = new CellStatisticsSummary()
            {
                CellStatusStatistics = cellStatusStatistics,
                TotalCells = totalCells,
                TotalFrontCells = this.dataContext.Cells.Count(c => c.Side == CellSide.Front),
                TotalBackCells = this.dataContext.Cells.Count(c => c.Side == CellSide.Front),
                CellOccupationPercentage =
                    100 * this.dataContext.Cells.Count(c => (c.Status == CellStatus.Occupied || c.Status == CellStatus.Unusable))
                / (double)totalCells,
            };

            return cellStatistics;
        }

        #endregion
    }
}

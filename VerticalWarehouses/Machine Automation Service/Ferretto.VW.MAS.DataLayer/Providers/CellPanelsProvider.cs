using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class CellPanelsProvider : ICellPanelsProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public CellPanelsProvider(DataLayerContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        #endregion

        #region Methods

        public IEnumerable<CellPanel> GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.CellPanels
                .Include(p => p.Cells)
                .ToArray();
            }
        }

        public CellPanel UpdateHeight(int cellId, double newHeight)
        {
            lock (this.dataContext)
            {
                var cell = this.dataContext.Cells.SingleOrDefault(c => c.Id == cellId);
                if (cell is null)
                {
                    throw new EntityNotFoundException(cellId);
                }

                var cellPanel = this.dataContext.CellPanels
                    .Include(p => p.Cells)
                    .SingleOrDefault(p => p.Cells.Contains(cell));

                var heightDifference = newHeight - cell.Position;

                var highestPanelHeight = cellPanel?.Cells.Max(c => c.Position) + heightDifference;
                var lowestPanelHeight = cellPanel?.Cells.Min(c => c.Position) + heightDifference;

                var isOverlapping = this.dataContext.Cells
                    .Include(c => c.Panel)
                    .Any(c =>
                        c.Position < highestPanelHeight
                        &&
                        c.Position > lowestPanelHeight
                        &&
                        c.PanelId != cell.PanelId
                        &&
                        c.Side == cellPanel.Side);

                if (isOverlapping)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(newHeight),
                        Resources.Cells.TheSpecifiedHeightWouldCauseThePanelToOverlapWithOtherPanels);
                }

                if (cellPanel != null)
                {
                    foreach (var panelCell in cellPanel.Cells)
                    {
                        panelCell.Position += heightDifference;

                        this.dataContext.Cells.Update(panelCell);
                    }
                }

                this.dataContext.SaveChanges();

                return this.dataContext.CellPanels
                    .Include(p => p.Cells)
                    .SingleOrDefault(p => p.Cells.Contains(cell));
            }
        }

        #endregion
    }
}

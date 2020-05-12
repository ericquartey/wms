using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class CellPanelsProvider : ICellPanelsProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public CellPanelsProvider(DataLayerContext dataContext,
            ISetupProceduresDataProvider setupProceduresDataProvider)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
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

        public CellPanel UpdateHeight(int cellPanelId, double heightDifference)
        {
            lock (this.dataContext)
            {
                var cellPanel = this.dataContext.CellPanels
                    .Include(p => p.Cells)
                    .SingleOrDefault(c => c.Id == cellPanelId);
                if (cellPanel is null)
                {
                    throw new EntityNotFoundException(cellPanelId);
                }

                var highestPanelHeight = cellPanel?.Cells.Max(c => c.Position) + heightDifference;
                var lowestPanelHeight = cellPanel?.Cells.Min(c => c.Position) + heightDifference;

                var isOverlapping = this.dataContext.Cells
                    .Include(c => c.Panel)
                    .Any(c =>
                        c.Position < highestPanelHeight
                        &&
                        c.Position > lowestPanelHeight
                        &&
                        c.PanelId != cellPanelId
                        &&
                        c.Side == cellPanel.Side);

                if (isOverlapping)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(heightDifference),
                        Resources.Cells.TheSpecifiedHeightWouldCauseThePanelToOverlapWithOtherPanels);
                }

                if (cellPanel != null && heightDifference != 0)
                {
                    foreach (var panelCell in cellPanel.Cells)
                    {
                        panelCell.Position += heightDifference;

                        this.dataContext.Cells.Update(panelCell);
                    }

                    this.dataContext.SaveChanges();
                }

                cellPanel.IsChecked = true;
                this.dataContext.CellPanels.Update(cellPanel);

                this.dataContext.SaveChanges();

                if (!this.dataContext.CellPanels.Where(w => w.Cells.Any(a => a.BlockLevel.Equals(BlockLevel.None))).Any(c => !c.IsChecked))
                {
                    this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetCellPanelsCheck());
                }
                else
                {
                    this.setupProceduresDataProvider.InProgressProcedure(this.setupProceduresDataProvider.GetCellPanelsCheck());
                }

                return this.dataContext.CellPanels
                    .Include(p => p.Cells)
                    .SingleOrDefault(p => p.Id == cellPanelId);
            }
        }

        #endregion
    }
}

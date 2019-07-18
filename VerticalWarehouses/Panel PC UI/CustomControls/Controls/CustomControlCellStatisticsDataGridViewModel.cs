using System.Collections.Generic;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomControlCellStatisticsDataGridViewModel : BaseViewModel, ICustomControlCellStatisticsDataGridViewModel
    {
        #region Fields

        private IEnumerable<CellStatusStatistics> cells;

        private CellStatusStatistics selectedCell;

        #endregion

        #region Properties

        public IEnumerable<CellStatusStatistics> Cells { get => this.cells; set => this.SetProperty(ref this.cells, value); }

        public CellStatusStatistics SelectedCell { get => this.selectedCell; set => this.SetProperty(ref this.selectedCell, value); }

        #endregion
    }
}

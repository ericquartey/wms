using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomControlCellStatisticsDataGridViewModel : BaseViewModel, ICustomControlCellStatisticsDataGridViewModel
    {
        #region Fields

        private IEnumerable<CellStatusStatistic> cells;

        private CellStatusStatistic selectedCell;

        #endregion

        #region Properties

        public IEnumerable<CellStatusStatistic> Cells { get => this.cells; set => this.SetProperty(ref this.cells, value); }

        public CellStatusStatistic SelectedCell { get => this.selectedCell; set => this.SetProperty(ref this.selectedCell, value); }

        #endregion
    }
}

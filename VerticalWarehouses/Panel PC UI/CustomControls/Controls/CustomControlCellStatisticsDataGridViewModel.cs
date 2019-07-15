using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomControlCellStatisticsDataGridViewModel : BindableBase, ICustomControlCellStatisticsDataGridViewModel
    {
        #region Fields

        private IEnumerable<CellStatusStatistic> cells;

        private CellStatusStatistic selectedCell;

        #endregion

        #region Properties

        public IEnumerable<CellStatusStatistic> Cells { get => this.cells; set => this.SetProperty(ref this.cells, value); }

        public BindableBase NavigationViewModel { get; set; }

        public CellStatusStatistic SelectedCell { get => this.selectedCell; set => this.SetProperty(ref this.selectedCell, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // HACK
        }

        public Task OnEnterViewAsync()
        {
            // HACK
            return Task.CompletedTask;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // HACK
        }

        #endregion
    }
}

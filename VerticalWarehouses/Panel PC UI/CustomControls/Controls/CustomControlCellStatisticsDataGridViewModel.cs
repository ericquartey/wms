using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.CustomControls.Utils;
using Prism.Mvvm;

namespace Ferretto.VW.CustomControls.Controls
{
    public class CustomControlCellStatisticsDataGridViewModel : BindableBase, ICustomControlCellStatisticsDataGridViewModel
    {
        #region Fields

        private ObservableCollection<DataGridCell> cells;

        private DataGridCell selectedCell;

        #endregion

        #region Properties

        public ObservableCollection<DataGridCell> Cells { get => this.cells; set => this.SetProperty(ref this.cells, value); }

        public BindableBase NavigationViewModel { get; set; }

        public DataGridCell SelectedCell { get => this.selectedCell; set => this.SetProperty(ref this.selectedCell, value); }

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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomControlErrorsDataGridViewModel : BindableBase, ICustomControlErrorsDataGridViewModel
    {
        #region Fields

        private IEnumerable<ErrorStatisticsDetail> cells;

        private ErrorStatisticsDetail selectedError;

        #endregion

        #region Properties

        public IEnumerable<ErrorStatisticsDetail> Cells { get => this.cells; set => this.SetProperty(ref this.cells, value); }

        public BindableBase NavigationViewModel { get; set; }

        public ErrorStatisticsDetail SelectedCell { get => this.selectedError; set => this.SetProperty(ref this.selectedError, value); }

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

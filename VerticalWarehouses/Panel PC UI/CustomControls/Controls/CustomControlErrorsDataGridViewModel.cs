using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomControlErrorsDataGridViewModel : BindableBase, ICustomControlErrorsDataGridViewModel
    {
        #region Fields

        private ObservableCollection<DataGridError> errors;

        private DataGridError selectedError;

        #endregion

        #region Properties

        public ObservableCollection<DataGridError> Errors { get => this.errors; set => this.SetProperty(ref this.errors, value); }

        public BindableBase NavigationViewModel { get; set; }

        public DataGridError SelectedError { get => this.selectedError; set => this.SetProperty(ref this.selectedError, value); }

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

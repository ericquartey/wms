using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class ErrorsStatisticsViewModel : BaseViewModel, IErrorsStatisticsViewModel
    {
        #region Fields

        private readonly CustomControlErrorsDataGridViewModel dataGridViewModelRef;

        private readonly IEventAggregator eventAggregator;

        private BindableBase dataGridViewModel;

        private ObservableCollection<DataGridError> errors;

        private DataGridError selectedError;

        #endregion

        #region Constructors

        public ErrorsStatisticsViewModel(
            IEventAggregator eventAggregator,
            ICustomControlErrorsDataGridViewModel errorsDataGridViewModel)
        {
            this.eventAggregator = eventAggregator;
            this.dataGridViewModelRef = errorsDataGridViewModel as CustomControlErrorsDataGridViewModel;
            this.DataGridViewModel = this.dataGridViewModelRef;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        #endregion

        #region Methods

        public override async Task OnEnterViewAsync()
        {
            var random = new Random();
            this.errors = new ObservableCollection<DataGridError>();
            for (var i = 0; i < random.Next(3, 30); i++)
            {
                this.errors.Add(new DataGridError
                {
                    Error = $"Error {i + 1}",
                    Total = random.Next(0, 500).ToString(),
                    TotalPercentage = random.Next(0, 100).ToString()
                }
                );
            }
            this.selectedError = this.errors[0];
            this.dataGridViewModelRef.Errors = this.errors;
            this.dataGridViewModelRef.SelectedError = this.selectedError;
            this.dataGridViewModel = this.dataGridViewModelRef;
        }

        #endregion
    }
}

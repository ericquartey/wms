using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.CustomControls.Utils;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class ErrorsStatisticsViewModel : BindableBase, IErrorsStatisticsViewModel
    {
        #region Fields

        private IUnityContainer container;

        private BindableBase dataGridViewModel;

        private CustomControlErrorsDataGridViewModel dataGridViewModelRef;

        private ObservableCollection<DataGridError> errors;

        private IEventAggregator eventAggregator;

        private DataGridError selectedError;

        #endregion

        #region Constructors

        public ErrorsStatisticsViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.dataGridViewModelRef = this.container.Resolve<ICustomControlErrorsDataGridViewModel>() as CustomControlErrorsDataGridViewModel;
            this.DataGridViewModel = this.dataGridViewModelRef;
        }

        public async Task OnEnterViewAsync()
        {
            var random = new Random();
            this.errors = new ObservableCollection<DataGridError>();
            for (int i = 0; i < random.Next(3, 30); i++)
            {
                errors.Add(new DataGridError
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

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}

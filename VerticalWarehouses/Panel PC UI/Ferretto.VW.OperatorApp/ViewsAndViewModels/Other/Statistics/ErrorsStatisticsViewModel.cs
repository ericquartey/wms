using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class ErrorsStatisticsViewModel : BindableBase, IErrorsStatisticsViewModel
    {
        #region Fields

        private readonly IErrorsService errorsService;

        private readonly IFeedbackNotifier feedbackNotifier;

        private int currentItemIndex;

        private ICustomControlErrorsDataGridViewModel dataGridViewModelRef;

        private ICommand downDataGridButtonCommand;

        private ErrorStatisticsSummary statistics;

        private ICommand upDataGridButtonCommand;

        #endregion

        #region Constructors

        public ErrorsStatisticsViewModel(
            IFeedbackNotifier feedbackNotifier,
            IErrorsService errorsService,
            ICustomControlErrorsDataGridViewModel errorsDataGridViewModel)
        {
            this.feedbackNotifier = feedbackNotifier;
            this.errorsService = errorsService;
            this.dataGridViewModelRef = errorsDataGridViewModel;
            this.DataGridViewModel = this.dataGridViewModelRef;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICustomControlErrorsDataGridViewModel DataGridViewModel { get => this.dataGridViewModelRef; set => this.SetProperty(ref this.dataGridViewModelRef, value); }

        public ICommand DownDataGridButtonCommand => this.downDataGridButtonCommand ?? (this.downDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItemAsync(false)));

        public BindableBase NavigationViewModel { get; set; }

        public ErrorStatisticsSummary Statistics { get => this.statistics; }

        public ICommand UpDataGridButtonCommand => this.upDataGridButtonCommand ?? (this.upDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItemAsync(true)));

        #endregion

        #region Methods

        public async void ChangeSelectedItemAsync(bool isUp)
        {
            if (!(this.dataGridViewModelRef is CustomControlErrorsDataGridViewModel gridData))
            {
                return;
            }

            var count = gridData.Cells.Count();
            if (gridData.Cells != null && count != 0)
            {
                this.currentItemIndex = isUp ? --this.currentItemIndex : ++this.currentItemIndex;
                if (this.currentItemIndex < 0 || this.currentItemIndex >= count)
                {
                    this.currentItemIndex = (this.currentItemIndex < 0) ? 0 : count - 1;
                }

                gridData.SelectedCell = gridData.Cells.ToList()[this.currentItemIndex];
            }
        }

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task OnEnterViewAsync()
        {
            if (!(this.dataGridViewModelRef is CustomControlErrorsDataGridViewModel gridData))
            {
                return;
            }

            try
            {
                this.statistics = await this.errorsService.GetStatisticsAsync();
                var selectedError = this.statistics.Errors.FirstOrDefault();

                gridData.Cells = this.statistics.Errors;
                gridData.SelectedCell = selectedError;
                this.currentItemIndex = 0;

                this.RaisePropertyChanged(nameof(this.DataGridViewModel));
                this.RaisePropertyChanged(nameof(this.Statistics));
            }
            catch (Exception ex)
            {
                this.feedbackNotifier.Notify($"Cannot load data. {ex.Message}");
            }
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

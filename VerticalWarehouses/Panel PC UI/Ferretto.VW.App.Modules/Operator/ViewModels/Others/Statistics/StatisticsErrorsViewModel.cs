using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class StatisticsErrorsViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly ICustomControlErrorsDataGridViewModel errorsDataGridViewModel;

        private readonly IMachineErrorsWebService errorsService;

        private int currentItemIndex;

        private ICustomControlErrorsDataGridViewModel dataGridViewModelRef;

        private ICommand downDataGridButtonCommand;

        private ErrorStatisticsSummary statistics;

        private ICommand upDataGridButtonCommand;

        #endregion

        #region Constructors

        public StatisticsErrorsViewModel(
            IMachineErrorsWebService errorsService,
            ICustomControlErrorsDataGridViewModel errorsDataGridViewModel)
            : base(PresentationMode.Operator)
        {
            this.errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
            this.errorsDataGridViewModel = errorsDataGridViewModel ?? throw new ArgumentNullException(nameof(errorsDataGridViewModel));

            this.dataGridViewModelRef = errorsDataGridViewModel as CustomControlErrorsDataGridViewModel;
        }

        #endregion

        #region Properties

        public ICustomControlErrorsDataGridViewModel DataGridViewModel { get => this.dataGridViewModelRef; set => this.SetProperty(ref this.dataGridViewModelRef, value); }

        public ICommand DownDataGridButtonCommand => this.downDataGridButtonCommand ?? (this.downDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItemAsync(false)));

        public override EnableMask EnableMask => EnableMask.Any;

        public ErrorStatisticsSummary Statistics => this.statistics;

        public ICommand UpDataGridButtonCommand => this.upDataGridButtonCommand ?? (this.upDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItemAsync(true)));

        #endregion

        #region Methods

        public void ChangeSelectedItemAsync(bool isUp)
        {
            if (!(this.dataGridViewModelRef is CustomControlErrorsDataGridViewModel gridData))
            {
                return;
            }

            var count = gridData.Cells?.Count() ?? 0;
            if (gridData?.Cells != null && count != 0)
            {
                this.currentItemIndex = isUp ? --this.currentItemIndex : ++this.currentItemIndex;
                if (this.currentItemIndex < 0 || this.currentItemIndex >= count)
                {
                    this.currentItemIndex = (this.currentItemIndex < 0) ? 0 : count - 1;
                }

                gridData.SelectedCell = gridData.Cells.ToList()[this.currentItemIndex];
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            if (!(this.dataGridViewModelRef is CustomControlErrorsDataGridViewModel gridData))
            {
                return;
            }

            try
            {
                this.statistics = await this.errorsService.GetStatisticsAsync();
                var selectedError = Enumerable.FirstOrDefault(this.statistics.Errors);

                gridData.Cells = Enumerable.OrderByDescending(this.statistics.Errors, e => e.Total);
                gridData.SelectedCell = selectedError;
                this.currentItemIndex = 0;

                this.RaisePropertyChanged(nameof(this.DataGridViewModel));
                this.RaisePropertyChanged(nameof(this.Statistics));
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}

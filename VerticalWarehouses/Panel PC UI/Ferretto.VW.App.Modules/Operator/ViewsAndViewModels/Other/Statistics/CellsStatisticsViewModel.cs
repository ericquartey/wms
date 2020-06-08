using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Mvvm;

namespace Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.Other.Statistics
{
    public class CellsStatisticsViewModel : BaseViewModel
    {
        #region Fields

        private readonly IMachineCellsWebService cellsService;

        private readonly CustomControlCellStatisticsDataGridViewModel dataGridViewModelRef;

        private ObservableCollection<CellStatusStatistics> cells;

        private CellStatisticsSummary cellStatistics;

        private BindableBase dataGridViewModel;

        private CellStatusStatistics selectedCell;

        #endregion

        #region Constructors

        public CellsStatisticsViewModel(
            IMachineCellsWebService cellsService,
            ICustomControlCellStatisticsDataGridViewModel cellStatisticsDataGridViewModel)
        {
            if (cellsService == null)
            {
                throw new System.ArgumentNullException(nameof(cellsService));
            }

            this.cellsService = cellsService;
            this.dataGridViewModelRef = cellStatisticsDataGridViewModel as CustomControlCellStatisticsDataGridViewModel;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ObservableCollection<CellStatusStatistics> Cells
        {
            get => this.cells;
            set => this.SetProperty(ref this.cells, value);
        }

        public CellStatisticsSummary CellStatistics
        {
            get => this.cellStatistics;
            set => this.SetProperty(ref this.cellStatistics, value);
        }

        public BindableBase DataGridViewModel
        {
            get => this.dataGridViewModel;
            set => this.SetProperty(ref this.dataGridViewModel, value);
        }

        /*
        public ICommand DrawerCompactingButtonCommand =>
            this.drawerCompactingButtonCommand
            ??
            (this.drawerCompactingButtonCommand = new DelegateCommand(() =>
                this.navigationService.NavigateToView<DrawerCompactingViewModel, IDrawerCompactingViewModel>()));
        */

        public CellStatusStatistics SelectedCell
        {
            get => this.selectedCell;
            set => this.SetProperty(ref this.selectedCell, value);
        }

        #endregion

        #region Methods

        public override async Task OnEnterViewAsync()
        {
            try
            {
                this.CellStatistics = await this.cellsService.GetStatisticsAsync();

                this.SelectedCell = this.CellStatistics.CellStatusStatistics.FirstOrDefault();

                this.dataGridViewModelRef.Cells = this.CellStatistics.CellStatusStatistics;
                this.dataGridViewModelRef.SelectedCell = this.SelectedCell;

                this.DataGridViewModel = this.dataGridViewModelRef;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                // this.statusMessageService.Notify(ex);
            }
        }

        #endregion
    }
}

using System.Collections.ObjectModel;
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
    public class CellsStatisticsViewModel : BaseViewModel, ICellsStatisticsViewModel
    {
        #region Fields

        private readonly ICellsService cellsService;

        private readonly CustomControlCellStatisticsDataGridViewModel dataGridViewModelRef;

        private readonly INavigationService navigationService;

        private ObservableCollection<CellStatusStatistics> cells;

        private CellStatisticsSummary cellStatistics;

        private BindableBase dataGridViewModel;

        private ICommand drawerCompactingButtonCommand;

        private CellStatusStatistics selectedCell;

        #endregion

        #region Constructors

        public CellsStatisticsViewModel(
            INavigationService navigationService,
            ICellsService cellsService,
            ICustomControlCellStatisticsDataGridViewModel cellStatisticsDataGridViewModel)
        {
            this.navigationService = navigationService;
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

        public CellStatisticsSummary CellStatistics => this.cellStatistics;

        public BindableBase DataGridViewModel
        {
            get => this.dataGridViewModel;
            set => this.SetProperty(ref this.dataGridViewModel, value);
        }

        public ICommand DrawerCompactingButtonCommand =>
            this.drawerCompactingButtonCommand
            ??
            (this.drawerCompactingButtonCommand = new DelegateCommand(() =>
                this.navigationService.NavigateToView<DrawerCompactingViewModel, IDrawerCompactingViewModel>()));

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
                this.cellStatistics = await this.cellsService.GetStatisticsAsync();

                this.SelectedCell = this.cellStatistics.CellStatusStatistics.FirstOrDefault();

                this.dataGridViewModelRef.Cells = this.cellStatistics.CellStatusStatistics;
                this.dataGridViewModelRef.SelectedCell = this.SelectedCell;

                this.DataGridViewModel = this.dataGridViewModelRef;

                this.RaisePropertyChanged(nameof(this.CellStatistics));
            }
            catch
            {
                //TODO call toolbar notification service
            }
        }

        #endregion
    }
}

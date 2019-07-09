using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.WmsCommunication.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class CellsStatisticsViewModel : BindableBase, ICellsStatisticsViewModel
    {
        #region Fields

        private readonly CustomControlCellStatisticsDataGridViewModel dataGridViewModelRef;

        private readonly IEventAggregator eventAggregator;

        private readonly INavigationService navigationService;

        private readonly IWmsDataProvider wmsDataProvider;

        private ObservableCollection<DataGridCell> cells;

        private BindableBase dataGridViewModel;

        private ICommand drawerCompactingButtonCommand;

        private DataGridCell selectedCell;

        #endregion

        #region Constructors

        public CellsStatisticsViewModel(
            IEventAggregator eventAggregator,
            INavigationService navigationService,
            IWmsDataProvider wmsDataProvider,
            ICustomControlCellStatisticsDataGridViewModel cellStatisticsDataGridViewModel)
        {
            this.eventAggregator = eventAggregator;
            this.wmsDataProvider = wmsDataProvider;
            this.navigationService = navigationService;

            this.dataGridViewModelRef = cellStatisticsDataGridViewModel as CustomControlCellStatisticsDataGridViewModel;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ObservableCollection<DataGridCell> Cells { get => this.cells; set => this.SetProperty(ref this.cells, value); }

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public ICommand DrawerCompactingButtonCommand =>
            this.drawerCompactingButtonCommand
            ??
            (this.drawerCompactingButtonCommand = new DelegateCommand(() =>
                this.navigationService.NavigateToView<DrawerCompactingViewModel, IDrawerCompactingViewModel>()));

        public BindableBase NavigationViewModel { get; set; }

        public DataGridCell SelectedCell { get => this.selectedCell; set => this.SetProperty(ref this.selectedCell, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task OnEnterViewAsync()
        {
            this.Cells = new ObservableCollection<DataGridCell>();
            var random = new Random();
            for (var i = 0; i < random.Next(5, 20); i++)
            {
                var cell = new DataGridCell
                {
                    State = "State",
                    TotalFront = random.Next(0, 30).ToString(),
                    TotalFrontPercentage = random.Next(0, 100).ToString(),
                    TotalBack = random.Next(0, 30).ToString(),
                    TotalBackPercentage = random.Next(0, 100).ToString()
                };
                this.Cells.Add(cell);
            }
            this.SelectedCell = this.Cells[0];

            this.dataGridViewModelRef.Cells = this.Cells;
            this.dataGridViewModelRef.SelectedCell = this.SelectedCell;

            this.DataGridViewModel = this.dataGridViewModelRef;
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

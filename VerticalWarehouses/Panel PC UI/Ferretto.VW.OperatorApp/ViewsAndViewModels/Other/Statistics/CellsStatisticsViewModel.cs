using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.WmsCommunication.Interfaces;
using System.Collections.ObjectModel;
using Ferretto.VW.CustomControls.Utils;

    public class CellsStatisticsViewModel : BindableBase, ICellsStatisticsViewModel
    {
        #region Fields

        private ObservableCollection<DataGridCell> cells;

        private IUnityContainer container;

        private BindableBase dataGridViewModel;

        private CustomControlCellStatisticsDataGridViewModel dataGridViewModelRef;

        private ICommand drawerCompactingButtonCommand;

        private IEventAggregator eventAggregator;

        private DataGridCell selectedCell;

        private IWmsDataProvider wmsDataProvider;

        #endregion

        #region Constructors

        public CellsStatisticsViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ObservableCollection<DataGridCell> Cells { get => this.cells; set => this.SetProperty(ref this.cells, value); }

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public ICommand DrawerCompactingButtonCommand => this.drawerCompactingButtonCommand ?? (this.drawerCompactingButtonCommand = new DelegateCommand(() =>
                {
                    NavigationService.NavigateToView<DrawerCompactingViewModel, IDrawerCompactingViewModel>();
                }));

        public BindableBase NavigationViewModel { get; set; }

        public DataGridCell SelectedCell { get => this.selectedCell; set => this.SetProperty(ref this.selectedCell, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.dataGridViewModelRef = this.container.Resolve<ICustomControlCellStatisticsDataGridViewModel>() as CustomControlCellStatisticsDataGridViewModel;

            this.wmsDataProvider = this.container.Resolve<IWmsDataProvider>();
        }

        public async Task OnEnterViewAsync()
        {
            this.Cells = new ObservableCollection<DataGridCell>();
            var random = new Random();
            for (int i = 0; i < random.Next(5, 20); i++)
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

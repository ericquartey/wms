﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Mvvm;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    public class StatisticsCellsViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineCellsWebService cellsService;

        private readonly ICustomControlCellStatisticsDataGridViewModel cellStatisticsDataGridViewModel;

        private readonly CustomControlCellStatisticsDataGridViewModel dataGridViewModelRef;

        private ObservableCollection<CellStatusStatistics> cells;

        private CellStatisticsSummary cellStatistics;

        private BindableBase dataGridViewModel;

        private CellStatusStatistics selectedCell;

        #endregion

        #region Constructors

        public StatisticsCellsViewModel(IMachineCellsWebService cellsService,
            ICustomControlCellStatisticsDataGridViewModel cellStatisticsDataGridViewModel)
            : base(PresentationMode.Operator)
        {
            this.cellsService = cellsService ?? throw new ArgumentNullException(nameof(cellsService));
            this.cellStatisticsDataGridViewModel = cellStatisticsDataGridViewModel ?? throw new ArgumentNullException(nameof(cellStatisticsDataGridViewModel));

            this.dataGridViewModelRef = cellStatisticsDataGridViewModel as CustomControlCellStatisticsDataGridViewModel;
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

        public override EnableMask EnableMask => EnableMask.Any;

        public CellStatusStatistics SelectedCell
        {
            get => this.selectedCell;
            set => this.SetProperty(ref this.selectedCell, value);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
            try
            {
                this.CellStatistics = await this.cellsService.GetStatisticsAsync();

                this.SelectedCell = this.CellStatistics.CellStatusStatistics.FirstOrDefault();

                this.dataGridViewModelRef.Cells = this.CellStatistics.CellStatusStatistics;
                this.dataGridViewModelRef.SelectedCell = this.SelectedCell;

                this.DataGridViewModel = this.dataGridViewModelRef;

                this.RaisePropertyChanged(nameof(this.DataGridViewModel));
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}

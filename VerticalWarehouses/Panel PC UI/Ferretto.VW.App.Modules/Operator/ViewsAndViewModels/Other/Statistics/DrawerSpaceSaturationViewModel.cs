using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.Other.Statistics
{
    public class DrawerSpaceSaturationViewModel : BaseViewModel
    {
        #region Fields

        private readonly IMachineIdentityWebService identityService;

        private readonly IMachineLoadingUnitsWebService loadingUnitService;

        private readonly ISessionService sessionService;

        // private readonly IMachineStatisticsService machineStatisticsService;

        private int currentItemIndex;

        private ICustomControlDrawerSaturationDataGridViewModel dataGridViewModel;

        private string dimension;

        private ICommand downDataGridButtonCommand;

        private double fillPercentage;

        private DataGridDrawerSaturation selectedLoadingUnit;

        private int totalLoadingUnits;

        private ICommand upDataGridButtonCommand;

        #endregion

        #region Constructors

        public DrawerSpaceSaturationViewModel(
            IMachineLoadingUnitsWebService loadingUnitService,
            IMachineIdentityWebService identityService,
            ISessionService sessionService,

            // IMachineStatisticsService machineStatisticsService,
            ICustomControlDrawerSaturationDataGridViewModel drawerSaturationDataGridViewModel)
        {
            this.loadingUnitService = loadingUnitService;
            this.identityService = identityService;
            this.sessionService = sessionService;

            // this.machineStatisticsService = machineStatisticsService;
            this.dataGridViewModel = drawerSaturationDataGridViewModel;
        }

        #endregion

        #region Properties

        public ICustomControlDrawerSaturationDataGridViewModel DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public string Dimension { get => this.dimension; set => this.SetProperty(ref this.dimension, value); }

        public ICommand DownDataGridButtonCommand => this.downDataGridButtonCommand ?? (this.downDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItem(false)));

        public double FillPercentage { get => this.fillPercentage; set => this.SetProperty(ref this.fillPercentage, value); }

        public DataGridDrawerSaturation SelectedDrawer { get => this.selectedLoadingUnit; set => this.SetProperty(ref this.selectedLoadingUnit, value); }

        public int TotalLoadingUnits { get => this.totalLoadingUnits; set => this.SetProperty(ref this.totalLoadingUnits, value); }

        public ICommand UpDataGridButtonCommand => this.upDataGridButtonCommand ?? (this.upDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItem(true)));

        #endregion

        #region Methods

        public void ChangeSelectedItem(bool isUp)
        {
            if (!(this.dataGridViewModel is CustomControlDrawerSaturationDataGridViewModel gridData))
            {
                return;
            }

            var count = gridData.LoadingUnits.Count();
            if (gridData.LoadingUnits != null && count != 0)
            {
                this.currentItemIndex = isUp ? --this.currentItemIndex : ++this.currentItemIndex;
                if (this.currentItemIndex < 0 || this.currentItemIndex >= count)
                {
                    this.currentItemIndex = (this.currentItemIndex < 0) ? 0 : count - 1;
                }

                gridData.SelectedLoadingUnit = gridData.LoadingUnits.ToList()[this.currentItemIndex];
            }
        }

        public override async Task OnEnterViewAsync()
        {
            if (!(this.dataGridViewModel is CustomControlDrawerSaturationDataGridViewModel gridData))
            {
                return;
            }

            try
            {
                var loadingUnits = await this.loadingUnitService.GetSpaceStatisticsAsync();
                var selectedLoadingUnit = loadingUnits.FirstOrDefault();

                gridData.LoadingUnits = loadingUnits;
                gridData.SelectedLoadingUnit = selectedLoadingUnit;
                this.TotalLoadingUnits = loadingUnits.Count();

                this.currentItemIndex = 0;

                var machine = this.sessionService.MachineIdentity;
                this.Dimension = $"{(int)machine.Width}x{(int)machine.Depth}";

                this.RaisePropertyChanged(nameof(this.DataGridViewModel));

                /*
                var machineStat = await this.machineStatisticsService.GetAsync();
                if (machineStat.AreaFillPercentage.HasValue)
                {
                    this.FillPercentage = machineStat.AreaFillPercentage.Value;
                }
                */
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                // this.statusMessageService.Notify($"Cannot load data. {ex.Message}");
            }
        }

        #endregion
    }
}

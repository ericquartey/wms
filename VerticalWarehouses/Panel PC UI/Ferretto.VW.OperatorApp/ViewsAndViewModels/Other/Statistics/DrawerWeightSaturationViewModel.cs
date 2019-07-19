using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Commands;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class DrawerWeightSaturationViewModel : BaseViewModel, IDrawerWeightSaturationViewModel
    {
        #region Fields

        private readonly CustomControlDrawerWeightSaturationDataGridViewModel dataGridViewModelRef;

        private readonly IFeedbackNotifier feedbackNotifier;

        private readonly IIdentityService identityService;

        private readonly ILoadingUnitsService loadingUnitService;

        private readonly INavigationService navigationService;

        private int currentItemIndex;

        private ICustomControlDrawerWeightSaturationDataGridViewModel dataGridViewModel;

        private ICommand drawerSpaceSaturationButtonCommand;

        private decimal grossWeight;

        private decimal maxGrossWeight;

        private decimal maxNetWeight;

        private decimal netWeight;

        private decimal netWeightPercent;

        private int totalLoadingUnits;

        #endregion

        #region Constructors

        public DrawerWeightSaturationViewModel(
            ILoadingUnitsService loadingUnitService,
            IIdentityService identityService,
            IFeedbackNotifier feedbackNotifier,
            INavigationService navigationService,
            ICustomControlDrawerWeightSaturationDataGridViewModel drawerWeightSaturationDataGridViewModel)
        {
            this.dataGridViewModel = drawerWeightSaturationDataGridViewModel;
            this.feedbackNotifier = feedbackNotifier;
            this.loadingUnitService = loadingUnitService;
            this.navigationService = navigationService;
            this.identityService = identityService;
        }

        #endregion

        #region Properties

        public ICustomControlDrawerWeightSaturationDataGridViewModel DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public ICommand DrawerSpaceSaturationButtonCommand => this.drawerSpaceSaturationButtonCommand ?? (this.drawerSpaceSaturationButtonCommand = new DelegateCommand(() =>
                {
                    this.navigationService.NavigateToView<DrawerSpaceSaturationViewModel, IDrawerSpaceSaturationViewModel>();
                }));

        public int TotalLoadingUnits { get => this.totalLoadingUnits; set => this.SetProperty(ref this.totalLoadingUnits, value); }

        private decimal GrossWeight { get => this.grossWeight; set => this.SetProperty(ref this.grossWeight, value); }

        private decimal MaxGrossWeight { get => this.maxGrossWeight; set => this.SetProperty(ref this.maxGrossWeight, value); }

        private decimal MaxNetWeight { get => this.maxNetWeight; set => this.SetProperty(ref this.maxNetWeight, value); }

        private decimal NetWeight { get => this.netWeight; set => this.SetProperty(ref this.netWeight, value); }

        private decimal NetWeightPercent { get => this.netWeightPercent; set => this.SetProperty(ref this.netWeightPercent, value); }

        #endregion

        #region Methods

        public async void ChangeSelectedItemAsync(bool isUp)
        {
            if (!(this.dataGridViewModel is CustomControlDrawerWeightSaturationDataGridViewModel gridData))
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
            if (!(this.dataGridViewModel is CustomControlDrawerWeightSaturationDataGridViewModel gridData))
            {
                return;
            }

            try
            {
                var loadingUnits = await this.loadingUnitService.GetWeightStatisticsAsync();
                var selectedLoadingUnit = loadingUnits.FirstOrDefault();

                gridData.LoadingUnits = loadingUnits;
                gridData.SelectedLoadingUnit = selectedLoadingUnit;
                this.TotalLoadingUnits = loadingUnits.Count();
                this.currentItemIndex = 0;
                var machine = await this.identityService.GetAsync();
                this.MaxGrossWeight = machine.MaxGrossWeight;
                this.MaxNetWeight = machine.MaxGrossWeight - loadingUnits.Sum(l => l.Tare);
                this.GrossWeight = loadingUnits.Sum(l => l.GrossWeight);
                this.NetWeight = loadingUnits.Sum(l => l.GrossWeight) - loadingUnits.Sum(l => l.Tare);
                this.NetWeightPercent = this.NetWeight / this.MaxNetWeight;

                this.RaisePropertyChanged(nameof(this.DataGridViewModel));
            }
            catch (Exception ex)
            {
                this.feedbackNotifier.Notify($"Cannot load data. {ex.Message}");
            }
        }

        #endregion
    }
}

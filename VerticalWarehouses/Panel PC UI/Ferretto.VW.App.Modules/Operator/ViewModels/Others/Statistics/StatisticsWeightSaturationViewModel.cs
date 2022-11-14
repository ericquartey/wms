using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    public class StatisticsWeightSaturationViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly CustomControlDrawerWeightSaturationDataGridViewModel dataGridViewModelRef;

        private readonly IMachineIdentityWebService identityService;

        private readonly IMachineLoadingUnitsWebService loadingUnitService;

        private readonly ISessionService sessionService;

        private int currentItemIndex;

        private ICommand downDataGridButtonCommand;

        private ICommand drawerSpaceSaturationButtonCommand;

        private ICustomControlDrawerWeightSaturationDataGridViewModel drawerWeightSaturationDataGridViewModel;

        private double grossWeight;

        private double maxGrossWeight;

        private double maxNetWeight;

        private double netWeight;

        private double netWeightPercent;

        private int totalLoadingUnits;

        private ICommand upDataGridButtonCommand;

        #endregion

        #region Constructors

        public StatisticsWeightSaturationViewModel(
            IMachineLoadingUnitsWebService loadingUnitService,
            IMachineIdentityWebService identityService,
            ISessionService sessionService,
            ICustomControlDrawerWeightSaturationDataGridViewModel drawerWeightSaturationDataGridViewModel)
            : base(PresentationMode.Operator)
        {
            this.loadingUnitService = loadingUnitService ?? throw new ArgumentNullException(nameof(loadingUnitService));
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.drawerWeightSaturationDataGridViewModel = drawerWeightSaturationDataGridViewModel ?? throw new ArgumentNullException(nameof(drawerWeightSaturationDataGridViewModel));

            this.dataGridViewModelRef = drawerWeightSaturationDataGridViewModel as CustomControlDrawerWeightSaturationDataGridViewModel;
        }

        #endregion

        #region Properties

        public ICustomControlDrawerWeightSaturationDataGridViewModel DataGridViewModel { get => this.drawerWeightSaturationDataGridViewModel; set => this.SetProperty(ref this.drawerWeightSaturationDataGridViewModel, value); }

        public ICommand DownDataGridButtonCommand => this.downDataGridButtonCommand ?? (this.downDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItem(false)));

        public ICommand DrawerSpaceSaturationButtonCommand => this.drawerSpaceSaturationButtonCommand ?? (this.drawerSpaceSaturationButtonCommand = new DelegateCommand(() =>
        {
            this.NavigationService.Appear(
                   nameof(Utils.Modules.Operator),
                   Utils.Modules.Operator.Others.Statistics.Drawers.SPACESATURATION,
                   null,
                   trackCurrentView: true);
        }));

        public override EnableMask EnableMask => EnableMask.Any;

        public double GrossWeight { get => this.grossWeight; set => this.SetProperty(ref this.grossWeight, value); }

        public double MaxGrossWeight
        {
            get => this.maxGrossWeight;
            set => this.SetProperty(ref this.maxGrossWeight, value);
        }

        public double MaxNetWeight { get => this.maxNetWeight; set => this.SetProperty(ref this.maxNetWeight, value); }

        public double NetWeight { get => this.netWeight; set => this.SetProperty(ref this.netWeight, value); }

        public double NetWeightPercent { get => this.netWeightPercent; set => this.SetProperty(ref this.netWeightPercent, value); }

        public int TotalLoadingUnits { get => this.totalLoadingUnits; set => this.SetProperty(ref this.totalLoadingUnits, value); }

        public ICommand UpDataGridButtonCommand => this.upDataGridButtonCommand ?? (this.upDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItem(true)));

        #endregion

        #region Methods

        public void ChangeSelectedItem(bool isUp)
        {
            if (!(this.drawerWeightSaturationDataGridViewModel is CustomControlDrawerWeightSaturationDataGridViewModel gridData))
            {
                return;
            }

            var count = gridData.LoadingUnits?.Count() ?? 0;
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

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            if (!(this.drawerWeightSaturationDataGridViewModel is CustomControlDrawerWeightSaturationDataGridViewModel gridData))
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
                var machine = this.sessionService.MachineIdentity;
                this.MaxGrossWeight = machine.MaxGrossWeight;
                this.MaxNetWeight = machine.MaxGrossWeight - loadingUnits.Sum(l => l.Tare);
                this.GrossWeight = loadingUnits.Sum(l => l.GrossWeight);
                this.NetWeight = loadingUnits.Sum(l => l.GrossWeight) - loadingUnits.Sum(l => l.Tare);
                this.NetWeightPercent = this.NetWeight * 100 / this.MaxNetWeight;

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

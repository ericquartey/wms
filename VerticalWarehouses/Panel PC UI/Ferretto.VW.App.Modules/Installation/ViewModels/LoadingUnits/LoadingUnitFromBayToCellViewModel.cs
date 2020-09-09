using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class LoadingUnitFromBayToCellViewModel : BaseCellMovementsViewModel
    {
        #region Fields

        private bool isEnabledEditing;

        private LoadingUnit selectedLU;

        #endregion

        #region Constructors

        public LoadingUnitFromBayToCellViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineModeWebService machineModeWebService,
            IMachineCellsWebService machineCellsWebService,
            ISensorsService sensorsService,
            IBayManager bayManagerService)
            : base(
                machineLoadingUnitsWebService,
                machineCellsWebService,
                machineModeWebService,
                sensorsService,
                bayManagerService)
        {
        }

        #endregion

        #region Properties

        public bool IsEnabledEditing
        {
            get => this.isEnabledEditing;
            set => this.SetProperty(ref this.isEnabledEditing, value && !this.IsMoving);
        }

        public LoadingUnit SelectedLU
        {
            get => this.selectedLU;
            set => this.SetProperty(ref this.selectedLU, value);
        }

        #endregion

        #region Methods

        public override bool CanStart()
        {
            return base.CanStart() &&
                   !this.IsMoving &&
                   this.MachineModeService.MachineMode == MachineMode.Manual &&
                   ((this.SensorsService.IsLoadingUnitInBay && (this.MachineService.Bay.IsDouble || this.MachineService.BayFirstPositionIsUpper)) ||
                    (!this.MachineService.HasCarousel && this.SensorsService.IsLoadingUnitInMiddleBottomBay && (this.MachineService.Bay.IsDouble || !this.MachineService.BayFirstPositionIsUpper))) &&
                   this.LoadingUnitId.HasValue &&
                   !this.MachineService.Loadunits.DrawerInLocationById(this.LoadingUnitId.Value);
        }

        public async Task GetLoadingUnits()
        {
            try
            {
                if (this.LoadingUnitId is null)
                {
                    var lst = await this.MachineLoadingUnitsWebService.GetAllAsync();
                    if (lst.Any())
                    {
                        this.LoadingUnitId = lst.Where(x => x.Status == LoadingUnitStatus.Undefined).FirstOrDefault()?.Id ?? (lst.Max(o => o.Id) + 1);
                    }
                    else
                    {
                        this.LoadingUnitId = null;
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
        }

        public override async Task StartAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                if (!this.IsLoadingUnitIdValid)
                {
                    await this.MachineLoadingUnitsWebService.InsertLoadingUnitOnlyDbAsync(this.LoadingUnitId.Value);
                }

                var source = this.GetLoadingUnitSource(this.IsPositionDownSelected);

                if (source == LoadingUnitLocation.NoLocation)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.InvalidSourceChoiceType"), Services.Models.NotificationSeverity.Warning);
                    return;
                }

                await this.MachineLoadingUnitsWebService.InsertLoadingUnitAsync(source, null, this.LoadingUnitId.Value);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        protected override void Ended()
        {
            base.Ended();

            this.RetrieveCellsAsync().ConfigureAwait(false);

            this.GetLoadingUnits().ConfigureAwait(false);
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.SensorsService.RefreshAsync(true);

            await this.InitializingData();
        }

        protected override async void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            await this.UpdateDrawerInfo();
        }

        private async Task InitializingData()
        {
            await this.GetLoadingUnits();

            if (this.MachineService.Bay.IsDouble || this.MachineService.BayFirstPositionIsUpper || this.MachineService.HasCarousel)
            {
                this.SelectBayPositionUp();
            }
            else
            {
                this.SelectBayPositionDown();
            }

            if (this.MachineStatus.LoadingUnitPositionDownInBay != null)
            {
                this.LoadingUnitId = this.MachineStatus.LoadingUnitPositionDownInBay.Id;
            }

            if (this.MachineStatus.LoadingUnitPositionUpInBay != null)
            {
                this.LoadingUnitId = this.MachineStatus.LoadingUnitPositionUpInBay.Id;
            }
        }

        private async Task UpdateDrawerInfo()
        {
            var lst = await this.MachineLoadingUnitsWebService.GetAllAsync();

            if (!lst.Any())
            {
                return;
            }

            if (this.LoadingUnitId.HasValue && lst.Any(i => i.Id == this.LoadingUnitId))
            {
                this.isEnabledEditing = false;
                this.selectedLU = lst.FirstOrDefault(i => i.Id == this.LoadingUnitId);
            }
            else
            {
                this.isEnabledEditing = true;
                var maxLU = lst.OrderByDescending(S => S.Id).FirstOrDefault();
                this.selectedLU = new LoadingUnit();
                if (maxLU != null)
                {
                    this.selectedLU.Tare = maxLU.Tare;
                    this.selectedLU.Height = maxLU.Height;
                    this.selectedLU.MaxNetWeight = maxLU.MaxNetWeight;
                    this.selectedLU.NetWeight = maxLU.NetWeight;
                    this.selectedLU.GrossWeight = maxLU.GrossWeight;
                }
            }

            this.RaisePropertyChanged(nameof(this.SelectedLU));
            this.RaisePropertyChanged(nameof(this.IsEnabledEditing));
        }

        #endregion
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class LoadingUnitFromCellToBayViewModel : BaseMovementsViewModel
    {
        #region Fields

        private readonly IMachineBaysWebService machineBaysWebService;

        private DelegateCommand confirmEjectLoadingUnitCommand;

        #endregion

        //private bool isEjectLoadingUnitConfirmationEnabled;

        #region Constructors

        public LoadingUnitFromCellToBayViewModel(
            IMachineBaysWebService machineBaysWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineModeWebService machineModeWebService,
            IBayManager bayManagerService,
            IMachineExternalBayWebService machineExternalBayWebService)
            : base(
                machineLoadingUnitsWebService,
                machineModeWebService,
                bayManagerService,
                machineExternalBayWebService)
        {
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
        }

        #endregion

        #region Properties

        public ICommand ConfirmEjectLoadingUnitCommand =>
        this.confirmEjectLoadingUnitCommand
        ??
        (this.confirmEjectLoadingUnitCommand = new DelegateCommand(
            async () => await this.ConfirmEjectLoadingUnit(),
                        this.CanConfirmEjectLoadingUnit));

        #endregion

        #region Methods

        public override bool CanSelectBayPositionDown()
        {
            return !this.IsExecutingProcedure &&
                   this.IsPositionUpSelected &&
                   (this.MachineStatus.LoadingUnitPositionUpInBay is null ||
                    this.MachineStatus.LoadingUnitPositionDownInBay is null);
        }

        public override bool CanSelectBayPositionUp()
        {
            return !this.IsExecutingProcedure &&
                   !this.IsPositionUpSelected &&
                   (this.MachineStatus.LoadingUnitPositionUpInBay is null ||
                    this.MachineStatus.LoadingUnitPositionDownInBay is null);
        }

        public override bool CanStart()
        {
            switch (this.MachineService.BayNumber)
            {
                case BayNumber.BayOne:
                default:
                    return base.CanStart() &&
                   this.LoadingUnitId.HasValue &&
                   this.MachineModeService.MachineMode == MachineMode.Manual &&
                   this.MachineService.Loadunits.Any(f => f.Id == this.LoadingUnitId && f.Status == LoadingUnitStatus.InLocation) &&
                   this.MachineService.Bay.Positions.Any(p => !p.IsBlocked) &&
                   (this.MachineStatus.LoadingUnitPositionUpInBay is null ||
                    (!this.MachineService.HasCarousel && this.MachineStatus.LoadingUnitPositionDownInBay is null));

                case BayNumber.BayTwo:
                    return base.CanStart() &&
                   this.LoadingUnitId.HasValue &&
                   this.MachineModeService.MachineMode == MachineMode.Manual2 &&
                   this.MachineService.Loadunits.Any(f => f.Id == this.LoadingUnitId && f.Status == LoadingUnitStatus.InLocation) &&
                   this.MachineService.Bay.Positions.Any(p => !p.IsBlocked) &&
                   (this.MachineStatus.LoadingUnitPositionUpInBay is null ||
                    (!this.MachineService.HasCarousel && this.MachineStatus.LoadingUnitPositionDownInBay is null));

                case BayNumber.BayThree:
                    return base.CanStart() &&
                   this.LoadingUnitId.HasValue &&
                   this.MachineModeService.MachineMode == MachineMode.Manual3 &&
                   this.MachineService.Loadunits.Any(f => f.Id == this.LoadingUnitId && f.Status == LoadingUnitStatus.InLocation) &&
                   this.MachineService.Bay.Positions.Any(p => !p.IsBlocked) &&
                   (this.MachineStatus.LoadingUnitPositionUpInBay is null ||
                    (!this.MachineService.HasCarousel && this.MachineStatus.LoadingUnitPositionDownInBay is null));
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.LoadingUnitId = null;

            await base.OnAppearedAsync();
        }

        public override async Task StartAsync()
        {
            try
            {
                if (!this.IsLoadingUnitIdValid)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.InvalidEnteredDrawerId"), Services.Models.NotificationSeverity.Warning);
                    return;
                }

                var destination = this.GetLoadingUnitSource(this.IsPositionDownSelected);

                if (destination == LoadingUnitLocation.NoLocation)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.InvalidSourceChoiceType"), Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;

                //await this.MachineLoadingUnitsWebService.EjectLoadingUnitAsync(destination, this.LoadingUnitId.Value);
                await this.MachineLoadingUnitsWebService.StartMovingLoadingUnitToBayAsync(this.LoadingUnitId.Value, destination);
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

            // this.isEjectLoadingUnitConfirmationEnabled = true;

            this.RaiseCanExecuteChanged();
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.SensorsService.RefreshAsync(true);

            if (this.MachineService.Bay.Positions.Any(p => p.IsUpper && !p.IsBlocked) &&
                (this.MachineService.BayFirstPositionIsUpper || this.MachineService.HasCarousel)
                )
            {
                this.SelectBayPositionUp();
            }
            else
            {
                this.SelectBayPositionDown();
            }
        }

        protected override void OnWaitResume()
        {
            // this.isEjectLoadingUnitConfirmationEnabled = true;

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.confirmEjectLoadingUnitCommand?.RaiseCanExecuteChanged();
        }

        private bool CanConfirmEjectLoadingUnit()
        {
            return !this.IsMoving &&
                   ((this.MachineStatus.LoadingUnitPositionUpInBay != null && this.IsPositionUpSelected) ||
                    (this.MachineStatus.LoadingUnitPositionDownInBay != null && this.IsPositionDownSelected));
        }

        private async Task ConfirmEjectLoadingUnit()
        {
            try
            {
                var lu = 0;

                if (this.IsPositionUpSelected)
                {
                    lu = this.MachineStatus.LoadingUnitPositionUpInBay.Id;
                }
                else
                {
                    lu = this.MachineStatus.LoadingUnitPositionDownInBay.Id;
                }

                await this.machineBaysWebService.RemoveLoadUnitAsync(lu);

                var refreshTask = this.SensorsService.RefreshAsync(true);

                var updateTask = this.MachineService.OnUpdateServiceAsync();

                this.ShowNotification(string.Format(Localized.Get("(InstallationApp.DrawerIdExtracted"), lu), Services.Models.NotificationSeverity.Warning);

                await refreshTask;

                await updateTask;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}

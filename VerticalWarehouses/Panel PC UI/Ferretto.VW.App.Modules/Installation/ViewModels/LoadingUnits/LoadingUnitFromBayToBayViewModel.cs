using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Resources;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class LoadingUnitFromBayToBayViewModel : BaseMovementsViewModel
    {
        #region Fields

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly Services.ISensorsService sensorsService;

        private DelegateCommand confirmEjectLoadingUnitCommand;

        private bool isBay1Destination;

        private bool isBay1Present;

        private bool isBay2Destination;

        private bool isBay2Present;

        private bool isBay3Destination;

        private bool isBay3Present;

        private bool isEjectLoadingUnitConfirmationEnabled;

        private IEnumerable<LoadingUnit> loadingUnits;

        private DelegateCommand sendToBay1Command;

        private DelegateCommand sendToBay2Command;

        private DelegateCommand sendToBay3Command;

        private DelegateCommand startToBayCommand;

        #endregion

        #region Constructors

        public LoadingUnitFromBayToBayViewModel(
            IMachineBaysWebService machineBaysWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            Services.ISensorsService sensorsService,
            Services.IBayManager bayManagerService,
            IMachineModeWebService machineModeWebService,
            IMachineExternalBayWebService machineExternalBayWebService)
            : base(
                machineLoadingUnitsWebService,
                machineModeWebService,
                bayManagerService,
                machineExternalBayWebService)
        {
            this.machineBaysWebService = machineBaysWebService ?? throw new System.ArgumentNullException(nameof(machineBaysWebService));
            this.sensorsService = sensorsService ?? throw new ArgumentNullException(nameof(sensorsService));
        }

        #endregion

        #region Properties

        public ICommand ConfirmEjectLoadingUnitCommand =>
                this.confirmEjectLoadingUnitCommand
                ??
                (this.confirmEjectLoadingUnitCommand = new DelegateCommand(async () => await this.ConfirmEjectLoadingUnit(), this.CanConfirmEjectLoadingUnit));

        public bool IsBay1Destination
        {
            get => this.isBay1Destination;
            set => this.SetProperty(ref this.isBay1Destination, value);
        }

        public bool IsBay1Present
        {
            get => this.isBay1Present;
            set => this.SetProperty(ref this.isBay1Present, value);
        }

        public bool IsBay2Destination
        {
            get => this.isBay2Destination;
            set => this.SetProperty(ref this.isBay2Destination, value);
        }

        public bool IsBay2Present
        {
            get => this.isBay2Present;
            set => this.SetProperty(ref this.isBay2Present, value);
        }

        public bool IsBay3Destination
        {
            get => this.isBay3Destination;
            set => this.SetProperty(ref this.isBay3Destination, value);
        }

        public bool IsBay3Present
        {
            get => this.isBay3Present;
            set => this.SetProperty(ref this.isBay3Present, value);
        }

        public ICommand SendToBay1Command =>
            this.sendToBay1Command
            ??
            (this.sendToBay1Command = new DelegateCommand(
                () => this.ChangeDestination(BayNumber.BayOne),
                () => !this.IsBay1Destination && !this.IsExecutingProcedure && !this.IsWaitingForResponse));

        public ICommand SendToBay2Command =>
            this.sendToBay2Command
            ??
            (this.sendToBay2Command = new DelegateCommand(
                () => this.ChangeDestination(BayNumber.BayTwo),
                () => !this.IsBay2Destination && !this.IsExecutingProcedure && !this.IsWaitingForResponse));

        public ICommand SendToBay3Command =>
            this.sendToBay3Command
            ??
            (this.sendToBay3Command = new DelegateCommand(
                () => this.ChangeDestination(BayNumber.BayThree),
                () => !this.IsBay3Destination && !this.IsExecutingProcedure && !this.IsWaitingForResponse));

        public ICommand StartToBayCommand =>
            this.startToBayCommand
            ??
            (this.startToBayCommand = new DelegateCommand(
                async () => await this.StartToBayAsync(),
                () => !this.IsExecutingProcedure &&
                      (!string.IsNullOrEmpty(this.MachineStatus.LoadingUnitPositionUpInBay?.Id.ToString()) ||
                       !string.IsNullOrEmpty(this.MachineStatus.LoadingUnitPositionDownInBay?.Id.ToString()))));

        #endregion

        #region Methods

        public override bool CanStart()
        {
            switch (this.MachineService.BayNumber)
            {
                case BayNumber.BayOne:
                default:
                    return base.CanStart() &&
                        this.MachineModeService.MachineMode == MachineMode.Manual;

                case BayNumber.BayTwo:
                    return base.CanStart() &&
                        this.MachineModeService.MachineMode == MachineMode.Manual2;

                case BayNumber.BayThree:
                    return base.CanStart() &&
                        this.MachineModeService.MachineMode == MachineMode.Manual3;
            }
        }

        public async Task GetLoadingUnits()
        {
            try
            {
                this.loadingUnits = await this.MachineLoadingUnitsWebService.GetAllAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
        }

        public async Task StartToBayAsync()
        {
            try
            {
                var source = this.GetLoadingUnitSource(!(this.MachineStatus.LoadingUnitPositionUpInBay != null));
                if (source == LoadingUnitLocation.NoLocation)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.InvalidSourceChoiceType"), Services.Models.NotificationSeverity.Warning);
                    return;
                }

                var loadingUnit = this.MachineStatus.LoadingUnitPositionUpInBay != null ?
                                  this.MachineStatus.LoadingUnitPositionUpInBay?.Id.ToString() :
                                  this.MachineStatus.LoadingUnitPositionDownInBay?.Id.ToString();
                var id = 0;
                int.TryParse(loadingUnit, out id);

                this.LoadingUnitId = this.loadingUnits.FirstOrDefault(f => f.Id == id)?.Id;

                if (this.LoadingUnitId is null)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.InvalidDrawerIdInBay"), Services.Models.NotificationSeverity.Warning);
                    return;
                }

                var bay = this.IsBay1Destination ? BayNumber.BayOne : this.IsBay2Destination ? BayNumber.BayTwo : BayNumber.BayThree;
                var destination = this.GetLoadingUnitSourceByDestination(bay, this.IsPositionDownSelected);

                if (destination == LoadingUnitLocation.NoLocation)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.InvalidDestinationChoiceType"), Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;

                this.isEjectLoadingUnitConfirmationEnabled = false;

                this.confirmEjectLoadingUnitCommand?.RaiseCanExecuteChanged();

                await this.MachineLoadingUnitsWebService.StartMovingSourceDestinationAsync(source, destination, null, null);
            }
            catch (Exception ex)
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

            this.isEjectLoadingUnitConfirmationEnabled = false;

            this.confirmEjectLoadingUnitCommand?.RaiseCanExecuteChanged();

            this.GetLoadingUnits().ConfigureAwait(false);
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.InitializingData();

            await this.SensorsService.RefreshAsync(true);

            await this.SetDataBays()
                .ContinueWith((m) => this.RaiseCanExecuteChanged());
        }

        protected override void OnWaitResume()
        {
            this.RaiseCanExecuteChanged();

            this.isEjectLoadingUnitConfirmationEnabled = true;

            this.confirmEjectLoadingUnitCommand?.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.sendToBay1Command?.RaiseCanExecuteChanged();
            this.sendToBay2Command?.RaiseCanExecuteChanged();
            this.sendToBay3Command?.RaiseCanExecuteChanged();
            this.startToBayCommand?.RaiseCanExecuteChanged();
            this.confirmEjectLoadingUnitCommand?.RaiseCanExecuteChanged();
        }

        private bool CanConfirmEjectLoadingUnit()
        {
            return this.isEjectLoadingUnitConfirmationEnabled;
        }

        private void ChangeDestination(BayNumber bayDestination)
        {
            if (bayDestination == BayNumber.BayOne)
            {
                this.IsBay1Destination = true;
                this.IsBay2Destination = false;
                this.IsBay3Destination = false;
            }
            else if (bayDestination == BayNumber.BayTwo)
            {
                this.IsBay1Destination = false;
                this.IsBay2Destination = true;
                this.IsBay3Destination = false;
            }
            else
            {
                this.IsBay1Destination = false;
                this.IsBay2Destination = false;
                this.IsBay3Destination = true;
            }

            this.RaiseCanExecuteChanged();
        }

        private async Task ConfirmEjectLoadingUnit()
        {
            await this.MachineLoadingUnitsWebService.ResumeAsync(this.CurrentMissionId, this.MachineService.BayNumber);
        }

        private async Task InitializingData()
        {
            await this.GetLoadingUnits();

            if (this.MachineService.Bay.Positions.Any(p => p.IsUpper && !p.IsBlocked) &&
                (this.MachineService.Bay.IsDouble || this.MachineService.BayFirstPositionIsUpper)
                )
            {
                this.SelectBayPositionUp();
            }
            else
            {
                this.SelectBayPositionDown();
            }
        }

        private async Task SetDataBays()
        {
            var bays = await this.machineBaysWebService.GetAllAsync();

            this.IsBay1Present = bays.Any(b => b.Number == BayNumber.BayOne);
            this.IsBay2Present = bays.Any(b => b.Number == BayNumber.BayTwo);
            this.IsBay3Present = bays.Any(b => b.Number == BayNumber.BayThree);

            var lst = new List<bool>() { this.IsBay1Present, this.IsBay2Present, this.IsBay3Present };
            if (lst.Count(a => a) == 1)
            {
                if (this.IsBay1Present)
                {
                    this.ChangeDestination(BayNumber.BayOne);
                }

                if (this.IsBay2Present)
                {
                    this.ChangeDestination(BayNumber.BayTwo);
                }

                if (this.IsBay3Present)
                {
                    this.ChangeDestination(BayNumber.BayThree);
                }
            }
            else
            {
                if (this.MachineService.BayNumber == BayNumber.BayOne)
                {
                    this.ChangeDestination(BayNumber.BayOne);
                }

                if (this.MachineService.BayNumber == BayNumber.BayTwo)
                {
                    this.ChangeDestination(BayNumber.BayTwo);
                }

                if (this.MachineService.BayNumber == BayNumber.BayThree)
                {
                    this.ChangeDestination(BayNumber.BayThree);
                }
            }
        }

        #endregion
    }
}

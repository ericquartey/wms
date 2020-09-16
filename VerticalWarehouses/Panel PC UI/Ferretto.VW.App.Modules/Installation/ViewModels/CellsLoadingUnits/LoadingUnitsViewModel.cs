using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class LoadingUnitsViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly Services.IDialogService dialogService;

        private readonly IHealthProbeService healthProbeService;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly ISessionService sessionService;

        private string currentError;

        private bool error = false;

        private DelegateCommand freeDrawerCommand;

        private DelegateCommand immediateDrawerCallCommand;

        private DelegateCommand immediateDrawerReturnCommand;

        private DelegateCommand insertDrawerCommand;

        private SubscriptionToken loadunitsToken;

        private DelegateCommand removeDrawerCommand;

        private DelegateCommand saveDrawerCommand;

        private int? selectedBayPositionId;

        private string selectedCode;

        private int? selectedId;

        private LoadingUnit selectedLU;

        private DelegateCommand stopMovingCommand;

        #endregion

        #region Constructors

        public LoadingUnitsViewModel(
            Services.IDialogService dialogService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineBaysWebService machineBaysWebService,
            IHealthProbeService healthProbeService,
            ISessionService sessionService)
            : base(PresentationMode.Installer)
        {
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public string Error => string.Join(
            this[nameof(this.SelectedId)],
            this[nameof(this.SelectedCode)]);

        public ICommand FreeDrawerCommand =>
            this.freeDrawerCommand
            ??
            (this.freeDrawerCommand = new DelegateCommand(
                async () => await this.FreeDrawer(),
                () => false));

        public ICommand ImmediateDrawerCallCommand =>
            this.immediateDrawerCallCommand
            ??
            (this.immediateDrawerCallCommand = new DelegateCommand(
                async () => await this.ImmediateDrawerCallAsync(),
                () => !this.IsMoving && !this.SensorsService.IsLoadingUnitInBay && this.SelectedLU != null && this.SelectedLU.Status == LoadingUnitStatus.InLocation));

        public ICommand ImmediateDrawerReturnCommand =>
            this.immediateDrawerReturnCommand
            ??
            (this.immediateDrawerReturnCommand = new DelegateCommand(
                async () => await this.ImmediateDrawerReturnAsync(),
                () => !this.IsMoving && this.SensorsService.IsLoadingUnitInBay && this.MachineStatus.LoadingUnitPositionUpInBay != null));

        public ICommand InsertDrawerCommand =>
            this.insertDrawerCommand
            ??
            (this.insertDrawerCommand = new DelegateCommand(
                async () => await this.InsertDrawer(),
                () => false));

        public bool IsDrawerCallVisible => !this.IsMoving && !this.SensorsService.IsLoadingUnitInBay;

        public bool IsDrawerReturnVisible => !this.IsMoving && this.SensorsService.IsLoadingUnitInBay;

        public bool IsEditStatus => this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

        public bool IsEnabledEditing => !this.IsMoving;

        public IEnumerable<LoadingUnit> LoadingUnits => this.MachineService.Loadunits;

        public ICommand RemoveDrawerCommand =>
            this.removeDrawerCommand
            ??
            (this.removeDrawerCommand = new DelegateCommand(
                async () => await this.RemoveDrawer(),
                () => !this.IsMoving &&
                      this.SelectedLU != null));

        public ICommand SaveDrawerCommand =>
            this.saveDrawerCommand
            ??
            (this.saveDrawerCommand = new DelegateCommand(
                async () => await this.SaveDrawerAsync(),
                () => !this.IsMoving && this.SelectedLU != null));

        public int? SelectedBayPositionId
        {
            get => this.selectedBayPositionId;
            set => this.SetProperty(ref this.selectedBayPositionId, value);
        }

        public string SelectedCode
        {
            get => this.selectedCode;
            set => this.SetProperty(ref this.selectedCode, value);
        }

        public int? SelectedId
        {
            get => this.selectedId;
            set => this.SetProperty(ref this.selectedId, value);
        }

        public LoadingUnit SelectedLU
        {
            get => this.selectedLU;
            set => this.SetProperty(ref this.selectedLU, value, () =>
            {
                this.SelectedId = this.SelectedLU?.Id;
                this.SelectedCode = this.SelectedLU?.Code;
                this.SelectedBayPositionId = null;
            });
        }

        public IEnumerable<LoadingUnitStatus> Status => Enum.GetValues(typeof(LoadingUnitStatus)).OfType<LoadingUnitStatus>().ToList();

        public ICommand StopMovingCommand =>
            this.stopMovingCommand
            ??
            (this.stopMovingCommand = new DelegateCommand(
                async () => await this.MachineService.StopMovingByAllAsync(),
                () => this.IsMoving));

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                this.currentError = null;

                if (this.IsWaitingForResponse)
                {
                    return null;
                }

                switch (columnName)
                {
                    case nameof(this.SelectedId):
                        break;

                    case nameof(this.SelectedCode):
                        break;
                }

                if (this.IsVisible && string.IsNullOrEmpty(this.currentError))
                {
                    //this.ClearNotifications();
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.loadunitsToken != null)
            {
                this.EventAggregator.GetEvent<LoadUnitsChangedPubSubEvent>().Unsubscribe(this.loadunitsToken);
                this.loadunitsToken.Dispose();
                this.loadunitsToken = null;
            }
        }

        public async Task ImmediateDrawerCallAsync()
        {
            try
            {
                await this.machineLoadingUnitsWebService.StartMovingLoadingUnitToBayAsync(this.SelectedLU.Id, this.GetBayPosition());
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
        }

        public async Task ImmediateDrawerReturnAsync()
        {
            try
            {
                await this.machineLoadingUnitsWebService.InsertLoadingUnitAsync(this.GetBayPosition(), null, this.MachineStatus.LoadingUnitPositionUpInBay.Id);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            if (this.LoadingUnits.Any())
            {
                this.SelectedLU = this.LoadingUnits?.ToList()[0];
            }

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.stopMovingCommand?.RaiseCanExecuteChanged();
            this.saveDrawerCommand?.RaiseCanExecuteChanged();
            this.freeDrawerCommand?.RaiseCanExecuteChanged();
            this.insertDrawerCommand?.RaiseCanExecuteChanged();
            this.removeDrawerCommand?.RaiseCanExecuteChanged();
            this.immediateDrawerCallCommand?.RaiseCanExecuteChanged();
            this.immediateDrawerReturnCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsDrawerReturnVisible));
            this.RaisePropertyChanged(nameof(this.IsDrawerCallVisible));
            this.RaisePropertyChanged(nameof(this.LoadingUnits));
            this.RaisePropertyChanged(nameof(this.IsEnabledEditing));
            this.RaisePropertyChanged(nameof(this.Status));
            this.RaisePropertyChanged(nameof(this.IsEditStatus));
        }

        private async Task FreeDrawer()
        {
            throw new NotImplementedException();
        }

        private MAS.AutomationService.Contracts.LoadingUnitLocation GetBayPosition()
        {
            if (this.MachineService.BayNumber == MAS.AutomationService.Contracts.BayNumber.BayOne)
            {
                return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay1Up;
            }

            if (this.MachineService.BayNumber == MAS.AutomationService.Contracts.BayNumber.BayTwo)
            {
                return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay2Up;
            }

            if (this.MachineService.BayNumber == MAS.AutomationService.Contracts.BayNumber.BayThree)
            {
                return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay3Up;
            }

            return MAS.AutomationService.Contracts.LoadingUnitLocation.NoLocation;
        }

        private async Task InsertDrawer()
        {
            throw new NotImplementedException();
        }

        private async Task RemoveDrawer()
        {
            try
            {
                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmationOperation"), Localized.Get("InstallationApp.DeleteUnit"), DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    await this.machineLoadingUnitsWebService.RemoveLoadUnitAsync(this.SelectedLU.Id);

                    await this.MachineService.GetLoadUnits();
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task SaveDrawerAsync()
        {
            try
            {
                await this.machineLoadingUnitsWebService.SaveLoadUnitAsync(this.SelectedLU);

                if (this.SelectedLU.Status == LoadingUnitStatus.InBay && this.SelectedBayPositionId.HasValue)
                {
                    await this.machineBaysWebService.SetLoadUnitOnBayAsync(this.SelectedBayPositionId.Value, this.SelectedLU.Id);
                }

                if (this.SelectedLU.Status == LoadingUnitStatus.InElevator)
                {
                    await this.machineElevatorWebService.SetLoadUnitOnElevatorAsync(this.SelectedLU.Id);
                }

                if (this.error)
                {
                    this.ClearNotifications();
                    this.error = false;
                }
                else
                {
                    await this.MachineService.GetLoadUnits();
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.error = true;
                this.ShowNotification(ex);
            }
            finally
            {
                await this.MachineService.GetLoadUnits();
            }
        }

        private void SubscribeToEvents()
        {
            this.loadunitsToken = this.loadunitsToken
                 ??
                 this.EventAggregator
                     .GetEvent<LoadUnitsChangedPubSubEvent>()
                     .Subscribe(
                         m => this.RaiseCanExecuteChanged(),
                         ThreadOption.UIThread,
                         false);
        }

        #endregion
    }
}

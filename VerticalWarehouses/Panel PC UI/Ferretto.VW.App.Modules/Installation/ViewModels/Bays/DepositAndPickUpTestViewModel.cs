using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum DepositAndPickUpStep
    {
        CallUnit,

        CycleTest,

        EndTest,
    }

    [Warning(WarningsArea.Installation)]
    public class DepositAndPickUpTestViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly Services.IDialogService dialogService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineProfileProcedureWebService machineProfileProcedureWeb;

        private readonly IMachineShuttersWebService shuttersWebService;

        private DelegateCommand callLoadunitToBayCommand;

        private bool canLoadingUnitId;

        private DelegateCommand completeCommand;

        private DepositAndPickUpStep currentStep;

        private bool isCalibrationCompletedOrStopped;

        private bool isCalibrationNotCompleted;

        private bool isChainOffsetVisible;

        private bool isExecutingProcedure;

        private bool isNewErrorValueVisible;

        private bool isTuningChain;

        private int? loadingUnitId;

        private double? measuredDistance;

        private DelegateCommand moveToCycleTestCommand;

        private DelegateCommand moveToEndTestCommand;

        private int? newErrorValue;

        private SubscriptionToken profileCalibrationToken;

        private int? requiredCycles;

        private DelegateCommand startCycleCommand;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        private SubscriptionToken themeChangedToken;

        #endregion

        #region Constructors

        public DepositAndPickUpTestViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineShuttersWebService shuttersWebService,
            IMachineProfileProcedureWebService machineProfileProcedureWeb)
            : base(PresentationMode.Installer)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.shuttersWebService = shuttersWebService ?? throw new ArgumentNullException(nameof(shuttersWebService));
            this.machineProfileProcedureWeb = machineProfileProcedureWeb ?? throw new ArgumentNullException(nameof(machineProfileProcedureWeb));

            this.CurrentStep = DepositAndPickUpStep.CallUnit;
        }

        #endregion

        #region Properties

        public ICommand CallLoadunitToBayCommand =>
            this.callLoadunitToBayCommand
            ??
            (this.callLoadunitToBayCommand = new DelegateCommand(
                async () => await this.CallLoadunitToBayCommandAsync(),
                this.CanCallLoadunitToBay));

        public bool CanLoadingUnitId
        {
            get => this.canLoadingUnitId;
            private set => this.SetProperty(ref this.canLoadingUnitId, value);
        }

        public double? ChainOffset => Math.Abs(this.MachineService.Bay.ChainOffset);

        public ICommand CompleteCommand =>
                    this.completeCommand
            ??
            (this.completeCommand = new DelegateCommand(
                async () => await this.CompleteAsync(), this.CanComplete));

        public DepositAndPickUpStep CurrentStep
        {
            get => this.currentStep;
            protected set => this.SetProperty(ref this.currentStep, value, () => this.UpdateStatusButtonFooter(false));
        }

        public string Error => this[nameof(this.NewErrorValue)];

        public bool HasBayExternal => this.MachineService.HasBayExternal;

        public bool HasStepCallUnit => this.currentStep is DepositAndPickUpStep.CallUnit;

        public bool HasStepCycleTest => this.currentStep is DepositAndPickUpStep.CycleTest;

        public bool HasStepEndTest => this.currentStep is DepositAndPickUpStep.EndTest;

        public bool IsCalibrationCompletedOrStopped
        {
            get => this.isCalibrationCompletedOrStopped;
            private set
            {
                if (this.SetProperty(ref this.isCalibrationCompletedOrStopped, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsCalibrationNotCompleted
        {
            get => this.isCalibrationNotCompleted;
            set => this.SetProperty(ref this.isCalibrationNotCompleted, value);
        }

        public bool IsChainOffsetVisible
        {
            get => this.isChainOffsetVisible;
            set => this.SetProperty(ref this.isChainOffsetVisible, value);
        }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            set => this.SetProperty(ref this.isExecutingProcedure, value);
        }

        public bool IsMoving => (this.MachineService?.MachineStatus?.IsMoving ?? true) || (this.MachineService?.MachineStatus?.IsMovingLoadingUnit ?? true);

        public bool IsNewErrorValueVisible
        {
            get => this.isNewErrorValueVisible;
            set => this.SetProperty(ref this.isNewErrorValueVisible, value);
        }

        public bool IsTuningChain
        {
            get => this.isTuningChain;
            private set => this.SetProperty(ref this.isTuningChain, value);
        }

        public int? LoadingUnitId
        {
            get => this.loadingUnitId;
            set => this.SetProperty(ref this.loadingUnitId, value, this.RaiseCanExecuteChanged);
        }

        public double? MeasuredDistance
        {
            get => this.measuredDistance;
            protected set => this.SetProperty(ref this.measuredDistance, value);
        }

        public ICommand MoveToCycleTest =>
           this.moveToCycleTestCommand
           ??
           (this.moveToCycleTestCommand = new DelegateCommand(
               () => this.CurrentStep = DepositAndPickUpStep.CycleTest,
               this.CanMoveToCycleTest));

        public ICommand MoveToEndTest =>
           this.moveToEndTestCommand
           ??
           (this.moveToEndTestCommand = new DelegateCommand(
               () => this.CurrentStep = DepositAndPickUpStep.EndTest,
               this.CanMoveToEndTest));

        public int? NewErrorValue
        {
            get => this.newErrorValue;
            set => this.SetProperty(ref this.newErrorValue, value);
        }

        public int? RequiredCycles
        {
            get => this.requiredCycles;
            set => this.SetProperty(ref this.requiredCycles, value, () => this.startCycleCommand?.RaiseCanExecuteChanged());
        }

        public ICommand StartCycleCommand =>
           this.startCycleCommand
           ??
           (this.startCycleCommand = new DelegateCommand(
               async () => await this.StartCycleAsync(),
               this.CanCycleStart));

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        protected Carousel ProcedureParameters { get; private set; }

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                if (this.IsWaitingForResponse)
                {
                    return null;
                }

                switch (columnName)
                {
                    case nameof(this.NewErrorValue):
                        if (!this.NewErrorValue.HasValue)
                        {
                            return InstallationApp.MissValue;
                        }

                        if (this.NewErrorValue.HasValue && this.NewErrorValue < 1)
                        {
                            return InstallationApp.DataBePositive;
                        }

                        if (this.NewErrorValue.HasValue && this.NewErrorValue > 9)
                        {
                            return InstallationApp.MaxValue9;
                        }

                        break;

                    case nameof(this.LoadingUnitId):
                        if (!this.LoadingUnitId.HasValue ||
                            (!this.MachineService.Loadunits.DrawerInLocationById(this.LoadingUnitId.Value) &&
                             !this.MachineService.Loadunits.DrawerInBayById(this.LoadingUnitId.Value)))
                        {
                            return VW.App.Resources.InstallationApp.InvalidDrawerSelected;
                        }

                        break;
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.stepChangedToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.stepChangedToken);
                this.stepChangedToken.Dispose();
                this.stepChangedToken = null;
            }

            if (this.profileCalibrationToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<ProfileCalibrationMessageData>>().Unsubscribe(this.profileCalibrationToken);
                this.profileCalibrationToken?.Dispose();
                this.profileCalibrationToken = null;
            }

            if (this.themeChangedToken != null)
            {
                this.EventAggregator.GetEvent<ThemeChangedPubSubEvent>().Unsubscribe(this.themeChangedToken);
                this.themeChangedToken?.Dispose();
                this.themeChangedToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            if ((this.MachineService.Bay.IsDouble && this.MachineStatus.LoadingUnitPositionUpInBay != null) ||
                (!this.MachineService.Bay.IsDouble && ((this.MachineService.BayFirstPositionIsUpper && this.MachineStatus.LoadingUnitPositionUpInBay != null) ||
                                                       (!this.MachineService.BayFirstPositionIsUpper && this.MachineStatus.LoadingUnitPositionDownInBay != null))))
            {
                if (this.MachineStatus.LoadingUnitPositionUpInBay != null)
                {
                    this.LoadingUnitId = this.MachineStatus.LoadingUnitPositionUpInBay.Id;
                }
                else
                {
                    this.LoadingUnitId = this.MachineStatus.LoadingUnitPositionDownInBay.Id;
                }
            }
            else
            {
                this.LoadingUnitId = 1;
            }

            this.UpdateStatusButtonFooter();

            await base.OnAppearedAsync();
        }

        public async Task RetrieveProcedureInformationAsync()
        {
            try
            {
                var procedureParameters = await this.machineCarouselWebService.GetParametersAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override async Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            await base.OnMachineStatusChangedAsync(e);

            if (!this.IsMoving)
            {
                this.IsTuningChain = false;
            }
        }

        protected void OnStepChanged(StepChangedMessage e)
        {
            switch (this.CurrentStep)
            {
                case DepositAndPickUpStep.CallUnit:
                    if (e.Next)
                    {
                        this.CurrentStep = DepositAndPickUpStep.CycleTest;
                    }

                    break;

                case DepositAndPickUpStep.CycleTest:
                    if (e.Next)
                    {
                        this.CurrentStep = DepositAndPickUpStep.EndTest;
                    }
                    else
                    {
                        this.CurrentStep = DepositAndPickUpStep.CallUnit;
                    }

                    break;

                case DepositAndPickUpStep.EndTest:
                    if (!e.Next)
                    {
                        this.CurrentStep = DepositAndPickUpStep.CallUnit;
                    }

                    break;

                default:
                    break;
            }

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.CanLoadingUnitId = this.CanBaseExecute() &&
                                    !this.SensorsService.IsLoadingUnitInBay;

            this.startCycleCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();

            this.callLoadunitToBayCommand?.RaiseCanExecuteChanged();
            this.moveToCycleTestCommand?.RaiseCanExecuteChanged();

            this.completeCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.NewErrorValue));
            this.RaisePropertyChanged(nameof(this.ChainOffset));
        }

        private async Task CallLoadunitToBayCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.EjectLoadingUnitAsync(this.MachineService.GetBayPositionSourceByDestination(false), this.LoadingUnitId.Value); ;
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

        private bool CanBaseExecute()
        {
            return !this.IsKeyboardOpened &&
                   !this.IsMoving &&
                   !this.SensorsService.IsHorizontalInconsistentBothLow &&
                   !this.SensorsService.IsHorizontalInconsistentBothHigh;
        }

        private bool CanCallLoadunitToBay()
        {
            return this.CanBaseExecute() &&
                   !this.SensorsService.IsLoadingUnitInBay &&
                   !this.MachineService.Loadunits.DrawerInBay() &&
                   string.IsNullOrEmpty(this.Error);
        }

        private bool CanComplete()
        {
            return this.CanBaseExecute();
        }

        private bool CanCycleStart()
        {
            return !this.IsMoving &&
                   this.RequiredCycles.HasValue;
        }

        private bool CanMoveToCycleTest()
        {
            return this.CanBaseExecute() &&
                   this.SensorsService.IsLoadingUnitInBay &&
                   (string.IsNullOrEmpty(this.Error) || !this.MachineService.Loadunits.Any());
        }

        private bool CanMoveToEndTest()
        {
            return this.CanBaseExecute() &&
                   this.SensorsService.IsLoadingUnitInBay &&
                   (string.IsNullOrEmpty(this.Error) || !this.MachineService.Loadunits.Any());
        }

        private bool CanStop()
        {
            return
                this.IsMoving;
        }

        private async Task CompleteAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.ConfirmCalibrationProcedure, InstallationApp.HorizontalCalibration, DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    await this.machineCarouselWebService.SetCalibrationCompletedAsync();

                    this.ShowNotification(
                            VW.App.Resources.InstallationApp.InformationSuccessfullyUpdated,
                            Services.Models.NotificationSeverity.Success);
                }

                this.NavigationService.GoBack();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.CurrentStep = DepositAndPickUpStep.CallUnit;
                this.IsWaitingForResponse = false;
            }
        }

        private void OnProfileCalibrationMessage(NotificationMessageUI<ProfileCalibrationMessageData> message)
        {
            var data = message.Data as ProfileCalibrationMessageData;

            if (message.IsErrored() ||
                this.MachineError != null)
            {
                this.ShowNotification(VW.App.Resources.InstallationApp.ProcedureWasStopped, Services.Models.NotificationSeverity.Warning);

                this.IsCalibrationCompletedOrStopped = false;

                this.IsChainOffsetVisible = true;
                this.IsNewErrorValueVisible = false;

                this.IsCalibrationNotCompleted = false;

                this.CurrentStep = DepositAndPickUpStep.EndTest;

                if (this.MachineError != null)
                {
                    this.IsChainOffsetVisible = false;
                    this.IsNewErrorValueVisible = false;
                    this.IsCalibrationNotCompleted = true;
                }

                return;
            }

            if (message.Status == MessageStatus.OperationEnd)
            {
                this.ShowNotification(VW.App.Resources.InstallationApp.CompletedTest, Services.Models.NotificationSeverity.Success);

                this.IsCalibrationNotCompleted = false;

                this.IsChainOffsetVisible = false;
                this.IsNewErrorValueVisible = true;

                this.IsCalibrationCompletedOrStopped = true;

                this.MeasuredDistance = data.Measured;

                this.NewErrorValue = 0;
                this.CurrentStep = DepositAndPickUpStep.EndTest;

                this.RaiseCanExecuteChanged();
            }

            if (message.Status == MessageStatus.OperationStop)
            {
                this.IsCalibrationNotCompleted = true;

                this.IsChainOffsetVisible = false;
                this.IsNewErrorValueVisible = false;

                this.ShowNotification(VW.App.Resources.InstallationApp.ProcedureWasStopped, Services.Models.NotificationSeverity.Warning);

                this.IsCalibrationCompletedOrStopped = false;
                this.NewErrorValue = 0;

                this.CurrentStep = DepositAndPickUpStep.EndTest;
                this.RaiseCanExecuteChanged();
            }
        }

        private async Task StartCycleAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StopAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.MachineService.StopMovingByAllAsync();

                this.IsExecutingProcedure = false;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;

                if (this.currentStep == DepositAndPickUpStep.CycleTest)
                {
                    this.CurrentStep = DepositAndPickUpStep.CallUnit;
                }
            }
        }

        private void SubscribeToEvents()
        {
            this.stepChangedToken = this.stepChangedToken
                ?? this.EventAggregator
                    .GetEvent<StepChangedPubSubEvent>()
                    .Subscribe(
                        (m) => this.OnStepChanged(m),
                        ThreadOption.UIThread,
                        false);

            this.profileCalibrationToken = this.profileCalibrationToken
                ?? this.EventAggregator
                    .GetEvent<NotificationEventUI<ProfileCalibrationMessageData>>()
                    .Subscribe(
                        (m) => this.OnProfileCalibrationMessage(m),
                        ThreadOption.UIThread,
                        false);

            this.themeChangedToken = this.themeChangedToken
               ?? this.EventAggregator
                   .GetEvent<ThemeChangedPubSubEvent>()
                   .Subscribe(
                       (m) =>
                       {
                           this.RaisePropertyChanged(nameof(this.HasStepEndTest));
                           this.RaisePropertyChanged(nameof(this.HasStepCycleTest));
                           this.RaisePropertyChanged(nameof(this.HasStepCallUnit));
                       },
                       ThreadOption.UIThread,
                       false);
        }

        private void UpdateStatusButtonFooter(bool force = false)
        {
            if (!this.IsVisible && !force)
            {
                return;
            }

            switch (this.CurrentStep)
            {
                case DepositAndPickUpStep.CallUnit:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, this.moveToCycleTestCommand?.CanExecute() ?? false);
                    break;

                case DepositAndPickUpStep.CycleTest:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, false);
                    break;

                case DepositAndPickUpStep.EndTest:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }

            this.ShowAbortStep(true, !this.IsMoving);

            this.RaisePropertyChanged(nameof(this.HasStepEndTest));
            this.RaisePropertyChanged(nameof(this.HasStepCycleTest));
            this.RaisePropertyChanged(nameof(this.HasStepCallUnit));
        }

        #endregion
    }
}

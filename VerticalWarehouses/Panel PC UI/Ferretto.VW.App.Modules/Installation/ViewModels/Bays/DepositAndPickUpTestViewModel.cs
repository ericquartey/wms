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

        OpenShutter,

        CycleTest,

        CloseShutter,

        EndTest,
    }

    [Warning(WarningsArea.Installation)]
    public class DepositAndPickUpTestViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly Services.IDialogService dialogService;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineDepositAndPickupProcedureWebService machineDepositAndPickupProcedureWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineEnduranceTestWebService machineEnduranceTestWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineShuttersWebService shuttersWebService;

        private DelegateCommand callLoadUnitToBayCommand;

        private bool canLoadingUnitId;

        private DelegateCommand closedShutterCommand;

        private DelegateCommand completeCommand;

        private DelegateCommand confirmCalibration;

        private DepositAndPickUpStep currentStep;

        private double? cyclesPercent;

        private bool isCalibrationCompletedOrStopped;

        private bool isCalibrationNotCompleted;

        private bool isChainOffsetVisible;

        //private bool isCompleted;

        private bool isExecutingProcedure;

        private bool isNewErrorValueVisible;

        private bool isShutterMoving;

        private bool isTuningChain;

        private int? loadingUnitId = 0;

        private double? measuredDistance;

        private DelegateCommand moveToCloseShutterCommand;

        private DelegateCommand moveToCycleTestCommand;

        private DelegateCommand moveToEndTestCommand;

        private DelegateCommand moveToOpenShutterCommand;

        private int? newErrorValue;

        private DelegateCommand openShutterCommand;

        private SubscriptionToken repetitiveHorizontalMovementsMessageReceivedToken;

        private int? requiredCycles;

        private DelegateCommand resetCommand;

        private bool resetCommandActive = false;

        private DelegateCommand returnCalibration;

        private DelegateCommand startCycleCommand;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        private DelegateCommand stopTestCommand;

        private SubscriptionToken themeChangedToken;

        private int? totalCompletedCycles;

        #endregion

        #region Constructors

        public DepositAndPickUpTestViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineShuttersWebService shuttersWebService,
            IMachineEnduranceTestWebService machineEnduranceTestWebService,
            IMachineCarouselWebService machineCarouselWebService,
            IMachineDepositAndPickupProcedureWebService machineDepositAndPickupProcedureWebService,
            IBayManager bayManager)
            : base(PresentationMode.Installer)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.shuttersWebService = shuttersWebService ?? throw new ArgumentNullException(nameof(shuttersWebService));
            this.machineEnduranceTestWebService = machineEnduranceTestWebService ?? throw new ArgumentNullException(nameof(machineEnduranceTestWebService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new ArgumentNullException(nameof(machineCarouselWebService));
            this.machineDepositAndPickupProcedureWebService = machineDepositAndPickupProcedureWebService ?? throw new ArgumentNullException(nameof(machineDepositAndPickupProcedureWebService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));

            this.CurrentStep = DepositAndPickUpStep.CallUnit;
        }

        #endregion

        #region Properties

        public bool BayIsShutterThreeSensors => this.MachineService.IsShutterThreeSensors;

        public ICommand CallLoadUnitToBayCommand =>
            this.callLoadUnitToBayCommand
            ??
            (this.callLoadUnitToBayCommand = new DelegateCommand(
                async () => await this.CallLoadUnitToBayCommandAsync(),
                this.CanCallLoadUnitToBay));

        public bool CanLoadingUnitId
        {
            get => this.canLoadingUnitId;
            private set => this.SetProperty(ref this.canLoadingUnitId, value);
        }

        public double? ChainOffset => Math.Abs(this.MachineService.Bay.ChainOffset);

        public ICommand ClosedShutterCommand =>
            this.closedShutterCommand
            ??
            (this.closedShutterCommand = new DelegateCommand(
                async () => await this.ClosedShutterAsync(),
                this.CanCloseShutter));

        public ICommand CompleteCommand =>
                    this.completeCommand
            ??
            (this.completeCommand = new DelegateCommand(
                async () => await this.CompleteAsync(), this.CanComplete));

        public ICommand ConfirmCalibration =>
            this.confirmCalibration
            ??
            (this.confirmCalibration = new DelegateCommand(
                async () =>
                {
                    try
                    {
                        this.NavigationService.GoBack();

                        this.CurrentStep = DepositAndPickUpStep.CallUnit;
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                }
                    ));

        public int? CumulativePerformedCycles
        {
            get => this.totalCompletedCycles;
            private set
            {
                if (this.SetProperty(ref this.totalCompletedCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public DepositAndPickUpStep CurrentStep
        {
            get => this.currentStep;
            protected set => this.SetProperty(ref this.currentStep, value, () => this.UpdateStatusButtonFooter(false));
        }

        public double? CyclesPercent
        {
            get => this.cyclesPercent;
            private set => this.SetProperty(ref this.cyclesPercent, value);
        }

        public string Error => string.Join(
            Environment.NewLine,
            this.GetType().GetProperties()
                .Select(p => this[p.Name])
                .Distinct()
                .Where(s => !string.IsNullOrEmpty(s)));

        public bool HasBayExternal => this.MachineService.HasBayExternal;

        public bool HasStepCallUnit => this.currentStep is DepositAndPickUpStep.CallUnit;

        public bool HasStepCloseShutter => this.currentStep is DepositAndPickUpStep.CloseShutter;

        public bool HasStepCycleTest => this.currentStep is DepositAndPickUpStep.CycleTest;

        public bool HasStepEndTest => this.currentStep is DepositAndPickUpStep.EndTest;

        public bool HasStepOpenShutter => this.currentStep is DepositAndPickUpStep.OpenShutter;

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

        public bool IsMovingTest => this.MachineService?.MachineStatus?.IsDepositAndPickUpRunning ?? true;

        public bool IsNewErrorValueVisible
        {
            get => this.isNewErrorValueVisible;
            set => this.SetProperty(ref this.isNewErrorValueVisible, value);
        }

        public bool IsShutterMoving
        {
            get => this.isShutterMoving;
            private set => this.SetProperty(ref this.isShutterMoving, value);
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

        public ICommand MoveToCloseShutter =>
           this.moveToCloseShutterCommand
           ??
           (this.moveToCloseShutterCommand = new DelegateCommand(
               () => this.CurrentStep = DepositAndPickUpStep.CloseShutter,
               this.CanMoveToCloseShutter));

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

        public ICommand MoveToOpenShutter =>
                           this.moveToOpenShutterCommand
           ??
           (this.moveToOpenShutterCommand = new DelegateCommand(
               () =>
               {
                   if (this.MachineService.HasShutter)
                   {
                       this.CurrentStep = DepositAndPickUpStep.OpenShutter;
                   }
                   else
                   {
                       this.CurrentStep = DepositAndPickUpStep.CycleTest;
                   }
               },
               this.CanMoveToOpenShutter));

        public int? NewErrorValue
        {
            get => this.newErrorValue;
            set => this.SetProperty(ref this.newErrorValue, value);
        }

        public ICommand OpenShutterCommand =>
            this.openShutterCommand
            ??
            (this.openShutterCommand = new DelegateCommand(
                async () => await this.OpenShutterAsync(),
                this.CanOpenShutter));

        public int? RequiredCycles
        {
            get => this.requiredCycles;
            set => this.SetProperty(ref this.requiredCycles, value, () => this.startCycleCommand?.RaiseCanExecuteChanged());
        }

        public ICommand ResetCommand =>
           this.resetCommand
           ??
           (this.resetCommand = new DelegateCommand(
               async () => await this.ResetCommandAsync(),
               this.CanExecuteResetCommand));

        public ICommand ReturnCalibration =>
                   this.returnCalibration
           ??
           (this.returnCalibration = new DelegateCommand(
               async () => await this.ReturnCalibrationAsync(),
               this.CanExecuteReturnCalibration));

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

        public ICommand StopTestCommand =>
           this.stopTestCommand
           ??
           (this.stopTestCommand = new DelegateCommand(
               async () => await this.StopTestAsync(),
               this.CanStopTest));

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
                            return Localized.Get("InstallationApp.MissValue");
                        }

                        if (this.NewErrorValue.HasValue && this.NewErrorValue < 1)
                        {
                            return Localized.Get("InstallationApp.DataBePositive");
                        }

                        if (this.NewErrorValue.HasValue && this.NewErrorValue > 9)
                        {
                            return Localized.Get("InstallationApp.MaxValue9");
                        }

                        break;

                    case nameof(this.LoadingUnitId):
                        if (!this.LoadingUnitId.HasValue ||
                            (!this.MachineService.Loadunits.DrawerInLocationById(this.LoadingUnitId.Value) &&
                             !this.MachineService.Loadunits.DrawerInElevatorById(this.LoadingUnitId.Value) &&
                             !this.MachineService.Loadunits.DrawerInBayById(this.LoadingUnitId.Value)))
                        {
                            return VW.App.Resources.Localized.Get("InstallationApp.InvalidDrawerSelected");
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

            if (this.repetitiveHorizontalMovementsMessageReceivedToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<RepetitiveHorizontalMovementsMessageData>>().Unsubscribe(this.repetitiveHorizontalMovementsMessageReceivedToken);
                this.repetitiveHorizontalMovementsMessageReceivedToken?.Dispose();
                this.repetitiveHorizontalMovementsMessageReceivedToken = null;
            }

            if (this.themeChangedToken != null)
            {
                this.EventAggregator.GetEvent<ThemeChangedPubSubEvent>().Unsubscribe(this.themeChangedToken);
                this.themeChangedToken?.Dispose();
                this.themeChangedToken = null;
            }
        }

        public async Task GetParameterValuesAsync()
        {
            if (this.requiredCycles == null || this.CumulativePerformedCycles == null)
            {
                var procedureParameters = await this.machineDepositAndPickupProcedureWebService.GetParametersAsync();
                this.requiredCycles = procedureParameters.RequiredCycles;
                this.CumulativePerformedCycles = procedureParameters.PerformedCycles;
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

            this.UpdateStatusButtonFooter(true);

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

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                await base.OnDataRefreshAsync();

                await this.SensorsService.RefreshAsync(true);

                await this.GetParameterValuesAsync();

                this.IsExecutingProcedure = this.MachineService.MachineStatus.IsMoving || this.MachineService.MachineMode == MachineMode.Test;

                //if (this.RequiredCycles == null || this.CumulativePerformedCycles == null)
                //{
                //    this.RequiredCycles = 200;
                //    this.CumulativePerformedCycles = 0;
                //}
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
                        this.CurrentStep = DepositAndPickUpStep.OpenShutter;
                    }

                    break;

                case DepositAndPickUpStep.OpenShutter:
                    if (e.Next)
                    {
                        this.CurrentStep = DepositAndPickUpStep.CycleTest;
                    }
                    else
                    {
                        this.CurrentStep = DepositAndPickUpStep.CallUnit;
                    }

                    break;

                case DepositAndPickUpStep.CycleTest:
                    if (e.Next)
                    {
                        this.CurrentStep = DepositAndPickUpStep.CloseShutter;
                    }
                    else
                    {
                        this.CurrentStep = DepositAndPickUpStep.OpenShutter;
                    }

                    break;

                case DepositAndPickUpStep.CloseShutter:
                    if (e.Next)
                    {
                        this.CurrentStep = DepositAndPickUpStep.EndTest;
                    }
                    else
                    {
                        this.CurrentStep = DepositAndPickUpStep.CycleTest;
                    }

                    break;

                case DepositAndPickUpStep.EndTest:
                    if (!e.Next)
                    {
                        this.CurrentStep = DepositAndPickUpStep.CloseShutter;
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
            this.stopTestCommand?.RaiseCanExecuteChanged();

            this.callLoadUnitToBayCommand?.RaiseCanExecuteChanged();
            this.moveToOpenShutterCommand?.RaiseCanExecuteChanged();
            this.moveToEndTestCommand?.RaiseCanExecuteChanged();
            this.moveToCycleTestCommand?.RaiseCanExecuteChanged();
            this.moveToCloseShutterCommand?.RaiseCanExecuteChanged();
            this.resetCommand?.RaiseCanExecuteChanged();
            this.returnCalibration?.RaiseCanExecuteChanged();

            this.openShutterCommand?.RaiseCanExecuteChanged();
            this.closedShutterCommand?.RaiseCanExecuteChanged();

            this.completeCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.NewErrorValue));
            this.RaisePropertyChanged(nameof(this.ChainOffset));
        }

        private async Task CallLoadUnitToBayCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.EjectLoadingUnitAsync(this.MachineService.GetBayPositionSourceByDestination(false), this.LoadingUnitId.Value);
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
                   !this.IsMoving;
        }

        private bool CanCallLoadUnitToBay()
        {
            //return true;

            //return this.CanBaseExecute() &&
            //       !this.SensorsService.IsLoadingUnitInBay &&
            //       !this.MachineService.Loadunits.DrawerInBay() &&
            //       string.IsNullOrEmpty(this.Error);

            return this.CanBaseExecute() &&
                !this.MachineService.Loadunits.DrawerInBay() &&
                this.MachineService.Loadunits.DrawerExists(this.loadingUnitId.Value) &&
                   !this.SensorsService.IsLoadingUnitInBay;
        }

        private bool CanCloseShutter()
        {
            return
                this.CanBaseExecute()
                &&
                // !this.IsShutterMoving &&
                ((this.SensorsService?.IsZeroChain ?? false) || this.SensorsService.IsLoadingUnitOnElevator)
                &&
                (this.SensorsService.ShutterSensors != null && (this.SensorsService.ShutterSensors.Open || this.SensorsService.ShutterSensors.MidWay));
        }

        private bool CanComplete()
        {
            return this.CanBaseExecute();
        }

        private bool CanCycleStart()
        {
            return !this.IsMovingTest &&
                !(this.resetCommandActive && this.CanExecuteResetCommand()) &&
                (this.CumulativePerformedCycles < this.RequiredCycles) &&
                   this.RequiredCycles.HasValue;
        }

        private bool CanExecuteResetCommand()
        {
            return this.CumulativePerformedCycles.HasValue &&
                   this.CumulativePerformedCycles > 0 &&
                   !this.IsMovingTest;
        }

        private bool CanExecuteReturnCalibration()
        {
            return this.CumulativePerformedCycles.HasValue &&
                   this.CumulativePerformedCycles > 0 &&
                   !this.IsMoving;
        }

        private bool CanMoveToCloseShutter()
        {
            return this.CanBaseExecute() &&
                   this.SensorsService.IsLoadingUnitInBay &&
                   (this.CumulativePerformedCycles >= this.RequiredCycles);
        }

        private bool CanMoveToCycleTest()
        {
            return this.CanBaseExecute() &&
                // !this.IsShutterMoving &&
                (this.SensorsService.ShutterSensors.Open || !this.MachineService.HasShutter);
        }

        private bool CanMoveToEndTest()
        {
            return this.CanBaseExecute() &&
                // !this.IsShutterMoving &&
                (this.SensorsService.ShutterSensors.Closed || !this.MachineService.HasShutter);
        }

        private bool CanMoveToOpenShutter()
        {
            // return true;

            // return this.CanBaseExecute() &&
            //       this.SensorsService.IsLoadingUnitInBay &&
            //       (string.IsNullOrEmpty(this.Error) || !this.MachineService.Loadunits.Any());

            return this.CanBaseExecute() &&
                   this.SensorsService.IsLoadingUnitInBay;
        }

        private bool CanOpenShutter()
        {
            return
                this.CanBaseExecute() &&
                   (this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay);
        }

        private bool CanStop()
        {
            if (this.CurrentStep == DepositAndPickUpStep.CycleTest)
            {
                return this.IsMovingTest;
            }
            else
            {
                return this.IsMoving;
            }
        }

        private bool CanStopTest()
        {
            return this.IsMovingTest;
        }

        private async Task ClosedShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersWebService.MoveToAsync(MAS.AutomationService.Contracts.ShutterPosition.Closed);
                this.IsShutterMoving = true;
                this.IsExecutingProcedure = true;
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

        private async Task CompleteAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmCalibrationProcedure"), Localized.Get("InstallationApp.EmbarkDisembarkMenuTitle"), DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    await this.machineElevatorWebService.SetDepositAndPickUpTestCompletedAsync();

                    this.ShowNotification(
                            VW.App.Resources.Localized.Get("InstallationApp.InformationSuccessfullyUpdated"),
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

        private void OnRepetitiveHorizontalMovementsMessageReceived(NotificationMessageUI<RepetitiveHorizontalMovementsMessageData> message)
        {
            var data = message.Data as RepetitiveHorizontalMovementsMessageData;

            if (message.IsNotRunning())
            {
                this.IsExecutingProcedure = false;
            }

            if (message.IsErrored())
            {
                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.ProcedureWasStopped"), Services.Models.NotificationSeverity.Warning);
                this.IsExecutingProcedure = false;
            }

            if (!message.Data.IsTestStopped)
            {
                //if (this.PerformedCyclesThisSession == null && this.CumulativePerformedCycles.HasValue)
                //{
                //    this.totalPerformedCyclesBeforeStart = this.CumulativePerformedCycles.Value;
                //}

                this.CumulativePerformedCycles = message.Data.ExecutedCycles;
                if (this.RequiredCycles.HasValue)
                {
                    this.CyclesPercent = ((double)(this.CumulativePerformedCycles ?? 0) / (double)this.RequiredCycles) * 100.0;
                }
                else
                {
                    this.CyclesPercent = null;
                }
            }

            if (message.Status == MessageStatus.OperationEnd &&
                message.Data?.ExecutedCycles == message.Data.RequiredCycles)
            {
                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.CompletedTest"), Services.Models.NotificationSeverity.Success);
                //this.isCompleted = true;
                this.IsExecutingProcedure = false;

                this.CurrentStep = DepositAndPickUpStep.CycleTest;
                this.RaiseCanExecuteChanged();
            }
        }

        private async Task OpenShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersWebService.MoveToAsync(MAS.AutomationService.Contracts.ShutterPosition.Opened);
                this.IsShutterMoving = true;
                this.IsExecutingProcedure = true;
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task ResetCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.CumulativePerformedCycles = 0;
                this.CyclesPercent = 0;

                await this.machineEnduranceTestWebService.ResetAsync();

                this.resetCommandActive = true;
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);

                this.resetCommandActive = false;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task ReturnCalibrationAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.CumulativePerformedCycles = 0;
                this.CyclesPercent = 0;

                await this.machineEnduranceTestWebService.ResetAsync();

                this.CurrentStep = DepositAndPickUpStep.CallUnit;
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StartCycleAsync()
        {
            try
            {
                var totalCyclesToPerform = this.RequiredCycles.Value - this.CumulativePerformedCycles.Value;
                if (totalCyclesToPerform <= 0)
                {
                    //this.isCompleted = true;
                    this.ShowNotification(Localized.Get("InstallationApp.RequiredCyclesCompleted"), Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsExecutingProcedure = true;
                this.IsWaitingForResponse = true;

                this.RaisePropertyChanged(nameof(this.CumulativePerformedCycles));

                //this.isCompleted = false;

                var bay = await this.bayManager.GetBayAsync();
                var bayPosition = bay.Positions.Single(b => b.LocationUpDown == MAS.AutomationService.Contracts.LoadingUnitLocation.Up);

                var unit = bayPosition?.LoadingUnit;

                await this.machineEnduranceTestWebService.StartHorizontalMovementsAsync(bayPosition.Id, unit.Id);
            }
            catch (Exception ex)
            {
                this.IsExecutingProcedure = false;
                this.ShowNotification(ex);
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
                if (this.currentStep == DepositAndPickUpStep.CycleTest)
                {
                    await this.machineEnduranceTestWebService.StopAsync();
                }
                else
                {
                    await this.MachineService.StopMovingByAllAsync();
                }

                this.IsExecutingProcedure = false;
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

        private async Task StopTestAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.machineEnduranceTestWebService.StopTestAsync();

                this.IsExecutingProcedure = false;
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

        private void SubscribeToEvents()
        {
            this.stepChangedToken = this.stepChangedToken
                ?? this.EventAggregator
                    .GetEvent<StepChangedPubSubEvent>()
                    .Subscribe(
                        (m) => this.OnStepChanged(m),
                        ThreadOption.UIThread,
                        false);

            this.repetitiveHorizontalMovementsMessageReceivedToken = this.repetitiveHorizontalMovementsMessageReceivedToken
                ?? this.EventAggregator
                    .GetEvent<NotificationEventUI<RepetitiveHorizontalMovementsMessageData>>()
                    .Subscribe(
                        (m) => this.OnRepetitiveHorizontalMovementsMessageReceived(m),
                        ThreadOption.UIThread,
                        false);

            this.themeChangedToken = this.themeChangedToken
               ?? this.EventAggregator
                   .GetEvent<ThemeChangedPubSubEvent>()
                   .Subscribe(
                       (m) =>
                       {
                           this.RaisePropertyChanged(nameof(this.HasStepEndTest));
                           this.RaisePropertyChanged(nameof(this.HasStepOpenShutter));
                           this.RaisePropertyChanged(nameof(this.HasStepCloseShutter));
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
                    this.ShowNextStepSinglePage(true, this.moveToOpenShutterCommand?.CanExecute() ?? false);
                    break;

                case DepositAndPickUpStep.OpenShutter:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, this.moveToCycleTestCommand?.CanExecute() ?? false);
                    break;

                case DepositAndPickUpStep.CloseShutter:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, this.moveToEndTestCommand?.CanExecute() ?? false);
                    break;

                case DepositAndPickUpStep.CycleTest:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, this.moveToCloseShutterCommand?.CanExecute() ?? false);
                    break;

                case DepositAndPickUpStep.EndTest:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }

            this.ShowAbortStep(true, !this.IsMoving);

            this.RaisePropertyChanged(nameof(this.HasStepOpenShutter));
            this.RaisePropertyChanged(nameof(this.HasStepCloseShutter));
            this.RaisePropertyChanged(nameof(this.HasStepEndTest));
            this.RaisePropertyChanged(nameof(this.HasStepCycleTest));
            this.RaisePropertyChanged(nameof(this.HasStepCallUnit));
        }

        #endregion
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
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
    public enum HorizontalResolutionCalibrationStep
    {
        StartCalibration,

        RunningCalibration,

        ConfirmAdjustment,
    }

    [Warning(WarningsArea.Installation)]
    public class HorizontalResolutionCalibrationViewModel : BaseMainViewModel//, IDataErrorInfo
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly Services.IDialogService dialogService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineCellsWebService machineCellsWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly Ferretto.VW.MAS.AutomationService.Contracts.BayNumber otherBayNumber;

        private readonly IMachineShuttersWebService shuttersWebService;

        private DelegateCommand applyCommand;

        private DelegateCommand completeCommand;

        private double? currentResolution;

        //private double? currentDistance;
        private HorizontalResolutionCalibrationStep currentStep;

        private double? cyclesPercent;

        private DelegateCommand findZeroElevatorCommand;

        private TimeSpan firstCycleTime = default(TimeSpan);

        private bool isCalibrationCompleted;

        private bool isCalibrationStopped;

        private bool isChainOffsetVisible;

        private bool isChainTuned;

        private bool isElevatorMovingToBay;

        private bool isErrorNegative = true;

        private bool isErrorPositive = false;

        private bool isErrorVisible;

        private bool isExecutingChainCalibration;

        private bool isExecutingProcedure;

        private bool isExecutingStopInPhase;

        //private bool isNewErrorValueVisible;

        private bool isShutterMoving;

        private bool isTuningChain;

        private bool isUseWeightControl;

        private DelegateCommand<string> moveToBayPositionCommand;

        private DelegateCommand moveToConfirmAdjustmentCommand;

        private DelegateCommand moveToShutter2Command;

        private DelegateCommand moveToShutterCommand;

        private DelegateCommand moveToStartCalibrationCommand;

        private int oldPerformedCycle = 0;

        private int performedCycles;

        private SubscriptionToken positioningMessageReceivedToken;

        private TimeSpan remainingTime = default(TimeSpan);

        private DelegateCommand repeatCalibrationCommand;

        private int requiredCycles;

        private DelegateCommand returnCalibration;

        private int sessionPerformedCycles;

        private string shutterLabel;

        private string shutterLabel2;

        private string shutterLabelBtn1;

        private string shutterLabelBtn2;

        private SubscriptionToken shutterPositioningMessageReceivedToken;

        private long singleRaisingTicks = 0;

        private DelegateCommand startCalibrationCommand;

        private int startPerformedCycles;

        private DateTime startTime;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        private DelegateCommand stopInPhaseCommand;

        private SubscriptionToken themeChangedToken;

        private DelegateCommand tuningChainCommand;

        #endregion

        #region Constructors

        public HorizontalResolutionCalibrationViewModel(
            IEventAggregator eventAggregator,
            IMachineElevatorWebService machineElevatorWebService,
            IDialogService dialogService,
            IMachineShuttersWebService shuttersWebService,
            IMachineBaysWebService machineBaysWebService,
            IMachineCellsWebService machineCellsWebService,
            IBayManager bayManager)
          : base(PresentationMode.Installer)
        {
            this.shuttersWebService = shuttersWebService ?? throw new ArgumentNullException(nameof(shuttersWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));

            this.CurrentStep = HorizontalResolutionCalibrationStep.StartCalibration;
            var bays = this.MachineService.Bays;

            var bayPosition = this.Bay.Positions.FirstOrDefault();

            var otherBay = bays.FirstOrDefault(b => b.Number != this.Bay.Number && b.Side != this.Bay.Side && b.Positions.Any(p => Math.Abs(p.Height - bayPosition.Height) < 1000));

            if (otherBay is null)
            {
                this.otherBayNumber = Ferretto.VW.MAS.AutomationService.Contracts.BayNumber.None;
            }
            else
            {
                this.otherBayNumber = otherBay.Number;
            }
        }

        #endregion

        #region Properties

        public ICommand ApplyCommand =>
                    this.applyCommand
            ??
            (this.applyCommand = new DelegateCommand(
                async () => await this.ApplyCorrectionAsync(), this.CanApply));

        public Bay Bay => this.MachineService.Bay;

        public bool BayIsShutterThreeSensors => this.MachineService.IsShutterThreeSensors;

        public BayPosition BayPositionActive { get; set; }

        public IEnumerable<Bay> Bays { get; set; }

        public IEnumerable<Cell> Cells { get; set; }

        public double? ChainOffset => Math.Abs(this.machineElevatorWebService.GetHorizontalOffsetAsync().Result);

        public ICommand CompleteCommand =>
                            this.completeCommand
            ??
            (this.completeCommand = new DelegateCommand(
                async () => await this.CompleteAsync(), this.CanComplete));

        public double? CurrentResolution
        {
            get => this.currentResolution;
            protected set => this.SetProperty(ref this.currentResolution, value);
        }

        public HorizontalResolutionCalibrationStep CurrentStep
        {
            get => this.currentStep;
            protected set => this.SetProperty(ref this.currentStep, value, () => this.UpdateStatusButtonFooter(false));
        }

        public double? CyclesPercent
        {
            get => this.cyclesPercent;
            private set => this.SetProperty(ref this.cyclesPercent, value);
        }

        public ICommand FindZeroElevatorCommand =>
                                                                                            this.findZeroElevatorCommand
            ??
            (this.findZeroElevatorCommand = new DelegateCommand(
                async () => await this.FindZeroElevatorAsync(),
                this.CanFindZeroElevatorCommand));

        public bool HasBayDown => this.Bay?.Positions.Any(p => !p.IsUpper) == true;

        public bool HasBayExternal => this.MachineService.HasBayExternal;

        public bool HasBayUp => this.Bay?.Positions.Any(p => p.IsUpper) == true;

        public bool HasShutter => this.MachineService.HasShutter;

        public bool HasShutter2 => this.otherBayNumber != MAS.AutomationService.Contracts.BayNumber.None;

        public bool HasShutterBoth => this.HasShutter && !this.HasShutter2;

        public bool HasStepConfirmAdjustment => this.currentStep is HorizontalResolutionCalibrationStep.ConfirmAdjustment;

        public bool HasStepRunningCalibration => this.currentStep is HorizontalResolutionCalibrationStep.RunningCalibration;

        public bool HasStepStartCalibration => this.currentStep is HorizontalResolutionCalibrationStep.StartCalibration;

        public bool IsCalibrationCompleted
        {
            get => this.isCalibrationCompleted;
            set => this.SetProperty(ref this.isCalibrationCompleted, value);
        }

        public bool IsCalibrationNotCompletedAndStopped => !this.IsCalibrationStopped && !this.IsCalibrationCompleted;

        public bool IsCalibrationStopped
        {
            get => this.isCalibrationStopped;
            private set
            {
                if (this.SetProperty(ref this.isCalibrationStopped, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsChainOffsetVisible
        {
            get => this.isChainOffsetVisible;
            set => this.SetProperty(ref this.isChainOffsetVisible, value);
        }

        public bool IsChainTuned
        {
            get => this.isChainTuned;
            private set => this.SetProperty(ref this.isChainTuned, value);
        }

        public bool IsElevatorMovingToBay
        {
            get => this.isElevatorMovingToBay;
            private set => this.SetProperty(ref this.isElevatorMovingToBay, value);
        }

        public bool IsErrorNegative
        {
            get => this.isErrorNegative;
            set
            {
                if (this.SetProperty(ref this.isErrorNegative, value))
                {
                    this.isErrorPositive = !this.isErrorNegative;
                    this.RaisePropertyChanged(nameof(this.IsErrorPositive));
                }
            }
        }

        public bool IsErrorPositive
        {
            get => this.isErrorPositive;
            set
            {
                if (this.SetProperty(ref this.isErrorPositive, value))
                {
                    this.isErrorNegative = !this.isErrorPositive;
                    this.RaisePropertyChanged(nameof(this.IsErrorNegative));
                }
            }
        }

        public bool IsExecutingChainCalibration
        {
            get => this.isExecutingChainCalibration;
            private set
            {
                if (this.SetProperty(ref this.isExecutingChainCalibration, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            private set
            {
                if (this.SetProperty(ref this.isExecutingProcedure, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsExecutingStopInPhase
        {
            get => this.isExecutingStopInPhase;
            private set
            {
                if (this.SetProperty(ref this.isExecutingStopInPhase, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsMoving => (this.MachineService?.MachineStatus?.IsMoving ?? true) || (this.MachineService?.MachineStatus?.IsMovingLoadingUnit ?? true) || this.IsShutterMoving;

        public bool IsShutterMoving
        {
            get => this.isShutterMoving;
            private set
            {
                if (this.SetProperty(ref this.isShutterMoving, value))
                {
                    this.RaisePropertyChanged();
                }
            }
        }

        //public bool IsNewErrorValueVisible
        //{
        //    get => this.isNewErrorValueVisible;
        //    set => this.SetProperty(ref this.isNewErrorValueVisible, value);
        //}
        public bool IsTuningChain
        {
            get => this.isTuningChain;
            private set => this.SetProperty(ref this.isTuningChain, value);
        }

        public bool IsUseWeightControl
        {
            get => this.isUseWeightControl;
            set => this.SetProperty(ref this.isUseWeightControl, value);
        }

        public ICommand MoveToBayPositionCommand =>
            this.moveToBayPositionCommand
            ??
            (this.moveToBayPositionCommand = new DelegateCommand<string>(
                async (level) => await this.MoveToBayPositionAsync(level),
                (level) => this.CanMoveToBayPosition(level)));

        public ICommand MoveToConfirmAdjustmentCommand =>
            this.moveToConfirmAdjustmentCommand
            ??
            (this.moveToConfirmAdjustmentCommand = new DelegateCommand(
                () => this.CurrentStep = HorizontalResolutionCalibrationStep.ConfirmAdjustment,
                () => this.CanBaseExecute()));

        public ICommand MoveToShutter2Command =>
                    this.moveToShutter2Command
                    ??
                    (this.moveToShutter2Command = new DelegateCommand(
                        async () => await this.MoveToShutter2Async(),
                        this.CanBaseExecute));

        public ICommand MoveToShutterCommand =>
                    this.moveToShutterCommand
            ??
            (this.moveToShutterCommand = new DelegateCommand(
                async () => await this.MoveToShutterAsync(),
                this.CanBaseExecute));

        public ICommand MoveToStartCalibrationCommand =>
            this.moveToStartCalibrationCommand
            ??
            (this.moveToStartCalibrationCommand = new DelegateCommand(
                () => this.CurrentStep = HorizontalResolutionCalibrationStep.StartCalibration,
                () => this.CanMoveToStartCalibration()));

        public int PerformedCycles
        {
            get => this.performedCycles;
            set
            {
                if (this.SetProperty(ref this.performedCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        //public int? NewErrorValue
        //{
        //    get => this.newErrorValue;
        //    set => this.SetProperty(ref this.newErrorValue, value);
        //}
        public TimeSpan RemainingTime
        {
            get => new TimeSpan(this.remainingTime.Hours, this.remainingTime.Minutes, this.remainingTime.Seconds);
            set => this.SetProperty(ref this.remainingTime, value, this.RaiseCanExecuteChanged);
        }

        public ICommand RepeatCalibrationCommand =>
            this.repeatCalibrationCommand
            ??
            (this.repeatCalibrationCommand = new DelegateCommand(
                () =>
                {
                    this.CurrentStep = HorizontalResolutionCalibrationStep.StartCalibration;
                },
                this.CanRepeat));

        public int RequiredCycles
        {
            get => this.requiredCycles;
            set
            {
                if (this.SetProperty(ref this.requiredCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand ReturnCalibration =>
                                                                                                                                                                                                           this.returnCalibration
           ??
           (this.returnCalibration = new DelegateCommand(
               async () =>
               {
                   try
                   {
                       this.CurrentStep = HorizontalResolutionCalibrationStep.StartCalibration;
                   }
                   catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                   {
                       this.ShowNotification(ex);
                       this.isErrorVisible = true;
                   }
               }));

        public int SessionPerformedCycles
        {
            get => this.sessionPerformedCycles;
            set
            {
                if (this.SetProperty(ref this.sessionPerformedCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string ShutterLabel
        {
            get => this.shutterLabel;
            private set => this.SetProperty(ref this.shutterLabel, value);
        }

        public string ShutterLabel2
        {
            get => this.shutterLabel2;
            private set => this.SetProperty(ref this.shutterLabel2, value);
        }

        public string ShutterLabelBtn1
        {
            get => this.shutterLabelBtn1;
            private set => this.SetProperty(ref this.shutterLabelBtn1, value);
        }

        public string ShutterLabelBtn2
        {
            get => this.shutterLabelBtn2;
            private set => this.SetProperty(ref this.shutterLabelBtn2, value);
        }

        public ICommand StartCalibrationCommand =>
            this.startCalibrationCommand
            ??
            (this.startCalibrationCommand = new DelegateCommand(
               async () => await this.StartCalibrationAsync(),
               this.CanStartCalibration));

        public int StartPerformedCycles
        {
            get => this.startPerformedCycles;
            set
            {
                if (this.SetProperty(ref this.startPerformedCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        public ICommand StopInPhaseCommand =>
            this.stopInPhaseCommand
            ??
            (this.stopInPhaseCommand = new DelegateCommand(
                async () => await this.StopInPhaseAsync(),
                this.CanStopInPhase));

        public double? TotalDistance => Math.Abs(this.machineElevatorWebService.GetHorizontalTotalDistanceAsync().Result);

        public ICommand TuningChainCommand =>
            this.tuningChainCommand
            ??
            (this.tuningChainCommand = new DelegateCommand(
                async () => await this.TuningChainAsync(),
                this.CanTuningChain));

        protected Carousel ProcedureParameters { get; private set; }

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

            if (this.positioningMessageReceivedToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>().Unsubscribe(this.positioningMessageReceivedToken);
                this.positioningMessageReceivedToken?.Dispose();
                this.positioningMessageReceivedToken = null;
            }

            if (this.shutterPositioningMessageReceivedToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<ShutterPositioningMessageData>>().Unsubscribe(this.shutterPositioningMessageReceivedToken);
                this.shutterPositioningMessageReceivedToken?.Dispose();
                this.shutterPositioningMessageReceivedToken = null;
            }

            if (this.themeChangedToken != null)
            {
                this.EventAggregator.GetEvent<ThemeChangedPubSubEvent>().Unsubscribe(this.themeChangedToken);
                this.themeChangedToken?.Dispose();
                this.themeChangedToken = null;
            }
        }

        public MAS.AutomationService.Contracts.LoadingUnitLocation GetBayPosition()
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

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            this.BayPositionActive = null;
            this.IsChainTuned = false;
            this.IsExecutingStopInPhase = false;
            this.IsShutterMoving = false;

            this.ShutterLabel = this.SensorsService.ShutterSensors.Open ? Localized.Get("InstallationApp.GateClose") : Localized.Get("InstallationApp.GateOpen");

            //  = "{res:Loc InstallationApp.Shutter}" />
            //< Run Text = "{res:Loc InstallationApp.Bay2}

            switch (this.Bay.Number)
            {
                case MAS.AutomationService.Contracts.BayNumber.BayOne:
                    this.ShutterLabelBtn1 = Localized.Get("InstallationApp.Shutter") + " " + Localized.Get("InstallationApp.Bay1");
                    break;

                case MAS.AutomationService.Contracts.BayNumber.BayTwo:
                    this.ShutterLabelBtn1 = Localized.Get("InstallationApp.Shutter") + " " + Localized.Get("InstallationApp.Bay2");
                    break;

                case MAS.AutomationService.Contracts.BayNumber.BayThree:
                    this.ShutterLabelBtn1 = Localized.Get("InstallationApp.Shutter") + " " + Localized.Get("InstallationApp.Bay3");
                    break;

                case MAS.AutomationService.Contracts.BayNumber.None:
                default:
                    this.ShutterLabelBtn1 = "";
                    break;
            }

            switch (this.otherBayNumber)
            {
                case MAS.AutomationService.Contracts.BayNumber.BayOne:
                    this.ShutterLabel2 = this.SensorsService.ShutterSensorsBay1.Open ? Localized.Get("InstallationApp.GateClose") : Localized.Get("InstallationApp.GateOpen");
                    this.ShutterLabelBtn2 = Localized.Get("InstallationApp.Shutter") + " " + Localized.Get("InstallationApp.Bay1");
                    break;

                case MAS.AutomationService.Contracts.BayNumber.BayTwo:
                    this.ShutterLabel2 = this.SensorsService.ShutterSensorsBay2.Open ? Localized.Get("InstallationApp.GateClose") : Localized.Get("InstallationApp.GateOpen");
                    this.ShutterLabelBtn2 = Localized.Get("InstallationApp.Shutter") + " " + Localized.Get("InstallationApp.Bay2");
                    break;

                case MAS.AutomationService.Contracts.BayNumber.BayThree:
                    this.ShutterLabel2 = this.SensorsService.ShutterSensorsBay3.Open ? Localized.Get("InstallationApp.GateClose") : Localized.Get("InstallationApp.GateOpen");
                    this.ShutterLabelBtn2 = Localized.Get("InstallationApp.Shutter") + " " + Localized.Get("InstallationApp.Bay3");
                    break;

                case MAS.AutomationService.Contracts.BayNumber.None:
                default:
                    this.ShutterLabelBtn2 = "";
                    break;
            }

            this.UpdateStatusButtonFooter(true);

            await base.OnAppearedAsync();
        }

        public async Task RetrieveProcedureInformationAsync()
        {
            try
            {
                var procedureParameters = await this.machineElevatorWebService.GetHorizontalResolutionProcedureAsync();

                this.RequiredCycles = procedureParameters.RequiredCycles;
                this.PerformedCycles = procedureParameters.PerformedCycles;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.isErrorVisible = true;
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
                this.CurrentResolution = await this.machineElevatorWebService.GetHorizontalResolutionAsync();
                await this.RetrieveProcedureInformationAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.isErrorVisible = true;
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
                this.IsExecutingChainCalibration = false;
            }
        }

        protected void OnStepChanged(StepChangedMessage e)
        {
            switch (this.CurrentStep)
            {
                case HorizontalResolutionCalibrationStep.StartCalibration:
                    if (e.Next)
                    {
                        this.CurrentStep = HorizontalResolutionCalibrationStep.RunningCalibration;
                    }

                    break;

                case HorizontalResolutionCalibrationStep.RunningCalibration:
                    if (!e.Next)
                    {
                        this.CurrentStep = HorizontalResolutionCalibrationStep.StartCalibration;
                    }

                    break;

                case HorizontalResolutionCalibrationStep.ConfirmAdjustment:
                    if (!e.Next)
                    {
                        this.CurrentStep = HorizontalResolutionCalibrationStep.StartCalibration;
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

            this.stopCommand?.RaiseCanExecuteChanged();
            this.stopInPhaseCommand?.RaiseCanExecuteChanged();

            this.startCalibrationCommand?.RaiseCanExecuteChanged();
            this.completeCommand?.RaiseCanExecuteChanged();
            this.tuningChainCommand?.RaiseCanExecuteChanged();
            this.moveToConfirmAdjustmentCommand?.RaiseCanExecuteChanged();
            this.moveToStartCalibrationCommand?.RaiseCanExecuteChanged();
            this.moveToShutterCommand?.RaiseCanExecuteChanged();
            this.moveToShutter2Command?.RaiseCanExecuteChanged();
            this.moveToBayPositionCommand?.RaiseCanExecuteChanged();
            this.repeatCalibrationCommand?.RaiseCanExecuteChanged();

            this.ShutterLabel = this.SensorsService.ShutterSensors.Open ? Localized.Get("InstallationApp.GateClose") : Localized.Get("InstallationApp.GateOpen");

            switch (this.otherBayNumber)
            {
                case MAS.AutomationService.Contracts.BayNumber.BayOne:
                    this.ShutterLabel2 = this.SensorsService.ShutterSensorsBay1.Open ? Localized.Get("InstallationApp.GateClose") : Localized.Get("InstallationApp.GateOpen");
                    break;

                case MAS.AutomationService.Contracts.BayNumber.BayTwo:
                    this.ShutterLabel2 = this.SensorsService.ShutterSensorsBay2.Open ? Localized.Get("InstallationApp.GateClose") : Localized.Get("InstallationApp.GateOpen");
                    break;

                case MAS.AutomationService.Contracts.BayNumber.BayThree:
                    this.ShutterLabel2 = this.SensorsService.ShutterSensorsBay3.Open ? Localized.Get("InstallationApp.GateClose") : Localized.Get("InstallationApp.GateOpen");
                    break;

                case MAS.AutomationService.Contracts.BayNumber.None:
                default:
                    break;
            }

            this.RaisePropertyChanged(nameof(this.RemainingTime));
            this.RaisePropertyChanged(nameof(this.PerformedCycles));
            this.RaisePropertyChanged(nameof(this.CyclesPercent));
            this.RaisePropertyChanged(nameof(this.RequiredCycles));
            this.RaisePropertyChanged(nameof(this.IsExecutingProcedure));
            this.RaisePropertyChanged(nameof(this.IsExecutingStopInPhase));
            this.RaisePropertyChanged(nameof(this.IsCalibrationNotCompletedAndStopped));

            //this.RaisePropertyChanged(nameof(this.NewErrorValue));
            this.RaisePropertyChanged(nameof(this.ChainOffset));
        }

        private async Task ApplyCorrectionAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                var measuredCorrection = this.IsErrorNegative ? this.ChainOffset : -this.ChainOffset;
                var distance = this.TotalDistance;
                var correctionForEachMovement = measuredCorrection / (this.SessionPerformedCycles > 2 ? this.SessionPerformedCycles : 2);
                var newDistance = correctionForEachMovement + distance;
                var newResolution = (double)(this.CurrentResolution * newDistance / distance);
                var applyCorrection = $"{Localized.Get("InstallationApp.Resolution")}: {this.CurrentResolution:0.000000}, {Localized.Get("InstallationApp.NewResolution")}: {newResolution:0.000000}";
                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ApplyCorrectionMessage"), applyCorrection, DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    await this.machineElevatorWebService.UpdateHorizontalResolutionAsync(newResolution);

                    this.Logger.Debug(applyCorrection);

                    this.CurrentResolution = newResolution;

                    this.ShowNotification(
                            Localized.Get("InstallationApp.InformationSuccessfullyUpdated"),
                            Services.Models.NotificationSeverity.Success);

                    this.CurrentStep = HorizontalResolutionCalibrationStep.StartCalibration;
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.isErrorVisible = true;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private bool CanApply()
        {
            return true;
        }

        private bool CanBaseExecute()
        {
            return !this.IsKeyboardOpened &&
                   !this.IsMoving;
        }

        private bool CanComplete()
        {
            return this.CanBaseExecute();
        }

        private bool CanFindZeroElevator()
        {
            return !this.SensorsService.IsZeroChain &&
                !this.SensorsService.Sensors.LuPresentInMachineSide &&
                !this.SensorsService.Sensors.LuPresentInOperatorSide;
        }

        private bool CanFindZeroElevatorCommand()
        {
            return this.CanFindZeroElevator() &&
                this.MachineService.MachinePower == MachinePowerState.Powered;
        }

        private bool CanMoveToBayPosition(string level)
        {
            return this.CanBaseExecute();
        }

        private bool CanMoveToGoToBay()
        {
            return this.CanBaseExecute() &&
                (this.SensorsService.ShutterSensors.Closed || !this.HasShutter);
        }

        private bool CanMoveToStartCalibration()
        {
            return this.CanBaseExecute();
        }

        private bool CanRepeat()
        {
            return this.CanBaseExecute();
        }

        private bool CanStartCalibration()
        {
            var isOk = !this.IsKeyboardOpened &&
                    !this.IsWaitingForResponse &&
                    !this.IsMoving &&
                    !this.isErrorVisible;
            if (isOk)
            {
                isOk = !this.SensorsService.IsLoadingUnitInBay;
                if (!isOk)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.LuPresenceOnOperatorCradle"), NotificationSeverity.Warning);
                }
            }
            if (isOk)
            {
                isOk = this.IsChainTuned;
                if (!isOk)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.HorizzontalChainCalibrationProcedureRequired"), NotificationSeverity.Warning);
                }
            }
            if (isOk)
            {
                isOk = this.BayPositionActive != null;
                if (!isOk)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.PositionElevatorBayHeight"), NotificationSeverity.Warning);
                }
            }
            if (isOk)
            {
                isOk = this.SensorsService.ShutterSensors.Open || !this.HasShutter;
                if (!isOk)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.OpenShutterCommand"), NotificationSeverity.Warning);
                }
            }
            if (isOk)
            {
                var level = this.BayPositionActive.Height;
                isOk = !this.Cells.Any(c => !c.IsFree && Math.Abs(c.Position - level) < 50 && c.Side != this.Bay.Side);
                if (!isOk)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.LuPresenceOnMachineCradle"), NotificationSeverity.Warning);
                }
            }
            if (isOk)
            {
                isOk = !this.SensorsService.IsHorizontalInconsistentBothLow &&
                   !this.SensorsService.IsHorizontalInconsistentBothHigh;
                if (!isOk)
                {
                    this.ShowNotification(Localized.Get("ServiceMachine.InconsistencyStateAndSensors"), NotificationSeverity.Warning);
                }
            }
            if (isOk)
            {
                isOk = this.SensorsService.BayZeroChain || !this.MachineService.HasCarousel;
                if (!isOk)
                {
                    this.ShowNotification(Localized.Get("ServiceMachine.CalibrationCarouselFailedChainNotZeroPosition"), NotificationSeverity.Warning);
                }
            }
            if (isOk)
            {
                var otherBay = this.Bays.FirstOrDefault(b => b.Number != this.Bay.Number
                            && b.Side != this.Bay.Side
                            && b.Positions.Any(p => Math.Abs(p.Height - this.BayPositionActive.Height) < 700));
                if (otherBay != null)
                {
                    isOk = !this.SensorsService.IsLoadingUnitInBayByNumber(otherBay.Number);

                    if (!isOk)
                    {
                        this.ShowNotification(string.Format(Localized.Get("ServiceMachine.InconsistencyStateAndSensorsBay"), (int)otherBay.Number), NotificationSeverity.Warning);
                    }
                }
            }
            return isOk;
        }

        private bool CanStop()
        {
            if (this.CurrentStep == HorizontalResolutionCalibrationStep.RunningCalibration)
            {
                return !this.IsKeyboardOpened
                        &&
                        this.MachineModeService.MachinePower == MachinePowerState.Powered;
            }
            else
            {
                return this.IsMoving;
            }
        }

        private bool CanStopInPhase()
        {
            return
                this.IsMoving
                &&
                !this.IsWaitingForResponse && !this.IsExecutingStopInPhase;
        }

        private bool CanTuningChain()
        {
            return this.CanBaseExecute()
                &&
                !this.IsTuningChain
                &&
                this.SensorsService.IsZeroChain
                &&
                !this.SensorsService.Sensors.LuPresentInMachineSide
                &&
                !this.SensorsService.Sensors.LuPresentInOperatorSide;
        }

        private async Task CompleteAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmCalibrationProcedure"), Localized.Get("InstallationApp.HorizontalResolutionCalibration"), DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    this.IsExecutingStopInPhase = false;
                    this.IsExecutingProcedure = false;

                    await this.machineElevatorWebService.SetHorizontalResolutionCalibrationCompletedAsync();
                    await this.machineElevatorWebService.SearchHorizontalZeroAsync();
                    this.IsChainTuned = true;

                    this.ShowNotification(
                            VW.App.Resources.Localized.Get("InstallationApp.InformationSuccessfullyUpdated"),
                            Services.Models.NotificationSeverity.Success);
                }

                this.NavigationService.GoBack();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.isErrorVisible = true;
            }
            finally
            {
                this.CurrentStep = HorizontalResolutionCalibrationStep.StartCalibration;
                this.IsWaitingForResponse = false;
            }
        }

        private async Task FindZeroElevatorAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.machineElevatorWebService.FindLostZeroAsync();
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

        private async Task MoveToBayPositionAsync(string level)
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.isErrorVisible = false;

                var bay = this.MachineService.Bay;
                this.BayPositionActive = bay.Positions.Single(b => b.IsUpper == (level == "UP"));
                this.Cells = await this.machineCellsWebService.GetAllAsync();
                this.Bays = this.MachineService.Bays;

                await this.machineElevatorWebService.MoveToBayPositionAsync(
                    this.BayPositionActive.Id,
                    computeElongation: true,
                    performWeighting: this.isUseWeightControl);

                this.IsElevatorMovingToBay = true;
                this.IsExecutingProcedure = true;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingToBay = false;

                this.ShowNotification(ex);
                this.isErrorVisible = true;
            }
            finally
            {
                this.IsWaitingForResponse = false;
                this.IsElevatorMovingToBay = false;
            }
        }

        private async Task MoveToShutter2Async()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsShutterMoving = true;
                this.isErrorVisible = false;

                switch (this.otherBayNumber)
                {
                    case MAS.AutomationService.Contracts.BayNumber.BayOne:
                        await this.shuttersWebService.MoveToBayNumberAsync(this.SensorsService.ShutterSensorsBay1.Open ? MAS.AutomationService.Contracts.ShutterPosition.Closed : MAS.AutomationService.Contracts.ShutterPosition.Opened, this.otherBayNumber);
                        break;

                    case MAS.AutomationService.Contracts.BayNumber.BayTwo:
                        await this.shuttersWebService.MoveToBayNumberAsync(this.SensorsService.ShutterSensorsBay2.Open ? MAS.AutomationService.Contracts.ShutterPosition.Closed : MAS.AutomationService.Contracts.ShutterPosition.Opened, this.otherBayNumber);
                        break;

                    case MAS.AutomationService.Contracts.BayNumber.BayThree:
                        await this.shuttersWebService.MoveToBayNumberAsync(this.SensorsService.ShutterSensorsBay3.Open ? MAS.AutomationService.Contracts.ShutterPosition.Closed : MAS.AutomationService.Contracts.ShutterPosition.Opened, this.otherBayNumber);
                        break;

                    case MAS.AutomationService.Contracts.BayNumber.None:
                    default:
                        break;
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.isErrorVisible = true;
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

        private async Task MoveToShutterAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsShutterMoving = true;
                this.isErrorVisible = false;

                await this.shuttersWebService.MoveToAsync(
                    this.SensorsService.ShutterSensors.Open ?
                    MAS.AutomationService.Contracts.ShutterPosition.Closed :
                    MAS.AutomationService.Contracts.ShutterPosition.Opened);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.isErrorVisible = true;
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

        private async void OnPositioningMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message.Data?.MovementMode != MovementMode.HorizontalResolution)
            {
                return;
            }

            if (message.IsNotRunning())
            {
                this.IsExecutingProcedure = false;
            }

            if (message.IsErrored() ||
                this.MachineError != null)
            {
                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.ProcedureWasStopped"), Services.Models.NotificationSeverity.Warning);

                //this.IsChainOffsetVisible = true;
                // this.IsNewErrorValueVisible = false;

                this.IsExecutingProcedure = false;

                this.IsCalibrationStopped = false;
                this.IsCalibrationCompleted = this.requiredCycles == this.performedCycles ? true : false;

                this.CurrentStep = HorizontalResolutionCalibrationStep.ConfirmAdjustment;

                if (this.MachineError != null)
                {
                    //this.IsChainOffsetVisible = false;
                    //this.IsNewErrorValueVisible = false;
                    this.IsCalibrationCompleted = false;
                }

                return;
            }

            if (message.Status == MessageStatus.OperationExecuting)
            {
                // update cycle info
                if (!this.IsExecutingStopInPhase)
                {
                    this.RequiredCycles = message.Data.RequiredCycles;
                }
                this.PerformedCycles = message.Data.ExecutedCycles;
                this.SessionPerformedCycles = this.PerformedCycles - this.StartPerformedCycles;

                this.UpdateRemainingTime();
            }

            if (message.Status == MessageStatus.OperationEnd)
            {
                this.PerformedCycles = message.Data.ExecutedCycles;

                if (message.Data.IsTestStopped)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.TestStopped"), Services.Models.NotificationSeverity.Success);

                    //this.IsNewErrorValueVisible = false;

                    this.IsCalibrationCompleted = false;

                    this.IsCalibrationStopped = true;
                }
                else
                {
                    this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.CompletedTest"), Services.Models.NotificationSeverity.Success);

                    this.IsCalibrationCompleted = this.requiredCycles == this.performedCycles ? true : false;

                    this.IsCalibrationStopped = false;
                }

                this.IsChainOffsetVisible = false;
                //this.IsNewErrorValueVisible = true;

                this.IsExecutingProcedure = false;

                //this.NewErrorValue = 0;
                this.CurrentStep = HorizontalResolutionCalibrationStep.ConfirmAdjustment;

                this.RaiseCanExecuteChanged();
            }

            if (message.Status == MessageStatus.OperationStop)
            {
                this.IsCalibrationCompleted = false;

                //this.IsChainOffsetVisible = false;
                //this.IsNewErrorValueVisible = false;

                this.IsExecutingStopInPhase = false;
                this.IsExecutingProcedure = false;
                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.ProcedureWasStopped"), Services.Models.NotificationSeverity.Warning);

                this.IsCalibrationStopped = true;
                //this.NewErrorValue = 0;
                this.PerformedCycles = message.Data.ExecutedCycles;

                this.SessionPerformedCycles = this.PerformedCycles - this.StartPerformedCycles;

                this.CurrentStep = HorizontalResolutionCalibrationStep.ConfirmAdjustment;
                this.RaiseCanExecuteChanged();

                this.IsExecutingProcedure = false;
            }
            this.CyclesPercent = (this.performedCycles * 100) / this.requiredCycles;
        }

        private void OnShutterPositioningMessageReceived(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            if (message.Data?.MovementMode == MovementMode.ShutterPosition
                && (message.Status == MessageStatus.OperationEnd
                    || message.Status == MessageStatus.OperationError
                    || message.Status == MessageStatus.OperationStop))
            {
                this.IsShutterMoving = false;
            }
        }

        private async Task StartCalibrationAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                await this.machineElevatorWebService.ResetHorizontalResolutionAsync();

                await this.RetrieveProcedureInformationAsync();

                // Update procedure info
                this.StartPerformedCycles = 0;
                this.SessionPerformedCycles = 0;

                this.CyclesPercent = 0;

                this.oldPerformedCycle = this.PerformedCycles;
                this.startTime = DateTime.Now;

                if (this.RemainingTime != null)
                {
                    this.RemainingTime = default(TimeSpan);
                }
                await this.machineElevatorWebService.MoveHorizontalResolutionAsync(MAS.AutomationService.Contracts.HorizontalMovementDirection.Forwards);

                this.IsExecutingProcedure = true;
                this.RaiseCanExecuteChanged();

                this.CurrentStep = HorizontalResolutionCalibrationStep.RunningCalibration;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.isErrorVisible = true;
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
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.isErrorVisible = true;
            }
            finally
            {
                this.IsExecutingProcedure = false;
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StopInPhaseAsync()
        {
            this.IsWaitingForResponse = true;
            this.IsExecutingProcedure = false;
            this.IsExecutingStopInPhase = true;
            try
            {
                await this.machineElevatorWebService.StopCalibrationAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.isErrorVisible = true;
            }
            finally
            {
                this.IsExecutingProcedure = false;
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

            this.positioningMessageReceivedToken = this.positioningMessageReceivedToken
                ?? this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        this.OnPositioningMessageReceived,
                        ThreadOption.UIThread,
                        false);

            this.shutterPositioningMessageReceivedToken = this.shutterPositioningMessageReceivedToken
                ?? this.EventAggregator
                    .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                    .Subscribe(
                        this.OnShutterPositioningMessageReceived,
                        ThreadOption.UIThread,
                        false);

            this.themeChangedToken = this.themeChangedToken
               ?? this.EventAggregator
                   .GetEvent<ThemeChangedPubSubEvent>()
                   .Subscribe(
                       (m) =>
                       {
                           this.RaisePropertyChanged(nameof(this.HasStepStartCalibration));
                           this.RaisePropertyChanged(nameof(this.HasStepRunningCalibration));
                           this.RaisePropertyChanged(nameof(this.HasStepConfirmAdjustment));
                       },
                       ThreadOption.UIThread,
                       false);
        }

        private async Task TuningChainAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.machineElevatorWebService.SearchHorizontalZeroAsync();
                this.IsTuningChain = true;
                this.IsChainTuned = true;
                this.IsExecutingProcedure = true;
                this.IsExecutingChainCalibration = true;
            }
            catch (Exception ex)
            {
                this.IsTuningChain = false;
                this.IsChainTuned = true;
                this.IsExecutingChainCalibration = false;

                this.ShowNotification(ex);
                this.isErrorVisible = true;
            }
            finally
            {
                this.IsExecutingProcedure = false;
                this.IsWaitingForResponse = false;
            }
        }

        private void UpdateRemainingTime()
        {
            try
            {
                if ((this.oldPerformedCycle == 0) && (this.PerformedCycles == 0))
                {
                    this.singleRaisingTicks = 0;

                    if (this.firstCycleTime != null)
                    {
                        this.firstCycleTime = default(TimeSpan);
                    }

                    if (this.RemainingTime != null)
                    {
                        this.RemainingTime = default(TimeSpan);
                    }
                }
                else
                {
                    if (this.PerformedCycles > this.oldPerformedCycle)
                    {
                        this.oldPerformedCycle = this.PerformedCycles;

                        // escludiamo il primo in quanto è affetto da ritardi (8s vs 4s circa)
                        if (this.PerformedCycles == 1)
                        {
                            this.firstCycleTime = DateTime.Now - this.startTime;
                        }
                        else
                        {
                            // ogni 10 cicli ricalcolo il tempo per singolo ciclo...
                            if ((this.PerformedCycles % 10) == 0 || this.PerformedCycles == 2)
                            {
                                var totalCycleTime = DateTime.Now - this.startTime - this.firstCycleTime;

                                this.singleRaisingTicks = totalCycleTime.Ticks / (this.PerformedCycles - 1);
                            }
                        }

                        this.RemainingTime = TimeSpan.FromTicks(this.singleRaisingTicks * (this.RequiredCycles - this.PerformedCycles));
                    }
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.isErrorVisible = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void UpdateStatusButtonFooter(bool force = false)
        {
            if (!this.IsVisible && !force)
            {
                return;
            }

            switch (this.CurrentStep)
            {
                case HorizontalResolutionCalibrationStep.StartCalibration:
                    this.ShowPrevStepSinglePage(true, false);
                    this.IsChainTuned = false;
                    this.isErrorVisible = false;
                    this.IsExecutingStopInPhase = false;
                    break;

                case HorizontalResolutionCalibrationStep.RunningCalibration:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, false);
                    break;

                case HorizontalResolutionCalibrationStep.ConfirmAdjustment:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }

            this.ShowAbortStep(true, !this.IsMoving);

            this.RaisePropertyChanged(nameof(this.HasStepStartCalibration));
            this.RaisePropertyChanged(nameof(this.HasStepRunningCalibration));
            this.RaisePropertyChanged(nameof(this.HasStepConfirmAdjustment));
        }

        #endregion
    }
}

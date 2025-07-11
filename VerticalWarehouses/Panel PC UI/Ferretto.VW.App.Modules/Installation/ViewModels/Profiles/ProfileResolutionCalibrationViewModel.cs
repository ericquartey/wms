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
    public enum ProfileResolutionCalibrationStep
    {
        StartCalibration,

        RunningCalibration,

        ConfirmAdjustment,
    }

    [Warning(WarningsArea.Installation)]
    public class ProfileResolutionCalibrationViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IDialogService dialogService;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineProfileProcedureWebService machineProfileProcedureWeb;

        private readonly IMachineShuttersWebService shuttersWebService;

        private readonly IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService;

        private DelegateCommand completeCommand;

        private double? currentResolution;

        private ProfileResolutionCalibrationStep currentStep;

        private double? cyclesPercent;

        private bool isCalibrationCompleted;

        private bool isCalibrationStopped;

        private bool isChainOffsetVisible;

        private bool isElevatorMovingToBay;

        private bool isErrorNegative = true;

        private bool isErrorPositive = false;

        private bool isErrorVisible;

        private bool isExecutingProcedure;

        private bool isExecutingStopInPhase;

        private bool isShutterMoving;

        private bool isTuningVertical;

        private bool isUseWeightControl;

        private bool isVerticalCalibration;

        //private DelegateCommand<string> moveToBayPositionCommand;

        private DelegateCommand moveToConfirmAdjustmentCommand;

        private DelegateCommand moveToShutterCommand;

        private DelegateCommand moveToStartCalibrationCommand;

        private int performedCycles;

        private SubscriptionToken positioningMessageReceivedToken;

        private double[] profileConst;

        private double profileConst0;

        private double profileConst1;

        private int[] profileSamples;

        private double profileSamples0;

        private double profileSamples1;

        private double profileSamples2;

        private TimeSpan remainingTime = default(TimeSpan);

        private DelegateCommand repeatCalibrationCommand;

        private int requiredCycles;

        private DelegateCommand returnCalibration;

        private int sessionPerformedCycles;

        private string shutterLabel;

        private SubscriptionToken shutterPositioningMessageReceivedToken;

        private DelegateCommand startCalibrationCommand;

        private int startPerformedCycles;

        private DateTime startTime;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        private DelegateCommand stopInPhaseCommand;

        private SubscriptionToken themeChangedToken;

        private DelegateCommand verticalCalibrationCommand;

        #endregion

        #region Constructors

        public ProfileResolutionCalibrationViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IDialogService dialogService,
            IMachineShuttersWebService shuttersWebService,
            IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService,
            IMachineProfileProcedureWebService machineProfileProcedureWeb,
            IMachineBaysWebService machineBaysWebService)
          : base(PresentationMode.Installer)
        {
            this.shuttersWebService = shuttersWebService ?? throw new ArgumentNullException(nameof(shuttersWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.machineProfileProcedureWeb = machineProfileProcedureWeb ?? throw new ArgumentNullException(nameof(machineProfileProcedureWeb));
            this.verticalOriginProcedureWebService = verticalOriginProcedureWebService ?? throw new ArgumentNullException(nameof(verticalOriginProcedureWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));

            this.CurrentStep = ProfileResolutionCalibrationStep.StartCalibration;
        }

        #endregion

        #region Properties

        public Bay Bay => this.MachineService.Bay;

        public bool BayIsShutterThreeSensors => this.MachineService.IsShutterThreeSensors;

        public BayPosition BayPosition => this.Bay.Carousel != null ? this.MachineService.Bay.Positions.OrderByDescending(o => o.Height).First() : this.MachineService.Bay.Positions.OrderBy(o => o.Height).First();

        public BayPosition BayPositionActive { get; set; }

        public IEnumerable<Bay> Bays { get; set; }

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

        public ProfileResolutionCalibrationStep CurrentStep
        {
            get => this.currentStep;
            protected set => this.SetProperty(ref this.currentStep, value, () => this.UpdateStatusButtonFooter(false));
        }

        public double? CyclesPercent
        {
            get => this.cyclesPercent;
            private set => this.SetProperty(ref this.cyclesPercent, value);
        }

        public bool HasBayDown => this.Bay?.Positions.Any(p => !p.IsUpper) == true;

        public bool HasBayExternal => this.MachineService.HasBayExternal;

        public bool HasBayUp => this.Bay?.Positions.Any(p => p.IsUpper) == true;

        public bool HasShutter => this.MachineService.HasShutter;

        public bool HasStepConfirmAdjustment => this.currentStep is ProfileResolutionCalibrationStep.ConfirmAdjustment;

        public bool HasStepRunningCalibration => this.currentStep is ProfileResolutionCalibrationStep.RunningCalibration;

        public bool HasStepStartCalibration => this.currentStep is ProfileResolutionCalibrationStep.StartCalibration;

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

        public bool IsUseWeightControl
        {
            get => this.isUseWeightControl;
            set => this.SetProperty(ref this.isUseWeightControl, value);
        }

        public bool IsVerticalCalibration
        {
            get => this.isVerticalCalibration;
            private set => this.SetProperty(ref this.isVerticalCalibration, value);
        }

        public ICommand MoveToConfirmAdjustmentCommand =>
            this.moveToConfirmAdjustmentCommand
            ??
            (this.moveToConfirmAdjustmentCommand = new DelegateCommand(
                () => this.CurrentStep = ProfileResolutionCalibrationStep.ConfirmAdjustment,
                () => this.CanBaseExecute()));

        public ICommand MoveToShutterCommand =>
            this.moveToShutterCommand
            ??
            (this.moveToShutterCommand = new DelegateCommand(
                async () => await this.MoveToShutterAsync(),
                () => this.CanMoveShutter()));

        public ICommand MoveToStartCalibrationCommand =>
                    this.moveToStartCalibrationCommand
            ??
            (this.moveToStartCalibrationCommand = new DelegateCommand(
                () => this.CurrentStep = ProfileResolutionCalibrationStep.StartCalibration,
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

        public double[] ProfileConst
        {
            get => this.profileConst;
            private set
            {
                this.SetProperty(ref this.profileConst, value);

                this.ProfileConst0 = value[0];
                this.ProfileConst1 = value[1];
            }
        }

        public double ProfileConst0 { get => this.profileConst0; set => this.SetProperty(ref this.profileConst0, value); }

        public double ProfileConst1 { get => this.profileConst1; set => this.SetProperty(ref this.profileConst1, value); }

        public int[] ProfileSamples
        {
            get => this.profileSamples;
            private set
            {
                this.SetProperty(ref this.profileSamples, value);

                this.ProfileSamples0 = value[0] / 100.0;
                this.ProfileSamples1 = value[1] / 100.0;
                this.ProfileSamples2 = value[2] / 100.0;
            }
        }

        public double ProfileSamples0 { get => this.profileSamples0; set => this.SetProperty(ref this.profileSamples0, value); }

        public double ProfileSamples1 { get => this.profileSamples1; set => this.SetProperty(ref this.profileSamples1, value); }

        public double ProfileSamples2 { get => this.profileSamples2; set => this.SetProperty(ref this.profileSamples2, value); }

        public ICommand RepeatCalibrationCommand =>
            this.repeatCalibrationCommand
            ??
            (this.repeatCalibrationCommand = new DelegateCommand(
                () =>
                {
                    this.CurrentStep = ProfileResolutionCalibrationStep.StartCalibration;
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
                       this.CurrentStep = ProfileResolutionCalibrationStep.StartCalibration;
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

        public ICommand VerticalCalibrationCommand =>
            this.verticalCalibrationCommand
            ??
            (this.verticalCalibrationCommand = new DelegateCommand(
                async () => await this.VerticalCalibrationAsync(),
                this.CanVerticalCalibration));

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

        public override async Task OnAppearedAsync()
        {
            this.Bays = this.MachineService.Bays;
            this.SubscribeToEvents();

            this.BayPositionActive = null;
            this.IsExecutingStopInPhase = false;
            this.IsVerticalCalibration = this.MachineService.IsHoming;
            this.IsShutterMoving = false;

            this.ShutterLabel = this.SensorsService.ShutterSensors.Open ? Localized.Get("InstallationApp.GateClose") : Localized.Get("InstallationApp.GateOpen");

            this.UpdateStatusButtonFooter(true);

            await base.OnAppearedAsync();
        }

        protected override async Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            await base.OnMachineStatusChangedAsync(e);

            if (!this.IsMoving)
            {
                this.isTuningVertical = false;
            }
        }

        protected void OnStepChanged(StepChangedMessage e)
        {
            switch (this.CurrentStep)
            {
                case ProfileResolutionCalibrationStep.StartCalibration:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileResolutionCalibrationStep.RunningCalibration;
                    }

                    break;

                case ProfileResolutionCalibrationStep.RunningCalibration:
                    if (!e.Next)
                    {
                        this.CurrentStep = ProfileResolutionCalibrationStep.StartCalibration;
                    }

                    break;

                case ProfileResolutionCalibrationStep.ConfirmAdjustment:
                    if (!e.Next)
                    {
                        this.CurrentStep = ProfileResolutionCalibrationStep.StartCalibration;
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
            this.verticalCalibrationCommand?.RaiseCanExecuteChanged();
            this.moveToConfirmAdjustmentCommand?.RaiseCanExecuteChanged();
            this.moveToStartCalibrationCommand?.RaiseCanExecuteChanged();
            this.moveToShutterCommand?.RaiseCanExecuteChanged();
            this.repeatCalibrationCommand?.RaiseCanExecuteChanged();

            this.ShutterLabel = this.SensorsService.ShutterSensors.Open ? Localized.Get("InstallationApp.GateClose") : Localized.Get("InstallationApp.GateOpen");

            this.RaisePropertyChanged(nameof(this.PerformedCycles));
            this.RaisePropertyChanged(nameof(this.CyclesPercent));
            this.RaisePropertyChanged(nameof(this.RequiredCycles));
            this.RaisePropertyChanged(nameof(this.IsExecutingProcedure));
            this.RaisePropertyChanged(nameof(this.IsExecutingStopInPhase));
            this.RaisePropertyChanged(nameof(this.IsCalibrationNotCompletedAndStopped));
        }

        private bool CanBaseExecute()
        {
            return !this.IsKeyboardOpened &&
                   !this.IsMoving;
        }

        private bool CanComplete()
        {
            return this.CanBaseExecute() && !this.IsCalibrationNotCompletedAndStopped;
        }

        private bool CanMoveShutter()
        {
            return this.CanBaseExecute()
                && (!this.SensorsService.ShutterSensors.Closed || this.MachineService.IsHoming);
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
            try
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
                    isOk = !this.SensorsService.IsLoadingUnitOnElevator;
                    if (!isOk)
                    {
                        this.ShowNotification(Localized.Get("InstallationApp.LuPresenceOnMachineCradleSide"), NotificationSeverity.Warning);
                    }
                }

                if (isOk)
                {
                    isOk = this.IsVerticalCalibration;
                    if (!isOk)
                    {
                        this.ShowNotification(Localized.Get("InstallationApp.OffsetVerticalAxisMenuDescription") + ", " + Localized.Get("InstallationApp.GateClose"), NotificationSeverity.Warning);
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

                return isOk;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CanStop()
        {
            if (this.CurrentStep == ProfileResolutionCalibrationStep.RunningCalibration)
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

        private bool CanVerticalCalibration()
        {
            return this.CanBaseExecute() &&
                   !this.MachineService.IsHoming &&
                   !this.IsVerticalCalibration &&
                   !this.MachineService.MachineStatus.IsMoving &&
                   !this.MachineService.MachineStatus.IsMovingLoadingUnit &&
                   !this.SensorsService.IsHorizontalInconsistentBothLow &&
                   !this.SensorsService.IsHorizontalInconsistentBothHigh &&
                   (this.SensorsService.ShutterSensors.Closed || (this.SensorsService.ShutterSensors.MidWay && this.MachineService.Bay.Shutter?.Type == MAS.AutomationService.Contracts.ShutterType.UpperHalf));
        }

        private async Task CompleteAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                if (Math.Abs(this.Bay.ProfileConst0 - this.ProfileConst[0]) > 50 || Math.Abs(this.Bay.ProfileConst1 - this.ProfileConst[1]) > 0.01)
                {
                    var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmDifferentValue"), Localized.Get("InstallationApp.ProfileResolutionCalibration"), DialogType.Question, DialogButtons.YesNo);
                    if (messageBoxResult == DialogResult.Yes)
                    {
                        this.IsExecutingStopInPhase = false;
                        this.IsExecutingProcedure = false;

                        await this.machineProfileProcedureWeb.SaveAsync();
                        this.Logger.Debug($"SetProfileConst: k0 = {this.ProfileConst[0]}; k1 = {this.ProfileConst[1]}; old k0 {this.Bay.ProfileConst0}; old k1 {this.Bay.ProfileConst1}");
                        await this.machineBaysWebService.SetProfileConstBayAsync(this.ProfileConst[0], this.ProfileConst[1]);

                        this.ShowNotification(Localized.Get("InstallationApp.InformationSuccessfullyUpdated"), NotificationSeverity.Success);
                        this.CurrentStep = ProfileResolutionCalibrationStep.StartCalibration;
                        this.NavigationService.GoBack();
                    }
                }
                else
                {
                    var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmCalibrationProcedure"), Localized.Get("InstallationApp.ProfileResolutionCalibration"), DialogType.Question, DialogButtons.YesNo);
                    if (messageBoxResult == DialogResult.Yes)
                    {
                        this.IsExecutingStopInPhase = false;
                        this.IsExecutingProcedure = false;

                        await this.machineProfileProcedureWeb.SaveAsync();
                        this.Logger.Debug($"SetProfileConst: k0 = {this.ProfileConst[0]}; k1 = {this.ProfileConst[1]}; old k0 {this.Bay.ProfileConst0}; old k1 {this.Bay.ProfileConst1}");
                        await this.machineBaysWebService.SetProfileConstBayAsync(this.ProfileConst[0], this.ProfileConst[1]);

                        this.ShowNotification(Localized.Get("InstallationApp.InformationSuccessfullyUpdated"), NotificationSeverity.Success);
                        this.CurrentStep = ProfileResolutionCalibrationStep.StartCalibration;
                        this.NavigationService.GoBack();
                    }
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

        private async Task MoveToShutterAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsShutterMoving = true;

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
            if (message.Data?.MovementMode != MovementMode.ProfileResolution)
            {
                return;
            }

            if (message.IsNotRunning())
            {
                this.IsExecutingProcedure = false;
            }

            if (message.IsErrored() || this.MachineError != null)
            {
                this.ShowNotification(Localized.Get("InstallationApp.ProcedureWasStopped"), NotificationSeverity.Warning);

                this.IsExecutingProcedure = false;

                this.IsCalibrationStopped = false;
                this.IsCalibrationCompleted = this.requiredCycles == this.performedCycles ? true : false;

                this.CurrentStep = ProfileResolutionCalibrationStep.ConfirmAdjustment;

                if (this.MachineError != null)
                {
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
                if (message.Data.ProfileSamples != null)
                {
                    this.ProfileSamples = message.Data.ProfileSamples;
                }
            }

            if (message.Status == MessageStatus.OperationEnd)
            {
                this.PerformedCycles = message.Data.ExecutedCycles;

                if (message.Data.IsTestStopped)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.TestStopped"), Services.Models.NotificationSeverity.Success);

                    this.IsCalibrationCompleted = false;

                    this.IsCalibrationStopped = true;
                }
                else
                {
                    this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.CompletedTest"), Services.Models.NotificationSeverity.Success);

                    this.IsCalibrationCompleted = this.requiredCycles == this.performedCycles ? true : false;

                    this.IsCalibrationStopped = false;

                    this.ProfileConst = message.Data.ProfileConst;
                    this.ProfileSamples = message.Data.ProfileSamples;
                }

                this.IsChainOffsetVisible = false;

                this.IsExecutingProcedure = false;

                this.CurrentStep = ProfileResolutionCalibrationStep.ConfirmAdjustment;

                this.RaiseCanExecuteChanged();
            }

            if (message.Status == MessageStatus.OperationStop)
            {
                this.IsCalibrationCompleted = false;

                this.IsExecutingStopInPhase = false;
                this.IsExecutingProcedure = false;
                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.ProcedureWasStopped"), Services.Models.NotificationSeverity.Warning);

                this.IsCalibrationStopped = true;
                this.PerformedCycles = message.Data.ExecutedCycles;

                this.SessionPerformedCycles = this.PerformedCycles - this.StartPerformedCycles;

                this.CurrentStep = ProfileResolutionCalibrationStep.ConfirmAdjustment;
                this.RaiseCanExecuteChanged();

                this.IsExecutingProcedure = false;
            }
            if (this.requiredCycles > 0)
            {
                this.CyclesPercent = (this.performedCycles * 100) / this.requiredCycles;
            }
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
                // Update procedure info
                this.StartPerformedCycles = 0;
                this.SessionPerformedCycles = 0;

                this.CyclesPercent = 0;

                this.startTime = DateTime.Now;

                await this.machineProfileProcedureWeb.ResolutionAsync(this.BayPosition.Id);

                this.RaiseCanExecuteChanged();

                this.CurrentStep = ProfileResolutionCalibrationStep.RunningCalibration;
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

        private void UpdateStatusButtonFooter(bool force = false)
        {
            if (!this.IsVisible && !force)
            {
                return;
            }

            switch (this.CurrentStep)
            {
                case ProfileResolutionCalibrationStep.StartCalibration:
                    this.ShowPrevStepSinglePage(true, false);
                    this.isErrorVisible = false;
                    this.IsExecutingStopInPhase = false;
                    break;

                case ProfileResolutionCalibrationStep.RunningCalibration:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, false);
                    break;

                case ProfileResolutionCalibrationStep.ConfirmAdjustment:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }

            this.ShowAbortStep(true, !this.IsMoving);

            this.RaisePropertyChanged(nameof(this.HasStepStartCalibration));
            this.RaisePropertyChanged(nameof(this.HasStepRunningCalibration));
            this.RaisePropertyChanged(nameof(this.HasStepConfirmAdjustment));
        }

        private async Task VerticalCalibrationAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.isTuningVertical = true;

                await this.verticalOriginProcedureWebService.StartAsync();

                this.IsVerticalCalibration = true;

                this.IsExecutingProcedure = true;
            }
            catch (Exception ex)
            {
                this.IsVerticalCalibration = false;
                this.ShowNotification(ex);
            }
            finally
            {
                this.isTuningVertical = false;
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}

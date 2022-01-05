using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Login;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Errors.ViewModels
{
    internal struct ErrorZeroSensorStepBay1Calibration
    {
    }

    internal struct ErrorZeroSensorStepElevatorCalibration
    {
    }

    internal struct ErrorZeroSensorStepLoadunitOnBay1
    {
    }

    internal struct ErrorZeroSensorStepLoadunitOnElevator
    {
    }

    internal struct ErrorZeroSensorStepStart
    {
    }

    internal sealed class ErrorZeroSensorViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineErrorsService machineErrorsService;

        private readonly IMachineErrorsWebService machineErrorsWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineModeWebService machineModeWebService;

        private readonly ISessionService sessionService;

        private bool calibrateStepVisible;

        private object currentStep = default(ErrorZeroSensorStepStart);

        private string errorTime;

        private bool findStepVisible;

        private DelegateCommand findZeroBayCommand;

        private DelegateCommand findZeroElevatorCommand;

        private MachineError machineError;

        private DelegateCommand markAsResolvedCommand;

        private DelegateCommand moveToNextCommand;

        private LoadingUnit selectedLoadingUnit;

        private bool startStepVisible;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        private SubscriptionToken themeChangedToken;

        #endregion

        #region Constructors

        public ErrorZeroSensorViewModel(
            ISessionService sessionService,
            IMachineModeWebService machineModeWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineBaysWebService machineBaysWebService,
            IMachineCarouselWebService machineCarouselWebService,
            IMachineErrorsWebService machineErrorsWebService,
            IMachineErrorsService machineErrorsService)
            : base(Services.PresentationMode.Menu | Services.PresentationMode.Installer | Services.PresentationMode.Operator)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
            this.machineModeWebService = machineModeWebService ?? throw new ArgumentNullException(nameof(machineModeWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new ArgumentNullException(nameof(machineCarouselWebService));
            this.machineErrorsService = machineErrorsService ?? throw new ArgumentNullException(nameof(machineErrorsService));

            this.CurrentStep = default(ErrorZeroSensorStepStart);
        }

        #endregion

        #region Properties

        public bool CalibrateStepVisible
        {
            get => this.calibrateStepVisible;
            set => this.SetProperty(ref this.calibrateStepVisible, value);
        }

        public object CurrentStep
        {
            get => this.currentStep;
            set => this.SetProperty(ref this.currentStep, value, this.UpdateStatusButtonFooter);
        }

        public override EnableMask EnableMask => EnableMask.MachineManualMode | EnableMask.MachinePoweredOn;

        public string ErrorTime
        {
            get => this.errorTime;
            set => this.SetProperty(ref this.errorTime, value);
        }

        public bool FindStepVisible
        {
            get => this.findStepVisible;
            set => this.SetProperty(ref this.findStepVisible, value);
        }

        public ICommand FindZeroBayCommand =>
            this.findZeroBayCommand
            ??
            (this.findZeroBayCommand = new DelegateCommand(
                async () => await this.FindZeroBayAsync(),
                this.CanFindZeroBayCommand));

        public ICommand FindZeroElevatorCommand =>
                    this.findZeroElevatorCommand
            ??
            (this.findZeroElevatorCommand = new DelegateCommand(
                async () => await this.FindZeroElevatorAsync(),
                this.CanFindZeroElevatorCommand));

        public bool HasStepBay1Calibration => this.currentStep is ErrorZeroSensorStepBay1Calibration;

        public bool HasStepElevatorCalibration => this.currentStep is ErrorZeroSensorStepElevatorCalibration;

        public bool HasStepLoadunitOnBay1 => this.currentStep is ErrorZeroSensorStepLoadunitOnBay1;

        public bool HasStepLoadunitOnElevator => this.currentStep is ErrorZeroSensorStepLoadunitOnElevator;

        public bool HasStepStart => this.currentStep is ErrorZeroSensorStepStart;

        public bool IsMoving
        {
            get => this.MachineService?.MachineStatus?.IsMoving ?? true;
        }

        public override bool KeepAlive => false;

        public MachineError MachineError
        {
            get => this.machineError;
            set => this.SetProperty(ref this.machineError, value, () => this.OnErrorChanged(null));
        }

        public ICommand MarkAsResolvedCommand =>
            this.markAsResolvedCommand
            ??
            (this.markAsResolvedCommand = new DelegateCommand(
                async () => await this.MarkAsResolvedAsync(),
                this.CanMarkAsResolved));

        public ICommand MoveToNextCommand =>
            this.moveToNextCommand
            ??
            (this.moveToNextCommand = new DelegateCommand(
                async () => await this.NextAsync(),
                () => this.CanBaseExecute() &&
                      this.MachineService.MachineMode != MachineMode.Automatic &&
                      this.MachineService.MachinePower == MachinePowerState.Powered));

        public LoadingUnit SelectedLoadingUnit
        {
            get => this.selectedLoadingUnit;
            private set => this.SetProperty(ref this.selectedLoadingUnit, value);
        }

        public bool StartStepVisible
        {
            get => this.startStepVisible;
            set => this.SetProperty(ref this.startStepVisible, value);
        }

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.MachineError = null;
            this.machineErrorsService.AutoNavigateOnError = true;

            if (this.stepChangedToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.stepChangedToken);
                this.stepChangedToken?.Dispose();
                this.stepChangedToken = null;
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

            await this.RetrieveErrorAsync();

            this.UpdateStatusButtonFooter();

            await base.OnAppearedAsync();

            this.machineErrorsService.AutoNavigateOnError = false;
        }

        protected override async Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            await base.OnMachineStatusChangedAsync(e);
            if (!this.IsMoving)
            {
                var newError = await this.machineErrorsWebService.GetCurrentAsync();
                if (newError != null && newError.Code != this.MachineError.Code)
                {
                    this.machineErrorsService.AutoNavigateOnError = true;
                }
                else
                {
                    if (!this.CanFindZeroElevator()
                        && this.HasStepLoadunitOnElevator)
                    {
                        await this.NextAsync();
                    }
                    else if (!this.CanFindZeroBay()
                        && this.HasStepLoadunitOnBay1)
                    {
                        await this.NextAsync();
                    }
                }
            }
            this.RaiseCanExecuteChanged();
            this.UpdateStatusButtonFooter();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.markAsResolvedCommand?.RaiseCanExecuteChanged();

            this.stopCommand?.RaiseCanExecuteChanged();
            this.moveToNextCommand?.RaiseCanExecuteChanged();
        }

        private async Task CalibrationBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.machineCarouselWebService.FindZeroAsync();
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

        private async Task CalibrationElevatorAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.machineElevatorWebService.SearchHorizontalZeroAsync();
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
            return
                !this.IsKeyboardOpened
                &&
                !this.IsMoving;
        }

        private bool CanCalibrateBay()
        {
            return this.SensorsService.BayZeroChain &&
                this.MachineService.Bays?.FirstOrDefault(a => a.Number == this.MachineService.BayNumber)?.Carousel != null;
        }

        private bool CanCalibrateElevator()
        {
            return this.SensorsService.IsZeroChain &&
                !this.SensorsService.Sensors.LuPresentInMachineSide &&
                !this.SensorsService.Sensors.LuPresentInOperatorSide;
        }

        private bool CanFindZeroBay()
        {
            return !this.SensorsService.BayZeroChain &&
                this.MachineService.Bays?.FirstOrDefault(a => a.Number == this.MachineService.BayNumber)?.Carousel != null;
        }

        private bool CanFindZeroBayCommand()
        {
            return this.CanFindZeroBay() &&
                this.MachineService.MachinePower == MachinePowerState.Powered;
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

        private bool CanMarkAsResolved()
        {
            return
                this.MachineError != null &&
                (this.MachineError.Code == (int)MachineErrorCode.LoadUnitMissingOnElevator ||
                 (this.MachineError.Code == (int)MachineErrorCode.LoadUnitMissingOnBay && this.MachineError.BayNumber == this.MachineService.BayNumber)) &&
                !this.IsWaitingForResponse;
        }

        private bool CanStop()
        {
            return
                this.IsMoving
                &&
                !this.IsWaitingForResponse;
        }

        private async Task FindZeroBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.machineCarouselWebService.FindLostZeroAsync();
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

        private async Task MarkAsResolvedAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineErrorsWebService.ResolveAllAsync();

                this.MachineError = await this.machineErrorsWebService.GetCurrentAsync();
                this.machineErrorsService.AutoNavigateOnError = true;

                this.CurrentStep = default(ErrorZeroSensorStepStart);
                this.StartStepVisible = true;
                this.FindStepVisible = false;
                this.CalibrateStepVisible = false;
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

        private async Task NextAsync()
        {
            this.StartStepVisible = false;
            this.FindStepVisible = false;
            this.CalibrateStepVisible = false;
            if (this.CurrentStep is ErrorZeroSensorStepStart)
            {
                if (this.CanFindZeroElevator())
                {
                    this.CurrentStep = default(ErrorZeroSensorStepLoadunitOnElevator);
                    this.FindStepVisible = true;
                }
                else if (this.CanFindZeroBay())
                {
                    this.CurrentStep = default(ErrorZeroSensorStepLoadunitOnBay1);
                    this.FindStepVisible = true;
                }
                else
                {
                    await this.MarkAsResolvedAsync();
                }
            }
            else if (this.CurrentStep is ErrorZeroSensorStepLoadunitOnElevator)
            {
                if (this.CanCalibrateElevator())
                {
                    this.CurrentStep = default(ErrorZeroSensorStepElevatorCalibration);
                    this.CalibrateStepVisible = true;
                    await this.CalibrationElevatorAsync();
                }
                else
                {
                    await this.MarkAsResolvedAsync();
                }
            }
            else if (this.CurrentStep is ErrorZeroSensorStepElevatorCalibration)
            {
                await this.MarkAsResolvedAsync();
            }
            else if (this.CurrentStep is ErrorZeroSensorStepLoadunitOnBay1)
            {
                if (this.CanCalibrateBay())
                {
                    this.CurrentStep = default(ErrorZeroSensorStepBay1Calibration);
                    this.CalibrateStepVisible = true;
                    await this.CalibrationBayAsync();
                }
                else
                {
                    await this.MarkAsResolvedAsync();
                }
            }
            else if (this.CurrentStep is ErrorZeroSensorStepBay1Calibration)
            {
                await this.MarkAsResolvedAsync();
            }
        }

        private void OnErrorChanged(object state)
        {
            if (this.MachineError is null)
            {
                this.ErrorTime = null;
                return;
            }

            var elapsedTime = DateTime.UtcNow - this.machineError.OccurrenceDate;
            if (elapsedTime.TotalMinutes < 1)
            {
                this.ErrorTime = Localized.Get("General.Now");
            }
            else if (elapsedTime.TotalHours < 1)
            {
                this.ErrorTime = string.Format(Localized.Get("General.MinutesAgo"), elapsedTime.TotalMinutes);
            }
            else if (elapsedTime.TotalDays < 1)
            {
                this.ErrorTime = string.Format(Localized.Get("General.HoursAgo"), elapsedTime.TotalHours);
            }
            else
            {
                this.ErrorTime = string.Format(Localized.Get("General.DaysAgo"), elapsedTime.TotalDays);
            }
        }

        private async Task OnStepChangedAsync(StepChangedMessage e)
        {
            this.StartStepVisible = false;
            this.FindStepVisible = false;
            this.CalibrateStepVisible = false;
            if (this.CurrentStep is ErrorZeroSensorStepStart)
            {
                if (e.Next)
                {
                    if (this.CanFindZeroElevator())
                    {
                        this.CurrentStep = default(ErrorZeroSensorStepLoadunitOnElevator);
                        this.FindStepVisible = true;
                    }
                    else if (this.CanFindZeroBay())
                    {
                        this.CurrentStep = default(ErrorZeroSensorStepLoadunitOnBay1);
                        this.FindStepVisible = true;
                    }
                    else
                    {
                        await this.MarkAsResolvedAsync();
                    }
                }
            }
            else if (this.CurrentStep is ErrorZeroSensorStepLoadunitOnElevator)
            {
                if (e.Next)
                {
                    if (this.CanCalibrateElevator())
                    {
                        this.CurrentStep = default(ErrorZeroSensorStepElevatorCalibration);
                        this.CalibrateStepVisible = true;
                        await this.CalibrationElevatorAsync();
                    }
                    else
                    {
                        await this.MarkAsResolvedAsync();
                    }
                }
                else
                {
                    this.CurrentStep = default(ErrorZeroSensorStepStart);
                    this.StartStepVisible = true;
                }
            }
            else if (this.CurrentStep is ErrorZeroSensorStepElevatorCalibration)
            {
                if (e.Next)
                {
                    await this.MarkAsResolvedAsync();
                }
                else
                {
                    this.CurrentStep = default(ErrorZeroSensorStepLoadunitOnElevator);
                    this.FindStepVisible = true;
                }
            }
            else if (this.CurrentStep is ErrorZeroSensorStepLoadunitOnBay1)
            {
                if (e.Next)
                {
                    if (this.CanCalibrateBay())
                    {
                        this.CurrentStep = default(ErrorZeroSensorStepBay1Calibration);
                        this.CalibrateStepVisible = true;
                        await this.CalibrationBayAsync();
                    }
                    else
                    {
                        await this.MarkAsResolvedAsync();
                    }
                }
                else
                {
                    this.CurrentStep = default(ErrorZeroSensorStepStart);
                    this.StartStepVisible = true;
                }
            }
            else if (this.CurrentStep is ErrorZeroSensorStepBay1Calibration)
            {
                if (e.Next)
                {
                    await this.MarkAsResolvedAsync();
                }
                else
                {
                    this.CurrentStep = default(ErrorZeroSensorStepLoadunitOnBay1);
                    this.FindStepVisible = true;
                }
            }

            this.RaiseCanExecuteChanged();
        }

        private async Task RetrieveErrorAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.StartStepVisible = true;
                this.FindStepVisible = false;
                this.CalibrateStepVisible = false;

                this.MachineError = await this.machineErrorsWebService.GetCurrentAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is HttpRequestException)
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
                        async (m) => await this.OnStepChangedAsync(m),
                        ThreadOption.UIThread,
                        false);

            this.themeChangedToken = this.themeChangedToken
               ?? this.EventAggregator
                   .GetEvent<ThemeChangedPubSubEvent>()
                   .Subscribe(
                       (m) =>
                       {
                           this.RaisePropertyChanged(nameof(this.HasStepStart));
                           this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnElevator));
                           this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnBay1));
                       },
                       ThreadOption.UIThread,
                       false);
        }

        private void UpdateStatusButtonFooter()
        {
            if (this.CurrentStep is ErrorZeroSensorStepStart)
            {
                this.ShowPrevStepSinglePage(true, false);
                this.ShowNextStepSinglePage(true, this.moveToNextCommand?.CanExecute() ?? false);
            }
            else if (this.CurrentStep is ErrorZeroSensorStepLoadunitOnElevator)
            {
                this.ShowPrevStepSinglePage(true, !this.IsMoving);
                this.ShowNextStepSinglePage(true, this.moveToNextCommand?.CanExecute() ?? false);
            }
            else if (this.CurrentStep is ErrorZeroSensorStepLoadunitOnBay1)
            {
                this.ShowPrevStepSinglePage(true, !this.IsMoving);
                this.ShowNextStepSinglePage(true, this.moveToNextCommand?.CanExecute() ?? false);
            }
            else if (this.CurrentStep is ErrorZeroSensorStepElevatorCalibration)
            {
                this.ShowPrevStepSinglePage(true, !this.IsMoving);
                this.ShowNextStepSinglePage(true, false);
            }
            else if (this.CurrentStep is ErrorZeroSensorStepBay1Calibration)
            {
                this.ShowPrevStepSinglePage(true, !this.IsMoving);
                this.ShowNextStepSinglePage(true, false);
            }

            this.ShowAbortStep(true, !this.IsMoving);

            this.RaisePropertyChanged(nameof(this.HasStepStart));
            this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnElevator));
            this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnBay1));
        }

        #endregion
    }
}

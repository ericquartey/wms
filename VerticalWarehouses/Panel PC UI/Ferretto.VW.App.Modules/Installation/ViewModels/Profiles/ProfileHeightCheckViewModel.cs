using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum ProfileCheckStep
    {
        Initialize,

        ElevatorPosition,

        ShapePositionDx,

        TuningChainDx,

        ShapePositionSx,

        TuningChainSx,

        ResultCheck,
    }

    [Warning(WarningsArea.Installation)]
    internal class ProfileHeightCheckViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private const double policyVerticalTolerance = 0.01;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineProfileProcedureWebService machineProfileProcedureWeb;

        private readonly IMachineShuttersWebService shuttersWebService;

        private string adminCalibration = "Collapsed";

        private DelegateCommand callLoadunitToBayCommand;

        private bool canLoadingUnitId;

        private DelegateCommand closeShutterCommand;

        private DelegateCommand completedCommand;

        private string currentError;

        private ProfileCheckStep currentStep;

        private DelegateCommand goToBayCommand;

        private int? loadingUnitId;

        private double? measuredDx;

        private double? measuredSx;

        private DelegateCommand mensurationDxCommand;

        private DelegateCommand mensurationSxCommand;

        private DelegateCommand moveToElevatorPositionCommand;

        private DelegateCommand moveToShapePositionDxCommand;

        private DelegateCommand openShutterCommand;

        private double? profileCalibrateDistanceDx;

        private double? profileCalibrateDistanceSx;

        private SubscriptionToken profileCalibrationToken;

        private double profileCorrectDistance;

        private double profileDegrees;

        private double? profileStartDistanceDx;

        private double? profileStartDistanceSx;

        private double profileTotalDistance;

        private DelegateCommand repeatCommand;

        private DelegateCommand saveParametersCommand;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        private SubscriptionToken themeChangedToken;

        #endregion

        #region Constructors

        public ProfileHeightCheckViewModel(
            IEventAggregator eventAggregator,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineShuttersWebService shuttersWebService,
            IMachineProfileProcedureWebService machineProfileProcedureWeb)
            : base(PresentationMode.Installer)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.shuttersWebService = shuttersWebService ?? throw new ArgumentNullException(nameof(shuttersWebService));
            this.machineProfileProcedureWeb = machineProfileProcedureWeb ?? throw new ArgumentNullException(nameof(machineProfileProcedureWeb));

            this.CurrentStep = ProfileCheckStep.Initialize;

            var sessionService = CommonServiceLocator.ServiceLocator.Current.GetInstance<ISessionService>();

            if (sessionService.UserAccessLevel == MAS.AutomationService.Contracts.UserAccessLevel.Admin)
            {
                this.AdminCalibration = "Visible";
            }
        }

        #endregion

        #region Properties

        public string AdminCalibration
        {
            get => this.adminCalibration;
            private set => this.SetProperty(ref this.adminCalibration, value);
        }

        public BayPosition BayPosition => this.MachineService.Bay.Positions.OrderByDescending(o => o.Height).First();

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

        public ICommand CloseShutterCommand =>
            this.closeShutterCommand
            ??
            (this.closeShutterCommand = new DelegateCommand(
                async () => await this.CloseShutterAsync(),
                this.CanCloseShutter));

        public ICommand CompletedCommand =>
            this.completedCommand
            ??
            (this.completedCommand = new DelegateCommand(
                async () => await this.CompleteProcedureAsync(),
                this.CanCompleteProcedure));

        public ProfileCheckStep CurrentStep
        {
            get => this.currentStep;
            protected set => this.SetProperty(ref this.currentStep, value, this.UpdateStatusButtonFooter);
        }

        public override EnableMask EnableMask => EnableMask.MachineManualMode | EnableMask.MachinePoweredOn;

        public string Error => string.Join(
            Environment.NewLine,
            this.GetType().GetProperties()
                .Select(p => this[p.Name])
                .Distinct()
                .Where(s => !string.IsNullOrEmpty(s)));

        public ICommand GoToBayCommand =>
            this.goToBayCommand
            ??
            (this.goToBayCommand = new DelegateCommand(
                async () => await this.GoToBayCommandAsync(),
                this.CanGoToBayCommand));

        public bool HasShutter => this.MachineService.HasShutter;

        public bool HasStepElevatorPosition => this.currentStep is ProfileCheckStep.ElevatorPosition;

        public bool HasStepInitialize => this.currentStep is ProfileCheckStep.Initialize;

        public bool HasStepResultCheck => this.currentStep is ProfileCheckStep.ResultCheck;

        public bool HasStepShapePositionDx => this.currentStep is ProfileCheckStep.ShapePositionDx;

        public bool HasStepShapePositionSx => this.currentStep is ProfileCheckStep.ShapePositionSx;

        public bool HasStepTuningChainDx => this.currentStep is ProfileCheckStep.TuningChainDx;

        public bool HasStepTuningChainSx => this.currentStep is ProfileCheckStep.TuningChainSx;

        public override bool KeepAlive => false;

        public int? LoadingUnitId
        {
            get => this.loadingUnitId;
            set => this.SetProperty(ref this.loadingUnitId, value, this.RaiseCanExecuteChanged);
        }

        public double? MeasuredDx
        {
            get => this.measuredDx;
            set => this.SetProperty(ref this.measuredDx, value, this.RaiseCanExecuteChanged);
        }

        public double? MeasuredSx
        {
            get => this.measuredSx;
            set => this.SetProperty(ref this.measuredSx, value, this.RaiseCanExecuteChanged);
        }

        public ICommand MensurationDxCommand =>
            this.mensurationDxCommand
            ??
            (this.mensurationDxCommand = new DelegateCommand(
                async () => await this.MensurationDxAsync(),
                this.CanMensurationDx));

        public ICommand MensurationSxCommand =>
            this.mensurationSxCommand
            ??
            (this.mensurationSxCommand = new DelegateCommand(
                async () => await this.MensurationSxAsync(),
                this.CanMensurationSx));

        public ICommand MoveToElevatorPositionCommand =>
            this.moveToElevatorPositionCommand
            ??
            (this.moveToElevatorPositionCommand = new DelegateCommand(
                () => this.CurrentStep = ProfileCheckStep.ElevatorPosition,
                this.CanMoveToElevatorPosition));

        public ICommand MoveToShapePositionDxCommand =>
            this.moveToShapePositionDxCommand
            ??
            (this.moveToShapePositionDxCommand = new DelegateCommand(
                () => this.CurrentStep = ProfileCheckStep.ShapePositionDx,
                this.CanMoveToShapePositionDx));

        public ICommand OpenShutterCommand =>
            this.openShutterCommand
            ??
            (this.openShutterCommand = new DelegateCommand(
                async () => await this.OpenShutterAsync(),
                this.CanOpenShutter));

        public double? ProfileCalibrateDistanceDx
        {
            get => this.profileCalibrateDistanceDx;
            set => this.SetProperty(ref this.profileCalibrateDistanceDx, value, this.RaiseCanExecuteChanged);
        }

        public double? ProfileCalibrateDistanceSx
        {
            get => this.profileCalibrateDistanceSx;
            set => this.SetProperty(ref this.profileCalibrateDistanceSx, value, this.RaiseCanExecuteChanged);
        }

        public double ProfileCorrectDistance
        {
            get => this.profileCorrectDistance;
            set
            {
                if (this.SetProperty(ref this.profileCorrectDistance, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public double ProfileDegrees
        {
            get => this.profileDegrees;
            set
            {
                if (this.SetProperty(ref this.profileDegrees, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public double? ProfileStartDistanceDx
        {
            get => this.profileStartDistanceDx;
            set => this.SetProperty(ref this.profileStartDistanceDx, value, this.RaiseCanExecuteChanged);
        }

        public double? ProfileStartDistanceSx
        {
            get => this.profileStartDistanceSx;
            set => this.SetProperty(ref this.profileStartDistanceSx, value, this.RaiseCanExecuteChanged);
        }

        public double ProfileTotalDistance
        {
            get => this.profileTotalDistance;
            set
            {
                if (this.SetProperty(ref this.profileTotalDistance, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand RepeatCommand =>
            this.repeatCommand
            ??
            (this.repeatCommand = new DelegateCommand(
                () => this.CurrentStep = ProfileCheckStep.ShapePositionDx));

        public ICommand SaveParametersCommand =>
                                                                                                                                                                                                                                                    this.saveParametersCommand
            ??
            (this.saveParametersCommand = new DelegateCommand(
                async () => await this.UpdateParameterAsync()));

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

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

            if (this.stepChangedToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.stepChangedToken);
                this.stepChangedToken?.Dispose();
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

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                var parameters = await this.machineProfileProcedureWeb.GetParametersAsync();
                this.ProfileTotalDistance = parameters.ProfileTotalDistance;
                this.ProfileDegrees = parameters.ProfileDegrees;
                this.ProfileCorrectDistance = parameters.ProfileCorrectDistance;
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

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            if (e.MachinePowerState == MAS.AutomationService.Contracts.MachinePowerState.Unpowered &&
                this.MachineError is null)
            {
                this.CurrentStep = ProfileCheckStep.Initialize;
            }
        }

        protected void OnStepChanged(StepChangedMessage e)
        {
            switch (this.CurrentStep)
            {
                case ProfileCheckStep.Initialize:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.ElevatorPosition;
                    }

                    break;

                case ProfileCheckStep.ElevatorPosition:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.ShapePositionDx;
                    }
                    else
                    {
                        this.CurrentStep = ProfileCheckStep.Initialize;
                    }

                    break;

                case ProfileCheckStep.ShapePositionDx:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.TuningChainDx;
                    }
                    else
                    {
                        this.CurrentStep = ProfileCheckStep.ElevatorPosition;
                    }

                    break;

                case ProfileCheckStep.TuningChainDx:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.ShapePositionSx;
                    }
                    else
                    {
                        this.CurrentStep = ProfileCheckStep.ShapePositionDx;
                    }

                    break;

                case ProfileCheckStep.ShapePositionSx:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.TuningChainSx;
                    }
                    else
                    {
                        this.CurrentStep = ProfileCheckStep.ShapePositionDx;
                    }

                    break;

                case ProfileCheckStep.TuningChainSx:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.ResultCheck;
                    }
                    else
                    {
                        this.CurrentStep = ProfileCheckStep.ShapePositionSx;
                    }

                    break;

                case ProfileCheckStep.ResultCheck:
                    if (!e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.ShapePositionSx;
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

            this.moveToElevatorPositionCommand?.RaiseCanExecuteChanged();
            this.callLoadunitToBayCommand?.RaiseCanExecuteChanged();
            this.openShutterCommand?.RaiseCanExecuteChanged();
            this.closeShutterCommand?.RaiseCanExecuteChanged();
            this.goToBayCommand?.RaiseCanExecuteChanged();
            this.moveToShapePositionDxCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();
            this.mensurationSxCommand?.RaiseCanExecuteChanged();
            this.mensurationDxCommand?.RaiseCanExecuteChanged();
            this.completedCommand?.RaiseCanExecuteChanged();

            this.UpdateStatusButtonFooter();
        }

        private async Task CallLoadunitToBayCommandAsync()
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

        private bool CanCallLoadunitToBay()
        {
            return this.CanBaseExecute() &&
                   !this.SensorsService.IsLoadingUnitInBay &&
                   !this.MachineService.Loadunits.DrawerInBay() &&
                   string.IsNullOrEmpty(this.Error);
        }

        private bool CanCloseShutter()
        {
            return this.CanBaseExecute() &&
                   this.SensorsService.ShutterSensors.Open;
        }

        private bool CanCompleteProcedure()
        {
            if ((this.measuredDx < 1.00) && (this.measuredSx < 1.00))
            {
                return true;
            }
            else
            {
                this.ShowNotification(VW.App.Resources.Localized.Get("ErrorsApp.CalibrationBarierError"));
                return false;
            }
        }

        private bool CanGoToBayCommand()
        {
            return this.CanBaseExecute() &&
                   (this.MachineStatus.ElevatorPositionType != ElevatorPositionType.Bay ||
                    !this.MachineStatus.BayPositionUpper.GetValueOrDefault() ||
                    !(Math.Abs(this.BayPosition.Height - this.MachineStatus?.ElevatorVerticalPosition ?? 0d) < policyVerticalTolerance));
        }

        private bool CanMensurationDx()
        {
            return this.CanBaseExecute() &&
                   (this.SensorsService.ShutterSensors.Open || !this.HasShutter);
        }

        private bool CanMensurationSx()
        {
            return this.CanBaseExecute() &&
                   (this.SensorsService.ShutterSensors.Open || !this.HasShutter);
        }

        private bool CanMoveToElevatorPosition()
        {
            return this.CanBaseExecute() &&
                   this.SensorsService.IsLoadingUnitInBay &&
                   (string.IsNullOrEmpty(this.Error) || !this.MachineService.Loadunits.Any());
        }

        private bool CanMoveToShapePositionDx()
        {
            var res = this.CanBaseExecute() &&
                      this.MachineStatus.ElevatorPositionType == ElevatorPositionType.Bay &&
                      this.MachineStatus.BayPositionUpper.GetValueOrDefault();

            this.ShowNextStepSinglePage(true, res);

            return res;
        }

        private bool CanOpenShutter()
        {
            return this.CanBaseExecute() &&
                   (this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay);
        }

        private bool CanStop()
        {
            return this.IsMoving;
        }

        private async Task CloseShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                var bay = this.MachineService.Bay;
                var closePosition = (bay.Shutter.Type == MAS.AutomationService.Contracts.ShutterType.ThreeSensors) ? MAS.AutomationService.Contracts.ShutterPosition.Half : MAS.AutomationService.Contracts.ShutterPosition.Closed;
                await this.shuttersWebService.MoveToAsync(closePosition);
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

        private async Task CompleteProcedureAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.machineProfileProcedureWeb.SaveAsync();

                this.CurrentStep = ProfileCheckStep.Initialize;

                this.NavigationService.GoBack();
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

        private async Task GoToBayCommandAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.machineElevatorWebService.MoveToBayPositionAsync(
                    this.BayPosition.Id,
                    computeElongation: false,
                    performWeighting: false);
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

        private async Task MensurationDxAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.machineProfileProcedureWeb.CalibrationAsync(this.BayPosition.Id);

                this.CurrentStep = ProfileCheckStep.TuningChainDx;
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

        private async Task MensurationSxAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.machineProfileProcedureWeb.CalibrationAsync(this.BayPosition.Id);

                this.CurrentStep = ProfileCheckStep.TuningChainSx;
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

        private async void OnProfileCalibrationMessageAsync(NotificationMessageUI<ProfileCalibrationMessageData> message)
        {
            var data = message.Data as ProfileCalibrationMessageData;
            if (this.CurrentStep == ProfileCheckStep.TuningChainDx)
            {
                this.ProfileStartDistanceDx = data.ProfileStartDistance;
                this.ProfileCalibrateDistanceDx = data.ProfileCalibrateDistance;

                this.MeasuredDx = data.Measured;
                if (this.HasShutter)
                {
                    this.CurrentStep = ProfileCheckStep.ShapePositionSx;
                    var bay = this.MachineService.Bay;
                    var closePosition = (bay.Shutter.Type == MAS.AutomationService.Contracts.ShutterType.ThreeSensors) ? MAS.AutomationService.Contracts.ShutterPosition.Half : MAS.AutomationService.Contracts.ShutterPosition.Closed;
                    await this.shuttersWebService.MoveToAsync(closePosition);
                }
                else
                {
                    this.CurrentStep = ProfileCheckStep.ShapePositionSx;
                }
            }
            else
            {
                this.ProfileStartDistanceSx = data.ProfileStartDistance;
                this.ProfileCalibrateDistanceSx = data.ProfileCalibrateDistance;

                this.MeasuredSx = data.Measured;

                if (this.HasShutter)
                {
                    this.CurrentStep = ProfileCheckStep.ResultCheck;
                    var bay = this.MachineService.Bay;
                    var closePosition = (bay.Shutter.Type == MAS.AutomationService.Contracts.ShutterType.ThreeSensors) ? MAS.AutomationService.Contracts.ShutterPosition.Half : MAS.AutomationService.Contracts.ShutterPosition.Closed;
                    await this.shuttersWebService.MoveToAsync(closePosition);
                }
                else
                {
                    this.CurrentStep = ProfileCheckStep.ResultCheck;
                }
            }
        }

        private async Task OpenShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersWebService.MoveToAsync(MAS.AutomationService.Contracts.ShutterPosition.Opened);
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
            catch (Exception ex)
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
                        (m) => this.OnStepChanged(m),
                        ThreadOption.UIThread,
                        false);

            this.profileCalibrationToken = this.profileCalibrationToken
                ?? this.eventAggregator
                    .GetEvent<NotificationEventUI<ProfileCalibrationMessageData>>()
                    .Subscribe(
                    this.OnProfileCalibrationMessageAsync,
                        ThreadOption.UIThread,
                        false);

            this.themeChangedToken = this.themeChangedToken
               ?? this.EventAggregator
                   .GetEvent<ThemeChangedPubSubEvent>()
                   .Subscribe(
                       (m) =>
                       {
                           this.RaisePropertyChanged(nameof(this.HasStepInitialize));
                           this.RaisePropertyChanged(nameof(this.HasStepElevatorPosition));
                           this.RaisePropertyChanged(nameof(this.HasStepShapePositionDx));
                           this.RaisePropertyChanged(nameof(this.HasStepTuningChainDx));
                           this.RaisePropertyChanged(nameof(this.HasStepShapePositionSx));
                           this.RaisePropertyChanged(nameof(this.HasStepTuningChainSx));
                           this.RaisePropertyChanged(nameof(this.HasStepResultCheck));
                       },
                       ThreadOption.UIThread,
                       false);
        }

        private async Task UpdateParameterAsync()
        {
            try
            {
                //da inserire il salvataggio
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

        private void UpdateStatusButtonFooter()
        {
            if (!this.IsVisible)
            {
                return;
            }

            switch (this.CurrentStep)
            {
                case ProfileCheckStep.Initialize:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, this.moveToElevatorPositionCommand?.CanExecute() ?? false);
                    break;

                case ProfileCheckStep.ElevatorPosition:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, this.moveToShapePositionDxCommand?.CanExecute() ?? false);
                    break;

                case ProfileCheckStep.TuningChainSx:
                case ProfileCheckStep.TuningChainDx:
                case ProfileCheckStep.ResultCheck:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, false);
                    break;

                default:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }

            this.ShowAbortStep(true, true);

            this.RaisePropertyChanged(nameof(this.HasStepInitialize));
            this.RaisePropertyChanged(nameof(this.HasStepElevatorPosition));
            this.RaisePropertyChanged(nameof(this.HasStepShapePositionDx));
            this.RaisePropertyChanged(nameof(this.HasStepTuningChainDx));
            this.RaisePropertyChanged(nameof(this.HasStepShapePositionSx));
            this.RaisePropertyChanged(nameof(this.HasStepTuningChainSx));
            this.RaisePropertyChanged(nameof(this.HasStepResultCheck));
        }

        #endregion
    }
}

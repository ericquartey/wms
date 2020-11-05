using System;
using System.Collections.Generic;
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
using Ferretto.VW.Utils.Maths;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum WeightCalibartionStep
    {
        CallUnit,

        EmptyUnitWeighing,

        OptionalWeighing1,

        OptionalWeighing2,

        OptionalWeighing3,

        FullUnitWeighing,

        SetWeight
    }

    public struct WeightData
    {
        #region Properties

        public double Current { get; set; }

        public double NetWeight { get; set; }

        #endregion
    }

    [Warning(WarningsArea.Installation)]
    public class WeightCalibrationViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly Services.IDialogService dialogService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineShuttersWebService shuttersWebService;

        private DelegateCommand callLoadunitToBayCommand;

        private double current;

        private WeightCalibartionStep currentStep;

        private DelegateCommand forwardCommand;

        private bool isBusyLoadingFromBay;

        private bool isBusyUnloadingToBay;

        private bool isEnabledForwards;

        private DelegateCommand loadFromBayCommand;

        private int loadingUnitId;

        private double measureConst0;

        private double measureConst1;

        private double measureConst2;

        private double netWeight;

        private SubscriptionToken positioningMessageReceivedToken;

        private DelegateCommand retryCommand;

        private DelegateCommand saveCommand;

        private LoadingUnit selectedLoadingUnit;

        private DelegateCommand skipCommand;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        private SubscriptionToken themeChangedToken;

        private List<WeightData> unitsWeighing = new List<WeightData>();

        private DelegateCommand unloadToBayCommand;

        #endregion

        #region Constructors

        public WeightCalibrationViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IEventAggregator eventAggregator,
            IMachineElevatorWebService machineElevatorWebService,
            IDialogService dialogService,
            IMachineShuttersWebService shuttersWebService,
            IMachineCarouselWebService machineCarouselWebService,
            IMachineBaysWebService machineBaysWebService,
            IBayManager bayManager)
          : base(PresentationMode.Installer)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.shuttersWebService = shuttersWebService ?? throw new ArgumentNullException(nameof(shuttersWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new ArgumentNullException(nameof(machineCarouselWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));

            this.CurrentStep = WeightCalibartionStep.CallUnit;
        }

        #endregion

        #region Properties

        private Bay Bay => this.MachineService.Bay;

        public ICommand CallLoadunitToBayCommand =>
            this.callLoadunitToBayCommand
            ??
            (this.callLoadunitToBayCommand = new DelegateCommand(
                async () => await this.CallLoadunitToBayCommandAsync(),
                this.CanCallLoadunitToBay));

        public double Current
        {
            get => this.current;
            set => this.SetProperty(ref this.current, value, this.RaiseCanExecuteChanged);
        }

        public WeightCalibartionStep CurrentStep
        {
            get => this.currentStep;
            set => this.SetProperty(ref this.currentStep, value, () => this.UpdateStatusButtonFooter(false));
        }

        public ICommand ForwardCommand =>
            this.forwardCommand
            ??
            (this.forwardCommand = new DelegateCommand(
                () => this.GoForward(),
                this.CanGoForward));

        public bool HasBayExternal => this.MachineService.HasBayExternal;

        public bool HasCarousel => this.MachineService.HasCarousel;

        public bool HasShutter => this.MachineService.HasShutter;

        public bool HasStepCallUnit => this.currentStep is WeightCalibartionStep.CallUnit;

        public bool HasStepEmptyUnitWeighing => this.currentStep is WeightCalibartionStep.EmptyUnitWeighing;

        public bool HasStepFullUnitWeighing => this.currentStep is WeightCalibartionStep.FullUnitWeighing;

        public bool HasStepOptionalWeighing1 => this.currentStep is WeightCalibartionStep.OptionalWeighing1;

        public bool HasStepOptionalWeighing2 => this.currentStep is WeightCalibartionStep.OptionalWeighing2;

        public bool HasStepOptionalWeighing3 => this.currentStep is WeightCalibartionStep.OptionalWeighing3;

        public bool HasStepSetWeight => this.currentStep is WeightCalibartionStep.SetWeight;

        public bool IsBusyLoadingFromBay
        {
            get => this.isBusyLoadingFromBay;
            private set => this.SetProperty(ref this.isBusyLoadingFromBay, value);
        }

        public bool IsBusyUnloadingToBay
        {
            get => this.isBusyUnloadingToBay;
            private set => this.SetProperty(ref this.isBusyUnloadingToBay, value);
        }

        public bool IsEnabledForwards
        {
            get => this.isEnabledForwards;
            set => this.SetProperty(ref this.isEnabledForwards, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMoving => (this.MachineService?.MachineStatus?.IsMoving ?? true) || (this.MachineService?.MachineStatus?.IsMovingLoadingUnit ?? true);

        public ICommand LoadFromBayCommand => this.loadFromBayCommand
                    ??
            (this.loadFromBayCommand = new DelegateCommand(
                async () => await this.LoadFromBayAsync(),
                this.CanLoadFromBay));

        public int LoadingUnitId
        {
            get => this.loadingUnitId;
            set => this.SetProperty(ref this.loadingUnitId, value, this.InputLoadingUnitIdPropertyChanged);
        }

        public double MeasureConst0
        {
            get => this.measureConst0;
            set => this.SetProperty(ref this.measureConst0, value);
        }

        public double MeasureConst1
        {
            get => this.measureConst1;
            set => this.SetProperty(ref this.measureConst1, value);
        }

        public double MeasureConst2
        {
            get => this.measureConst2;
            set => this.SetProperty(ref this.measureConst2, value);
        }

        public double NetWeight
        {
            get => this.netWeight;
            set => this.SetProperty(ref this.netWeight, value, this.RaiseCanExecuteChanged);
        }

        public ICommand RetryCommand =>
           this.retryCommand ?? (this.retryCommand =
           new DelegateCommand(
               () => this.GoRetryCommand(),
               this.CanBaseExecute));

        public ICommand SaveCommand => this.saveCommand
                           ??
           (this.saveCommand = new DelegateCommand(
               async () => await this.SaveAsync(),
               this.CanBaseExecute));

        public LoadingUnit SelectedLoadingUnit
        {
            get => this.selectedLoadingUnit;
            set => this.SetProperty(ref this.selectedLoadingUnit, value, this.RaiseCanExecuteChanged);
        }

        public ICommand SkipCommand =>
            this.skipCommand ?? (this.skipCommand =
            new DelegateCommand(
                () => this.Skip(),
                this.CanBaseExecute));

        public ICommand StopCommand =>
                            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                () => !this.IsMoving));

        public List<WeightData> UnitsWeighing
        {
            get => this.unitsWeighing;
            set => this.SetProperty(ref this.unitsWeighing, value);
        }

        public ICommand UnloadToBayCommand =>
                                    this.unloadToBayCommand ?? (this.unloadToBayCommand =
            new DelegateCommand(
                async () => await this.UnloadToBayAsync(),
                this.CanUnloadToBay));

        private IEnumerable<LoadingUnit> LoadingUnits => this.MachineService.Loadunits;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.positioningMessageReceivedToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.positioningMessageReceivedToken);
                this.positioningMessageReceivedToken.Dispose();
                this.positioningMessageReceivedToken = null;
            }

            if (this.stepChangedToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.stepChangedToken);
                this.stepChangedToken.Dispose();
                this.stepChangedToken = null;
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

            if (this.CurrentStep == WeightCalibartionStep.CallUnit)
            {
                this.unitsWeighing.Clear();
            }

            this.UpdateStatusButtonFooter(true);

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
        }

        protected void OnStepChanged(StepChangedMessage e)
        {
            switch (this.CurrentStep)
            {
                case WeightCalibartionStep.CallUnit:
                    if (e.Next)
                    {
                        this.CurrentStep = WeightCalibartionStep.EmptyUnitWeighing;
                    }

                    break;

                case WeightCalibartionStep.EmptyUnitWeighing:
                    if (e.Next)
                    {
                        this.CurrentStep = WeightCalibartionStep.OptionalWeighing1;
                    }
                    else
                    {
                        this.CurrentStep = WeightCalibartionStep.CallUnit;
                    }

                    break;

                case WeightCalibartionStep.OptionalWeighing1:
                    if (e.Next)
                    {
                        this.CurrentStep = WeightCalibartionStep.OptionalWeighing2;
                    }
                    else
                    {
                        this.CurrentStep = WeightCalibartionStep.EmptyUnitWeighing;
                    }

                    break;

                case WeightCalibartionStep.OptionalWeighing2:
                    if (e.Next)
                    {
                        this.CurrentStep = WeightCalibartionStep.OptionalWeighing3;
                    }
                    else
                    {
                        this.CurrentStep = WeightCalibartionStep.OptionalWeighing1;
                    }

                    break;

                case WeightCalibartionStep.OptionalWeighing3:
                    if (e.Next)
                    {
                        this.CurrentStep = WeightCalibartionStep.FullUnitWeighing;
                    }
                    else
                    {
                        this.CurrentStep = WeightCalibartionStep.OptionalWeighing2;
                    }

                    break;

                case WeightCalibartionStep.FullUnitWeighing:
                    if (e.Next)
                    {
                        this.CurrentStep = WeightCalibartionStep.SetWeight;
                    }
                    else
                    {
                        this.CurrentStep = WeightCalibartionStep.OptionalWeighing3;
                    }

                    break;

                case WeightCalibartionStep.SetWeight:
                    if (!e.Next)
                    {
                        this.CurrentStep = WeightCalibartionStep.FullUnitWeighing;
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

            this.callLoadunitToBayCommand?.RaiseCanExecuteChanged();

            this.forwardCommand?.RaiseCanExecuteChanged();

            this.stopCommand?.RaiseCanExecuteChanged();

            this.loadFromBayCommand?.RaiseCanExecuteChanged();

            this.unloadToBayCommand?.RaiseCanExecuteChanged();

            this.skipCommand?.RaiseCanExecuteChanged();

            this.saveCommand?.RaiseCanExecuteChanged();

            this.UpdateStatusButtonFooter();
        }

        private async Task CallLoadunitToBayCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.EjectLoadingUnitAsync(this.MachineService.GetBayPositionSourceByDestination(false), this.LoadingUnitId);
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
                   this.SelectedLoadingUnit != null &&
                   this.SelectedLoadingUnit.CellId != null &&
                   this.SelectedLoadingUnit.Height > 0;
        }

        private bool CanGoForward()
        {
            switch (this.currentStep)
            {
                case WeightCalibartionStep.CallUnit:
                    this.IsEnabledForwards = !this.IsMachineMoving &&
                        this.SensorsService.IsLoadingUnitInBay &&
                        this.MachineService.Loadunits.DrawerInBay();
                    break;

                case WeightCalibartionStep.EmptyUnitWeighing:
                    this.IsEnabledForwards = !this.IsMachineMoving &&
                        this.SensorsService.IsLoadingUnitInBay &&
                        this.MachineService.Loadunits.DrawerInBay() &&
                        this.current != null &&
                        this.netWeight != null &&
                        this.current > 0;
                    break;

                case WeightCalibartionStep.FullUnitWeighing:
                    this.IsEnabledForwards = !this.IsMachineMoving &&
                        this.SensorsService.IsLoadingUnitInBay &&
                        this.MachineService.Loadunits.DrawerInBay() &&
                        this.current != null &&
                        this.netWeight != null &&
                        this.netWeight > 0 &&
                        this.current > 0;
                    break;

                case WeightCalibartionStep.OptionalWeighing1:
                    this.IsEnabledForwards = !this.IsMachineMoving &&
                        this.SensorsService.IsLoadingUnitInBay &&
                        this.MachineService.Loadunits.DrawerInBay() &&
                        this.current != null &&
                        this.netWeight != null &&
                        this.netWeight > 0 &&
                        this.current > 0;
                    break;

                case WeightCalibartionStep.OptionalWeighing2:
                    this.IsEnabledForwards = !this.IsMachineMoving &&
                        this.SensorsService.IsLoadingUnitInBay &&
                        this.MachineService.Loadunits.DrawerInBay() &&
                        this.current != null &&
                        this.netWeight != null &&
                        this.netWeight > 0 &&
                        this.current > 0;
                    break;

                case WeightCalibartionStep.OptionalWeighing3:
                    this.IsEnabledForwards = !this.IsMachineMoving &&
                        this.SensorsService.IsLoadingUnitInBay &&
                        this.MachineService.Loadunits.DrawerInBay() &&
                        this.current != null &&
                        this.netWeight != null &&
                        this.netWeight > 0 &&
                        this.current > 0;
                    break;

                default:
                    this.IsEnabledForwards = true;
                    break;
            }

            this.RaisePropertyChanged(nameof(this.IsEnabledForwards));
            return this.IsEnabledForwards;
        }

        private bool CanLoadFromBay()
        {
            // Check a condition for external bay
            var conditionOnExternalBay = true;
            if (this.HasBayExternal && this.SensorsService.IsLoadingUnitInMiddleBottomBay)
            {
                conditionOnExternalBay = false;
            }

            var selectedBayPosition = this.Bay.Positions.Single(p => p.Height == this.Bay.Positions.Max(pos => pos.Height));
            return (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay) &&
                   this.MachineStatus.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Bay &&
                   this.MachineStatus.LogicalPositionId == this.Bay.Id &&
                   this.CanBaseExecute() &&
                   selectedBayPosition != null &&
                   selectedBayPosition.LoadingUnit != null &&
                   this.MachineStatus.EmbarkedLoadingUnit is null &&
                   conditionOnExternalBay;
        }

        private bool CanUnloadToBay()
        {
            var selectedBayPosition = this.Bay.Positions.Single(p => p.Height == this.Bay.Positions.Max(pos => pos.Height));
            var res = (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay) &&
                   this.CanBaseExecute() &&
                   this.MachineStatus.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Bay &&
                   this.MachineStatus.LogicalPositionId == this.Bay.Id &&
                   selectedBayPosition != null &&
                   selectedBayPosition.LoadingUnit == null &&
                   this.MachineStatus.EmbarkedLoadingUnit != null;

            return res;
        }

        private void GoForward()
        {
            WeightData data = new WeightData();
            switch (this.currentStep)
            {
                case WeightCalibartionStep.CallUnit:
                    this.CurrentStep = WeightCalibartionStep.EmptyUnitWeighing;
                    break;

                case WeightCalibartionStep.EmptyUnitWeighing:
                    data.Current = this.current;
                    data.NetWeight = this.netWeight;
                    this.unitsWeighing.Add(data);
                    this.Current = 0.0;
                    this.NetWeight = 0.0;
                    this.CurrentStep = WeightCalibartionStep.OptionalWeighing1;
                    break;

                case WeightCalibartionStep.FullUnitWeighing:
                    data.Current = this.current;
                    data.NetWeight = this.netWeight;
                    this.unitsWeighing.Add(data);
                    this.Current = 0.0;
                    this.NetWeight = 0.0;
                    this.CurrentStep = WeightCalibartionStep.SetWeight;
                    break;

                case WeightCalibartionStep.OptionalWeighing1:
                    data.Current = this.current;
                    data.NetWeight = this.netWeight;
                    this.unitsWeighing.Add(data);
                    this.Current = 0.0;
                    this.NetWeight = 0.0;
                    this.CurrentStep = WeightCalibartionStep.OptionalWeighing2;
                    break;

                case WeightCalibartionStep.OptionalWeighing2:
                    data.Current = this.current;
                    data.NetWeight = this.netWeight;
                    this.unitsWeighing.Add(data);
                    this.Current = 0.0;
                    this.NetWeight = 0.0;
                    this.CurrentStep = WeightCalibartionStep.OptionalWeighing3;
                    break;

                case WeightCalibartionStep.OptionalWeighing3:
                    data.Current = this.current;
                    data.NetWeight = this.netWeight;
                    this.unitsWeighing.Add(data);
                    this.Current = 0.0;
                    this.NetWeight = 0.0;
                    this.CurrentStep = WeightCalibartionStep.FullUnitWeighing;
                    break;
            }
        }

        private void GoRetryCommand()
        {
            this.unitsWeighing.Clear();
            this.CurrentStep = WeightCalibartionStep.EmptyUnitWeighing;
            this.Current = 0.0;
            this.NetWeight = 0.0;
        }

        private void InputLoadingUnitIdPropertyChanged()
        {
            if (this.LoadingUnits is null)
            {
                return;
            }

            // HACK: 1
            this.SelectedLoadingUnit = this.loadingUnitId == null
                ? null
                : this.LoadingUnits.SingleOrDefault(c => c.Id == this.loadingUnitId);
        }

        private async Task LoadFromBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var selectedBayPosition = this.Bay.Positions.Single(p => p.Height == this.Bay.Positions.Max(pos => pos.Height));
                if (selectedBayPosition.LoadingUnit != null)
                {
                    await this.machineLoadingUnitsWebService.StartMovingLoadingUnitToBayAsync(selectedBayPosition.LoadingUnit.Id, MAS.AutomationService.Contracts.LoadingUnitLocation.Elevator);
                }

                this.IsBusyLoadingFromBay = true;
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

        private void OnPositioningMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            try
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationStart:
                        break;

                    case MessageStatus.OperationError:
                        this.IsBusyLoadingFromBay = false;
                        this.IsBusyUnloadingToBay = false;
                        break;

                    case MessageStatus.OperationStop:
                    case MessageStatus.OperationEnd:
                        this.IsBusyLoadingFromBay = false;
                        this.IsBusyUnloadingToBay = false;
                        break;
                }

                if (message.Status == MessageStatus.OperationUpdateData)
                {
                    if (message.Data != null)
                    {
                        this.Current = message.Data.TorqueCurrentSample.Value;
                    }
                }
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                await this.machineElevatorWebService.SetMeasureConstAsync(this.measureConst0, this.measureConst1, this.measureConst2);

                this.ShowNotification(
                        VW.App.Resources.Localized.Get("InstallationApp.InformationSuccessfullyUpdated"),
                        Services.Models.NotificationSeverity.Success);

                this.NavigationService.GoBack();

                this.CurrentStep = WeightCalibartionStep.CallUnit;
            }
            catch (Exception ex)
            {
                this.ShowNotification(
                       ex.Message,
                       Services.Models.NotificationSeverity.Error);
            }
        }

        private async void SetMeasureConst()
        {
            var unitTare = this.selectedLoadingUnit.Tare;
            var elevatorWeight = await this.machineElevatorWebService.GetWeightAsync();

            var orderList = this.unitsWeighing.OrderBy(s => s.NetWeight).ToList();

            this.UnitsWeighing = orderList;

            this.RaisePropertyChanged(nameof(this.UnitsWeighing));

            LstSquQuadRegr solvr = new LstSquQuadRegr();

            foreach (var units in this.unitsWeighing)
            {
                solvr.AddPoints(units.Current, (units.NetWeight + unitTare + elevatorWeight));
            }

            this.MeasureConst0 = Math.Round(solvr.cTerm(), 4);

            this.MeasureConst1 = Math.Round(solvr.bTerm(), 4);

            this.MeasureConst2 = Math.Round(solvr.aTerm(), 4);
        }

        private void Skip()
        {
            WeightData data = new WeightData();
            switch (this.currentStep)
            {
                case WeightCalibartionStep.CallUnit:
                    this.CurrentStep = WeightCalibartionStep.EmptyUnitWeighing;
                    break;

                case WeightCalibartionStep.EmptyUnitWeighing:
                    this.Current = 0.0;
                    this.NetWeight = 0.0;
                    this.CurrentStep = WeightCalibartionStep.OptionalWeighing1;
                    break;

                case WeightCalibartionStep.FullUnitWeighing:
                    this.Current = 0.0;
                    this.NetWeight = 0.0;
                    this.CurrentStep = WeightCalibartionStep.SetWeight;
                    break;

                case WeightCalibartionStep.OptionalWeighing1:
                    this.Current = 0.0;
                    this.NetWeight = 0.0;
                    this.CurrentStep = WeightCalibartionStep.OptionalWeighing2;
                    break;

                case WeightCalibartionStep.OptionalWeighing2:
                    this.Current = 0.0;
                    this.NetWeight = 0.0;
                    this.CurrentStep = WeightCalibartionStep.OptionalWeighing3;
                    break;

                case WeightCalibartionStep.OptionalWeighing3:
                    this.Current = 0.0;
                    this.NetWeight = 0.0;
                    this.CurrentStep = WeightCalibartionStep.FullUnitWeighing;
                    break;
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
                this.IsBusyLoadingFromBay = false;
                this.IsBusyUnloadingToBay = false;
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

            this.themeChangedToken = this.themeChangedToken
               ?? this.EventAggregator
                   .GetEvent<ThemeChangedPubSubEvent>()
                   .Subscribe(
                       (m) =>
                       {
                           this.RaisePropertyChanged(nameof(this.HasStepCallUnit));
                           this.RaisePropertyChanged(nameof(this.HasStepEmptyUnitWeighing));
                           this.RaisePropertyChanged(nameof(this.HasStepFullUnitWeighing));
                           this.RaisePropertyChanged(nameof(this.HasStepOptionalWeighing1));
                           this.RaisePropertyChanged(nameof(this.HasStepOptionalWeighing2));
                           this.RaisePropertyChanged(nameof(this.HasStepOptionalWeighing3));
                           this.RaisePropertyChanged(nameof(this.HasStepSetWeight));
                       },
                       ThreadOption.UIThread,
                       false);

            this.positioningMessageReceivedToken = this.positioningMessageReceivedToken
              ??
              this.eventAggregator
                  .GetEvent<NotificationEventUI<PositioningMessageData>>()
                  .Subscribe(
                      this.OnPositioningMessageReceived,
                      ThreadOption.UIThread,
                      false);
        }

        private async Task UnloadToBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var selectedBayPosition = this.Bay.Positions.Single(p => p.Height == this.Bay.Positions.Max(pos => pos.Height));
                if (this.MachineStatus.EmbarkedLoadingUnit is null)
                {
                    await this.machineElevatorWebService.UnloadToBayAsync(selectedBayPosition.Id);
                }
                else
                {
                    await this.machineLoadingUnitsWebService.EjectLoadingUnitAsync(selectedBayPosition.Location, this.MachineStatus.EmbarkedLoadingUnit.Id);
                }

                this.IsBusyUnloadingToBay = true;
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

        private void UpdateStatusButtonFooter(bool force = false)
        {
            if (!this.IsVisible && !force)
            {
                return;
            }

            switch (this.CurrentStep)
            {
                case WeightCalibartionStep.CallUnit:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, !this.IsMoving && this.IsEnabledForwards);
                    break;

                case WeightCalibartionStep.EmptyUnitWeighing:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, !this.IsMoving && this.IsEnabledForwards);
                    break;

                case WeightCalibartionStep.OptionalWeighing1:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, !this.IsMoving && this.IsEnabledForwards);
                    break;

                case WeightCalibartionStep.OptionalWeighing2:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, !this.IsMoving);
                    break;

                case WeightCalibartionStep.OptionalWeighing3:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, !this.IsMoving);
                    break;

                case WeightCalibartionStep.FullUnitWeighing:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, !this.IsMoving && this.IsEnabledForwards);
                    break;

                case WeightCalibartionStep.SetWeight:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, false);
                    this.SetMeasureConst();
                    break;
            }

            this.ShowAbortStep(true, !this.IsMoving);

            this.RaisePropertyChanged(nameof(this.HasStepCallUnit));
            this.RaisePropertyChanged(nameof(this.HasStepEmptyUnitWeighing));
            this.RaisePropertyChanged(nameof(this.HasStepFullUnitWeighing));
            this.RaisePropertyChanged(nameof(this.HasStepOptionalWeighing1));
            this.RaisePropertyChanged(nameof(this.HasStepOptionalWeighing2));
            this.RaisePropertyChanged(nameof(this.HasStepOptionalWeighing3));
            this.RaisePropertyChanged(nameof(this.HasStepSetWeight));
        }

        #endregion
    }
}

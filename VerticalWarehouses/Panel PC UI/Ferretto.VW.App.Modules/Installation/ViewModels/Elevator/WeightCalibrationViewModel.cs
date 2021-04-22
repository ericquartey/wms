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

        FullUnitWeighing,

        SetWeight
    }

    public struct WeightData
    {
        #region Properties

        public double Current { get; set; }

        public double LUTare { get; set; }

        public double NetWeight { get; set; }

        public WeightCalibartionStep Step { get; set; }

        #endregion
    }

    [Warning(WarningsArea.Installation)]
    public class WeightCalibrationViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineShuttersWebService shuttersWebService;

        private DelegateCommand callLoadunitToBayCommand;

        private DelegateCommand cancelCommand;

        private DelegateCommand changeUnitCommand;

        private double current;

        private WeightCalibartionStep currentStep;

        private DelegateCommand forwardCommand;

        private bool isBusyCallDrawer;

        private bool isBusyLoadingFromBay;

        private bool isCalibrationOk;

        private bool isEnabledForwards;

        private bool isPositionDownSelected;

        private bool isPositionUpSelected;

        private DelegateCommand loadFromBayCommand;

        private int loadingUnitId;

        private double measureConst0;

        private double measureConst1;

        private double measureConst2;

        private SubscriptionToken moveLoadingUnitToken;

        private double netWeight;

        private SubscriptionToken positioningMessageReceivedToken;

        private DelegateCommand retryCommand;

        private DelegateCommand saveCommand;

        private DelegateCommand selectBayPositionDownCommand;

        private DelegateCommand selectBayPositionUpCommand;

        private LoadingUnit selectedLoadingUnit;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        private SubscriptionToken themeChangedToken;

        private List<WeightData> unitsWeighing = new List<WeightData>();

        #endregion

        #region Constructors

        public WeightCalibrationViewModel(
            IEventAggregator eventAggregator,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineShuttersWebService shuttersWebService)
          : base(PresentationMode.Installer)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.shuttersWebService = shuttersWebService ?? throw new ArgumentNullException(nameof(shuttersWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));

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

        public ICommand CancelCommand =>
           this.cancelCommand ?? (this.cancelCommand =
           new DelegateCommand(
               () =>
               {
                   this.IsBusyCallDrawer = false;
                   this.UpdateStatusButtonFooter();
               }));

        public ICommand ChangeUnitCommand =>
                    this.changeUnitCommand ?? (this.changeUnitCommand =
            new DelegateCommand(
                async () => await this.ChangeUnitAsync(),
                this.CanChangeUnit));

        public double Current
        {
            get => this.current;
            set => this.SetProperty(ref this.current, value, this.RaiseCanExecuteChanged);
        }

        public WeightCalibartionStep CurrentStep
        {
            get => this.currentStep;
            set
            {
                this.SetProperty(ref this.currentStep, value, () => this.UpdateStatusButtonFooter(false));

                this.UpdateNextData();
            }
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

        public bool HasStepOptionalWeighing2 => false;

        public bool HasStepOptionalWeighing3 => false;

        public bool HasStepSetWeight => this.currentStep is WeightCalibartionStep.SetWeight;

        public bool IsBusyCallDrawer
        {
            get => this.isBusyCallDrawer;
            private set => this.SetProperty(ref this.isBusyCallDrawer, value);
        }

        public bool IsBusyLoadingFromBay
        {
            get => this.isBusyLoadingFromBay;
            private set => this.SetProperty(ref this.isBusyLoadingFromBay, value);
        }

        public bool IsEnabledForwards
        {
            get => this.isEnabledForwards;
            set => this.SetProperty(ref this.isEnabledForwards, value, this.RaiseCanExecuteChanged);
        }

        public bool IsExternalDouble => this.MachineService.Bay.IsExternal && this.MachineService.Bay.IsDouble;

        public bool IsMoving => (this.MachineService?.MachineStatus?.IsMoving ?? true) || (this.MachineService?.MachineStatus?.IsMovingLoadingUnit ?? true);

        public bool IsPositionDownSelected
        {
            get => this.isPositionDownSelected;
            set
            {
                if (this.SetProperty(ref this.isPositionDownSelected, value))
                {
                    this.IsPositionUpSelected = !this.isPositionDownSelected;
                }
            }
        }

        public bool IsPositionUpSelected
        {
            get => this.isPositionUpSelected;
            set
            {
                if (this.SetProperty(ref this.isPositionUpSelected, value) && value)
                {
                    this.IsPositionDownSelected = !this.isPositionUpSelected;
                }
            }
        }

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
               this.CanSave));

        public ICommand SelectBayPositionDownCommand =>
                                                                                                                                                                                                                                                                    this.selectBayPositionDownCommand
            ??
            (this.selectBayPositionDownCommand = new DelegateCommand(
                this.SelectBayPositionDown,
                this.CanSelectBayPositionDown));

        public ICommand SelectBayPositionUpCommand =>
            this.selectBayPositionUpCommand
            ??
            (this.selectBayPositionUpCommand = new DelegateCommand(
                this.SelectBayPositionUp,
                this.CanSelectBayPositionUp));

        public LoadingUnit SelectedLoadingUnit
        {
            get => this.selectedLoadingUnit;
            set => this.SetProperty(ref this.selectedLoadingUnit, value, this.RaiseCanExecuteChanged);
        }

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

        private IEnumerable<LoadingUnit> LoadingUnits => this.MachineService.Loadunits;

        #endregion

        #region Methods

        public bool CanSelectBayPositionDown()
        {
            return !this.IsMoving &&
                   this.IsPositionUpSelected;
        }

        public bool CanSelectBayPositionUp()
        {
            return !this.IsMoving &&
                   !this.IsPositionUpSelected;
        }

        public override void Disappear()
        {
            base.Disappear();

            if (this.moveLoadingUnitToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<MoveLoadingUnitMessageData>>().Unsubscribe(this.moveLoadingUnitToken);
                this.moveLoadingUnitToken?.Dispose();
                this.moveLoadingUnitToken = null;
            }

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

        public override async Task OnAppearedAsync()
        {
            if (!this.IsPositionUpSelected && !this.IsPositionDownSelected && this.CurrentStep == WeightCalibartionStep.CallUnit)
            {
                this.IsPositionUpSelected = true;
            }

            this.isCalibrationOk = false;
            this.SubscribeToEvents();

            if ((this.MachineService.Bay.IsDouble && this.MachineStatus.LoadingUnitPositionUpInBay != null) ||
                (this.MachineService.Bay.IsDouble && this.MachineService.Bay.Carousel is null && this.MachineStatus.LoadingUnitPositionDownInBay != null) ||
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

            //if (this.CurrentStep == WeightCalibartionStep.CallUnit)
            //{
            //    this.unitsWeighing.Clear();
            //}

            this.UpdateStatusButtonFooter(true);

            await base.OnAppearedAsync();
        }

        public void SelectBayPositionDown()
        {
            this.IsPositionDownSelected = true;
            this.selectBayPositionDownCommand?.RaiseCanExecuteChanged();
            this.selectBayPositionUpCommand?.RaiseCanExecuteChanged();

            this.RaiseCanExecuteChanged();
        }

        public void SelectBayPositionUp()
        {
            this.IsPositionUpSelected = true;
            this.selectBayPositionDownCommand?.RaiseCanExecuteChanged();
            this.selectBayPositionUpCommand?.RaiseCanExecuteChanged();

            this.RaiseCanExecuteChanged();
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

                //case WeightCalibartionStep.OptionalWeighing1:
                //    if (e.Next)
                //    {
                //        this.CurrentStep = WeightCalibartionStep.OptionalWeighing2;
                //    }
                //    else
                //    {
                //        this.CurrentStep = WeightCalibartionStep.EmptyUnitWeighing;
                //    }

                //    break;

                case WeightCalibartionStep.OptionalWeighing1:
                    if (e.Next)
                    {
                        this.CurrentStep = WeightCalibartionStep.FullUnitWeighing;
                    }
                    else
                    {
                        this.CurrentStep = WeightCalibartionStep.EmptyUnitWeighing;
                    }

                    break;

                case WeightCalibartionStep.FullUnitWeighing:
                    if (e.Next)
                    {
                        this.CurrentStep = WeightCalibartionStep.SetWeight;
                    }
                    else
                    {
                        this.CurrentStep = WeightCalibartionStep.OptionalWeighing1;
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

            this.selectBayPositionDownCommand?.RaiseCanExecuteChanged();

            this.selectBayPositionUpCommand?.RaiseCanExecuteChanged();

            this.callLoadunitToBayCommand?.RaiseCanExecuteChanged();

            this.forwardCommand?.RaiseCanExecuteChanged();

            this.stopCommand?.RaiseCanExecuteChanged();

            this.loadFromBayCommand?.RaiseCanExecuteChanged();

            this.saveCommand?.RaiseCanExecuteChanged();

            this.changeUnitCommand?.RaiseCanExecuteChanged();

            this.cancelCommand?.RaiseCanExecuteChanged();

            this.retryCommand?.RaiseCanExecuteChanged();

            this.UpdateStatusButtonFooter();

            this.RaisePropertyChanged(nameof(this.IsExternalDouble));
        }

        private async Task CallLoadunitToBayCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.EjectLoadingUnitAsync(this.MachineService.GetBayPositionSourceByDestination(this.isPositionDownSelected), this.LoadingUnitId);
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
            if (this.isPositionDownSelected)
            {
                return this.CanBaseExecute() &&
                   !this.SensorsService.IsLoadingUnitInMiddleBottomBay &&
                   !this.MachineService.Loadunits.DrawerInBay() &&
                   this.SelectedLoadingUnit != null &&
                   this.SelectedLoadingUnit.CellId != null &&
                   this.SelectedLoadingUnit.Height > 0;
            }
            else
            {
                return this.CanBaseExecute() &&
                   !this.SensorsService.IsLoadingUnitInBay &&
                   !this.MachineService.Loadunits.DrawerInBay() &&
                   this.SelectedLoadingUnit != null &&
                   this.SelectedLoadingUnit.CellId != null &&
                   this.SelectedLoadingUnit.Height > 0;
            }
        }

        private bool CanChangeUnit()
        {
            if (this.isPositionDownSelected)
            {
                return !this.IsMoving &&
                this.SensorsService.IsLoadingUnitInMiddleBottomBay &&
                this.MachineStatus.LoadingUnitPositionDownInBay != null;
            }
            else
            {
                return !this.IsMoving &&
                this.SensorsService.IsLoadingUnitInBay &&
                this.MachineStatus.LoadingUnitPositionUpInBay != null;
            }
        }

        private bool CanGoForward()
        {
            switch (this.currentStep)
            {
                case WeightCalibartionStep.CallUnit:
                    if (this.isPositionDownSelected)
                    {
                        this.IsEnabledForwards = !this.IsMachineMoving &&
                            this.SensorsService.IsLoadingUnitInMiddleBottomBay &&
                            this.MachineService.Loadunits.DrawerInBay();
                    }
                    else
                    {
                        this.IsEnabledForwards = !this.IsMachineMoving &&
                            this.SensorsService.IsLoadingUnitInBay &&
                            this.MachineService.Loadunits.DrawerInBay();
                    }
                    break;

                case WeightCalibartionStep.EmptyUnitWeighing:
                    if (this.isPositionDownSelected)
                    {
                        this.IsEnabledForwards = !this.IsMachineMoving &&
                            this.SensorsService.IsLoadingUnitInMiddleBottomBay &&
                            this.MachineService.Loadunits.DrawerInBay() &&
                            this.current != null &&
                            this.netWeight != null &&
                            this.current > 0;
                    }
                    else
                    {
                        this.IsEnabledForwards = !this.IsMachineMoving &&
                            this.SensorsService.IsLoadingUnitInBay &&
                            this.MachineService.Loadunits.DrawerInBay() &&
                            this.current != null &&
                            this.netWeight != null &&
                            this.current > 0;
                    }
                    break;

                case WeightCalibartionStep.FullUnitWeighing:
                    if (this.isPositionDownSelected)
                    {
                        this.IsEnabledForwards = !this.IsMachineMoving &&
                            this.SensorsService.IsLoadingUnitInMiddleBottomBay &&
                            this.MachineService.Loadunits.DrawerInBay() &&
                            this.current != null &&
                            this.netWeight != null &&
                            this.netWeight > 0 &&
                            this.current > 0;
                    }
                    else
                    {
                        this.IsEnabledForwards = !this.IsMachineMoving &&
                            this.SensorsService.IsLoadingUnitInBay &&
                            this.MachineService.Loadunits.DrawerInBay() &&
                            this.current != null &&
                            this.netWeight != null &&
                            this.netWeight > 0 &&
                            this.current > 0;
                    }
                    break;

                case WeightCalibartionStep.OptionalWeighing1:
                    if (this.isPositionDownSelected)
                    {
                        this.IsEnabledForwards = !this.IsMachineMoving &&
                            this.SensorsService.IsLoadingUnitInMiddleBottomBay &&
                            this.MachineService.Loadunits.DrawerInBay() &&
                            this.current != null &&
                            this.netWeight != null &&
                            this.netWeight > 0 &&
                            this.current > 0;
                    }
                    else
                    {
                        this.IsEnabledForwards = !this.IsMachineMoving &&
                            this.SensorsService.IsLoadingUnitInBay &&
                            this.MachineService.Loadunits.DrawerInBay() &&
                            this.current != null &&
                            this.netWeight != null &&
                            this.netWeight > 0 &&
                            this.current > 0;
                    }
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

            // var selectedBayPosition = this.Bay.Positions.Single(p => p.Height == this.Bay.Positions.Max(pos => pos.Height));

            var selectedBayPosition = this.SelectedBayPosition();
            return (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay || !this.HasShutter) &&
                   this.MachineStatus.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Bay &&
                   this.MachineStatus.LogicalPositionId == this.Bay.Id &&
                   this.CanBaseExecute() &&
                   selectedBayPosition != null &&
                   selectedBayPosition.LoadingUnit != null &&
                   this.MachineStatus.EmbarkedLoadingUnit is null &&
                   conditionOnExternalBay;
        }

        private bool CanSave()
        {
            return this.CanBaseExecute() && this.isCalibrationOk;
        }

        private async Task ChangeUnitAsync()
        {
            try
            {
                if (this.isPositionDownSelected)
                {
                    await this.machineLoadingUnitsWebService.InsertLoadingUnitAsync(this.SelectedBayPosition().Location, null, this.MachineStatus.LoadingUnitPositionDownInBay.Id);
                }
                else
                {
                    await this.machineLoadingUnitsWebService.InsertLoadingUnitAsync(this.SelectedBayPosition().Location, null, this.MachineStatus.LoadingUnitPositionUpInBay.Id);
                }
                this.IsBusyCallDrawer = true;
                //this.CurrentStep = WeightCalibartionStep.CallUnit;

                this.UpdateStatusButtonFooter();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
        }

        private void GoForward()
        {
            WeightData data = new WeightData();
            switch (this.currentStep)
            {
                case WeightCalibartionStep.CallUnit:

                    this.UpdateNextData();

                    this.CurrentStep = WeightCalibartionStep.EmptyUnitWeighing;
                    break;

                case WeightCalibartionStep.EmptyUnitWeighing:
                    if (!this.unitsWeighing.Where(s => s.Step == WeightCalibartionStep.EmptyUnitWeighing).Any())
                    {
                        data.Current = this.current;
                        data.NetWeight = this.netWeight;
                        data.Step = WeightCalibartionStep.EmptyUnitWeighing;
                        if (this.IsPositionDownSelected)
                        {
                            data.LUTare = this.MachineStatus.LoadingUnitPositionDownInBay.Tare;
                        }
                        else
                        {
                            data.LUTare = this.MachineStatus.LoadingUnitPositionUpInBay.Tare;
                        }
                        this.unitsWeighing.Add(data);
                    }

                    this.UpdateNextData();

                    this.CurrentStep = WeightCalibartionStep.OptionalWeighing1;
                    break;

                case WeightCalibartionStep.FullUnitWeighing:
                    if (!this.unitsWeighing.Where(s => s.Step == WeightCalibartionStep.FullUnitWeighing).Any())
                    {
                        data.Current = this.current;
                        data.NetWeight = this.netWeight;
                        data.Step = WeightCalibartionStep.FullUnitWeighing;
                        if (this.IsPositionDownSelected)
                        {
                            data.LUTare = this.MachineStatus.LoadingUnitPositionDownInBay.Tare;
                        }
                        else
                        {
                            data.LUTare = this.MachineStatus.LoadingUnitPositionUpInBay.Tare;
                        }
                        this.unitsWeighing.Add(data);
                    }

                    this.CurrentStep = WeightCalibartionStep.SetWeight;
                    break;

                case WeightCalibartionStep.OptionalWeighing1:
                    if (!this.unitsWeighing.Where(s => s.Step == WeightCalibartionStep.OptionalWeighing1).Any())
                    {
                        data.Current = this.current;
                        data.NetWeight = this.netWeight;
                        data.Step = WeightCalibartionStep.OptionalWeighing1;
                        if (this.IsPositionDownSelected)
                        {
                            data.LUTare = this.MachineStatus.LoadingUnitPositionDownInBay.Tare;
                        }
                        else
                        {
                            data.LUTare = this.MachineStatus.LoadingUnitPositionUpInBay.Tare;
                        }
                        this.unitsWeighing.Add(data);
                    }

                    this.UpdateNextData();

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

                //var selectedBayPosition = this.Bay.Positions.Single(p => p.Height == this.Bay.Positions.Max(pos => pos.Height));

                var selectedBayPosition = this.SelectedBayPosition();
                if (selectedBayPosition.LoadingUnit != null)
                {
                    await this.machineLoadingUnitsWebService.StartScaleCalibrationAsync(selectedBayPosition.LoadingUnit.Id);
                    //await this.machineLoadingUnitsWebService.StartMovingLoadingUnitToBayAsync(selectedBayPosition.LoadingUnit.Id, MAS.AutomationService.Contracts.LoadingUnitLocation.Elevator);

                    this.IsBusyLoadingFromBay = true;
                }
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

        private void OnMoveLoadingUnitChanged(NotificationMessageUI<MoveLoadingUnitMessageData> message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationEnd:
                    if ((message.Data as MoveLoadingUnitMessageData).Destination == CommonUtils.Messages.Enumerations.LoadingUnitLocation.InternalBay1Up
                        ||
                        (message.Data as MoveLoadingUnitMessageData).Destination == CommonUtils.Messages.Enumerations.LoadingUnitLocation.CarouselBay1Up
                        ||
                        (message.Data as MoveLoadingUnitMessageData).Destination == CommonUtils.Messages.Enumerations.LoadingUnitLocation.CarouselBay2Up
                        ||
                        (message.Data as MoveLoadingUnitMessageData).Destination == CommonUtils.Messages.Enumerations.LoadingUnitLocation.CarouselBay3Up
                        ||
                        (message.Data as MoveLoadingUnitMessageData).Destination == CommonUtils.Messages.Enumerations.LoadingUnitLocation.ExternalBay1Up
                        ||
                        (message.Data as MoveLoadingUnitMessageData).Destination == CommonUtils.Messages.Enumerations.LoadingUnitLocation.ExternalBay2Up
                        ||
                        (message.Data as MoveLoadingUnitMessageData).Destination == CommonUtils.Messages.Enumerations.LoadingUnitLocation.ExternalBay3Up
                        ||
                        (message.Data as MoveLoadingUnitMessageData).Destination == CommonUtils.Messages.Enumerations.LoadingUnitLocation.InternalBay2Up
                        ||
                        (message.Data as MoveLoadingUnitMessageData).Destination == CommonUtils.Messages.Enumerations.LoadingUnitLocation.InternalBay3Up)
                    {
                        this.IsBusyCallDrawer = false;
                        break;
                    }

                    break;
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
                        break;

                    case MessageStatus.OperationStop:
                    case MessageStatus.OperationEnd:
                        this.IsBusyLoadingFromBay = false;
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

                this.unitsWeighing.Clear();
                this.Current = 0.0;
                this.NetWeight = 0.0;
            }
            catch (Exception ex)
            {
                this.ShowNotification(
                       ex.Message,
                       Services.Models.NotificationSeverity.Error);
            }
        }

        private BayPosition SelectedBayPosition()
        {
            if (this.IsPositionDownSelected)
            {
                return this.Bay.Positions.Single(p => p.Height == this.Bay.Positions.Min(pos => pos.Height));
            }
            else
            {
                return this.Bay.Positions.Single(p => p.Height == this.Bay.Positions.Max(pos => pos.Height));
            }
        }

        private async void SetMeasureConst()
        {
            this.isCalibrationOk = false;
            var elevatorWeight = await this.machineElevatorWebService.GetWeightAsync();

            var orderList = this.unitsWeighing.OrderBy(s => s.NetWeight).ToList();

            this.UnitsWeighing = orderList;

            this.RaisePropertyChanged(nameof(this.UnitsWeighing));

            var ParameterCalculator = new LstSquQuadRegr();

            foreach (var units in this.unitsWeighing)
            {
                if (orderList.Any(x => Math.Abs(x.Current - units.Current) < 0.1 && x.Step != units.Step))
                {
                    this.ShowNotification(
                           VW.App.Resources.Localized.Get("InstallationApp.InvalidCurrentValues"),
                           Services.Models.NotificationSeverity.Error);
                    return;
                }
                if (orderList.Any(x => Math.Abs(x.NetWeight - units.NetWeight) < 20 && x.Step != units.Step))
                {
                    this.ShowNotification(
                           VW.App.Resources.Localized.Get("InstallationApp.InvalidWeightValues"),
                           Services.Models.NotificationSeverity.Error);
                    return;
                }
                ParameterCalculator.AddPoints(units.Current, units.NetWeight + units.LUTare + elevatorWeight);
                this.Logger.Debug($"Current {units.Current} net weight {units.NetWeight} total weight {units.NetWeight + units.LUTare + elevatorWeight} ");
            }

            this.MeasureConst0 = Math.Round(ParameterCalculator.cTerm(), 4);

            this.MeasureConst1 = Math.Round(ParameterCalculator.bTerm(), 4);

            this.MeasureConst2 = Math.Round(ParameterCalculator.aTerm(), 4);

            this.Logger.Debug($"Weight parameters Const2 {this.MeasureConst2} Const1 {this.MeasureConst1} Const0 {this.MeasureConst0}");
            this.isCalibrationOk = true;
            this.saveCommand?.RaiseCanExecuteChanged();
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

            this.moveLoadingUnitToken = this.moveLoadingUnitToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<MoveLoadingUnitMessageData>>()
                    .Subscribe(
                        this.OnMoveLoadingUnitChanged,
                        ThreadOption.UIThread,
                        false);
        }

        private void UpdateNextData()
        {
            switch (this.currentStep)
            {
                case WeightCalibartionStep.EmptyUnitWeighing:

                    if (!this.unitsWeighing.Where(s => s.Step == WeightCalibartionStep.EmptyUnitWeighing).Any())
                    {
                        this.Current = 0.0;
                        this.NetWeight = 0.0;
                    }
                    else
                    {
                        this.Current = this.unitsWeighing.Where(s => s.Step == WeightCalibartionStep.EmptyUnitWeighing).Select(s => s.Current).FirstOrDefault();
                        this.NetWeight = this.unitsWeighing.Where(s => s.Step == WeightCalibartionStep.EmptyUnitWeighing).Select(s => s.NetWeight).FirstOrDefault();
                    }

                    break;

                case WeightCalibartionStep.FullUnitWeighing:

                    if (!this.unitsWeighing.Where(s => s.Step == WeightCalibartionStep.FullUnitWeighing).Any())
                    {
                        this.Current = 0.0;
                        this.NetWeight = 0.0;
                    }
                    else
                    {
                        this.Current = this.unitsWeighing.Where(s => s.Step == WeightCalibartionStep.FullUnitWeighing).Select(s => s.Current).FirstOrDefault();
                        this.NetWeight = this.unitsWeighing.Where(s => s.Step == WeightCalibartionStep.FullUnitWeighing).Select(s => s.NetWeight).FirstOrDefault();
                    }

                    break;

                case WeightCalibartionStep.OptionalWeighing1:

                    if (!this.unitsWeighing.Where(s => s.Step == WeightCalibartionStep.OptionalWeighing1).Any())
                    {
                        this.Current = 0.0;
                        this.NetWeight = 0.0;
                    }
                    else
                    {
                        this.Current = this.unitsWeighing.Where(s => s.Step == WeightCalibartionStep.OptionalWeighing1).Select(s => s.Current).FirstOrDefault();
                        this.NetWeight = this.unitsWeighing.Where(s => s.Step == WeightCalibartionStep.OptionalWeighing1).Select(s => s.NetWeight).FirstOrDefault();
                    }

                    break;
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
                    this.ShowNextStepSinglePage(true, !this.IsMoving && this.IsEnabledForwards && !this.IsBusyCallDrawer);
                    break;

                case WeightCalibartionStep.EmptyUnitWeighing:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving && !this.IsBusyCallDrawer);
                    this.ShowNextStepSinglePage(true, !this.IsMoving && this.IsEnabledForwards && !this.IsBusyCallDrawer);
                    break;

                case WeightCalibartionStep.OptionalWeighing1:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving && !this.IsBusyCallDrawer);
                    this.ShowNextStepSinglePage(true, !this.IsMoving && this.IsEnabledForwards && !this.IsBusyCallDrawer);
                    break;

                case WeightCalibartionStep.FullUnitWeighing:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving && !this.IsBusyCallDrawer);
                    this.ShowNextStepSinglePage(true, !this.IsMoving && this.IsEnabledForwards && !this.IsBusyCallDrawer);
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

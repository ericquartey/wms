using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    internal struct ErrorLoadunitMissingStepAutomaticMode
    {
    }

    internal struct ErrorLoadunitMissingStepLoadunitOnBay1
    {
    }

    internal struct ErrorLoadunitMissingStepLoadunitOnBay2
    {
    }

    internal struct ErrorLoadunitMissingStepLoadunitOnBay3
    {
    }

    internal struct ErrorLoadunitMissingStepLoadunitOnElevator
    {
    }

    internal struct ErrorLoadunitMissingStepManualMode
    {
    }

    internal struct ErrorLoadunitMissingStepStart
    {
    }

    internal sealed class ErrorLoadunitMissingViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineErrorsWebService machineErrorsWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineModeWebService machineModeWebService;

        private readonly ISessionService sessionService;

        private DelegateCommand automaticCommand;

        private string automaticStepText;

        private string bay1StepText;

        private bool bay1StepVisible;

        private string bay2StepText;

        private bool bay2StepVisible;

        private string bay3StepText;

        private bool bay3StepVisible;

        private bool canLuIdOnElevator;

        private string currentError;

        private object currentStep = default(ErrorLoadunitMissingStepStart);

        private bool elevatorStepVisible;

        private string elevatorText;

        private string errorTime;

        private bool isBay1PositionDownVisible;

        private bool isBay1PositionUpVisible;

        private bool isBay2PositionDownVisible;

        private bool isBay2PositionUpVisible;

        private bool isBay3PositionDownVisible;

        private bool isBay3PositionUpVisible;

        private SubscriptionToken loadunitsToken;

        private int? luIdOnBay1Down;

        private int? luIdOnBay1Up;

        private int? luIdOnBay2Down;

        private int? luIdOnBay2Up;

        private int? luIdOnBay3Down;

        private int? luIdOnBay3Up;

        private int? luIdOnElevator;

        private MachineError machineError;

        private DelegateCommand manualCommad;

        private string manualStepText;

        private DelegateCommand markAsResolvedCommand;

        private DelegateCommand moveLoadunitCommand;

        private DelegateCommand moveToNextCommand;

        private LoadingUnit selectedLoadingUnit;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        private SubscriptionToken themeChangedToken;

        #endregion

        #region Constructors

        public ErrorLoadunitMissingViewModel(
            ISessionService sessionService,
            IMachineModeWebService machineModeWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineBaysWebService machineBaysWebService,
            IMachineErrorsWebService machineErrorsWebService)
            : base(Services.PresentationMode.Menu | Services.PresentationMode.Installer | Services.PresentationMode.Operator)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
            this.machineModeWebService = machineModeWebService ?? throw new ArgumentNullException(nameof(machineModeWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));

            this.CurrentStep = default(ErrorLoadunitMissingStepStart);
        }

        #endregion

        #region Properties

        public ICommand AutomaticCommand =>
            this.automaticCommand
            ??
            (this.automaticCommand = new DelegateCommand(
                async () => await this.AutomaticCommandAsync(),
                this.CanAutomaticCommand));

        public string AutomaticStepText
        {
            get => this.automaticStepText;
            set => this.SetProperty(ref this.automaticStepText, value);
        }

        public string Bay1StepText
        {
            get => this.bay1StepText;
            set => this.SetProperty(ref this.bay1StepText, value);
        }

        public bool Bay1StepVisible
        {
            get => this.bay1StepVisible;
            set => this.SetProperty(ref this.bay1StepVisible, value);
        }

        public string Bay2StepText
        {
            get => this.bay2StepText;
            set => this.SetProperty(ref this.bay2StepText, value);
        }

        public bool Bay2StepVisible
        {
            get => this.bay2StepVisible;
            set => this.SetProperty(ref this.bay2StepVisible, value);
        }

        public string Bay3StepText
        {
            get => this.bay3StepText;
            set => this.SetProperty(ref this.bay3StepText, value);
        }

        public bool Bay3StepVisible
        {
            get => this.bay3StepVisible;
            set => this.SetProperty(ref this.bay3StepVisible, value);
        }

        public bool CanLuIdOnElevator
        {
            get => this.canLuIdOnElevator;
            private set => this.SetProperty(ref this.canLuIdOnElevator, value);
        }

        public object CurrentStep
        {
            get => this.currentStep;
            set => this.SetProperty(ref this.currentStep, value, this.UpdateStatusButtonFooter);
        }

        public string ElevatorStepText
        {
            get => this.elevatorText;
            set => this.SetProperty(ref this.elevatorText, value);
        }

        public bool ElevatorStepVisible
        {
            get => this.elevatorStepVisible;
            set => this.SetProperty(ref this.elevatorStepVisible, value);
        }

        public override EnableMask EnableMask => EnableMask.MachineManualMode | EnableMask.MachinePoweredOn;

        public string Error => string.Join(
            this[nameof(this.LuIdOnElevator)],
            this[nameof(this.LuIdOnBay1Down)],
            this[nameof(this.LuIdOnBay1Up)],
            this[nameof(this.LuIdOnBay2Down)],
            this[nameof(this.LuIdOnBay2Up)],
            this[nameof(this.LuIdOnBay3Down)],
            this[nameof(this.LuIdOnBay3Up)]);

        public string ErrorTime
        {
            get => this.errorTime;
            set => this.SetProperty(ref this.errorTime, value);
        }

        public bool HasBay1PositionDownVisible => this.Bay1Positions?.Any(a => a.LocationUpDown == LoadingUnitLocation.Down && !a.IsBlocked) ?? false;

        public bool HasBay1PositionUpVisible => this.Bay1Positions?.Any(a => a.LocationUpDown == LoadingUnitLocation.Up && !a.IsBlocked) ?? false;

        public bool HasBay2PositionDownVisible => this.Bay2Positions?.Any(a => a.LocationUpDown == LoadingUnitLocation.Down && !a.IsBlocked) ?? false;

        public bool HasBay2PositionUpVisible => this.Bay2Positions?.Any(a => a.LocationUpDown == LoadingUnitLocation.Up && !a.IsBlocked) ?? false;

        public bool HasBay3PositionDownVisible => this.Bay3Positions?.Any(a => a.LocationUpDown == LoadingUnitLocation.Down && !a.IsBlocked) ?? false;

        public bool HasBay3PositionUpVisible => this.Bay3Positions?.Any(a => a.LocationUpDown == LoadingUnitLocation.Up && !a.IsBlocked) ?? false;

        public bool HasStepAutomaticMode => this.currentStep is ErrorLoadunitMissingStepAutomaticMode;

        public bool HasStepLoadunitOnBay1 => this.currentStep is ErrorLoadunitMissingStepLoadunitOnBay1;

        public bool HasStepLoadunitOnBay2 => this.currentStep is ErrorLoadunitMissingStepLoadunitOnBay2;

        public bool HasStepLoadunitOnBay3 => this.currentStep is ErrorLoadunitMissingStepLoadunitOnBay3;

        public bool HasStepLoadunitOnElevator => this.currentStep is ErrorLoadunitMissingStepLoadunitOnElevator;

        public bool HasStepManualMode => this.currentStep is ErrorLoadunitMissingStepManualMode;

        public bool HasStepStart => this.currentStep is ErrorLoadunitMissingStepStart;

        public bool IsBay1PositionDownVisible
        {
            get => this.isBay1PositionDownVisible;
            set => this.SetProperty(ref this.isBay1PositionDownVisible, value);
        }

        public bool IsBay1PositionUpVisible
        {
            get => this.isBay1PositionUpVisible;
            set => this.SetProperty(ref this.isBay1PositionUpVisible, value);
        }

        public bool IsBay2PositionDownVisible
        {
            get => this.isBay2PositionDownVisible;
            set => this.SetProperty(ref this.isBay2PositionDownVisible, value);
        }

        public bool IsBay2PositionUpVisible
        {
            get => this.isBay2PositionUpVisible;
            set => this.SetProperty(ref this.isBay2PositionUpVisible, value);
        }

        public bool IsBay3PositionDownVisible
        {
            get => this.isBay3PositionDownVisible;
            set => this.SetProperty(ref this.isBay3PositionDownVisible, value);
        }

        public bool IsBay3PositionUpVisible
        {
            get => this.isBay3PositionUpVisible;
            set => this.SetProperty(ref this.isBay3PositionUpVisible, value);
        }

        public bool IsMoving
        {
            get => this.MachineService?.MachineStatus?.IsMoving ?? true;
        }

        public override bool KeepAlive => false;

        public int? LuIdOnBay1Down
        {
            get => this.luIdOnBay1Down;
            set => this.SetProperty(ref this.luIdOnBay1Down, value);
        }

        public int? LuIdOnBay1Up
        {
            get => this.luIdOnBay1Up;
            set => this.SetProperty(ref this.luIdOnBay1Up, value);
        }

        public int? LuIdOnBay2Down
        {
            get => this.luIdOnBay2Down;
            set => this.SetProperty(ref this.luIdOnBay2Down, value);
        }

        public int? LuIdOnBay2Up
        {
            get => this.luIdOnBay2Up;
            set => this.SetProperty(ref this.luIdOnBay2Up, value);
        }

        public int? LuIdOnBay3Down
        {
            get => this.luIdOnBay3Down;
            set => this.SetProperty(ref this.luIdOnBay3Down, value);
        }

        public int? LuIdOnBay3Up
        {
            get => this.luIdOnBay3Up;
            set => this.SetProperty(ref this.luIdOnBay3Up, value);
        }

        public int? LuIdOnElevator
        {
            get => this.luIdOnElevator;
            set => this.SetProperty(ref this.luIdOnElevator, value);
        }

        internal bool LUPresentInBay1 => this.IsBay1External && this.IsBay1Double ? (this.SensorsService.Sensors.LUPresentInBay1 || this.SensorsService.Sensors.LUPresentMiddleBottomBay1 || this.SensorsService.Sensors.RobotOptionBay1 || this.SensorsService.Sensors.TrolleyOptionBay1) : this.IsBay1External ? (this.SensorsService.Sensors.LUPresentInBay1 || this.SensorsService.Sensors.LUPresentMiddleBottomBay1) : this.SensorsService.Sensors.LUPresentInBay1;

        internal bool LUPresentInBay2 => this.IsBay2External && this.IsBay2Double ? (this.SensorsService.Sensors.LUPresentInBay2 || this.SensorsService.Sensors.LUPresentMiddleBottomBay2 || this.SensorsService.Sensors.RobotOptionBay2 || this.SensorsService.Sensors.TrolleyOptionBay2) : this.IsBay2External ? (this.SensorsService.Sensors.LUPresentInBay2 || this.SensorsService.Sensors.LUPresentMiddleBottomBay2) : this.SensorsService.Sensors.LUPresentInBay2;

        internal bool LUPresentInBay3 => this.IsBay3External && this.IsBay3Double ? (this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.LUPresentMiddleBottomBay3 || this.SensorsService.Sensors.RobotOptionBay3 || this.SensorsService.Sensors.TrolleyOptionBay3) : this.IsBay3External ? (this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.LUPresentMiddleBottomBay3) : this.SensorsService.Sensors.LUPresentInBay3;

        public MachineError MachineError
        {
            get => this.machineError;
            set => this.SetProperty(ref this.machineError, value, () => this.OnErrorChanged(null));
        }

        public ICommand ManualCommand =>
            this.manualCommad
            ??
            (this.manualCommad = new DelegateCommand(
                async () => await this.ManualCommandAsync(),
                this.CanManualCommand));

        public string ManualStepText
        {
            get => this.manualStepText;
            set => this.SetProperty(ref this.manualStepText, value);
        }

        public ICommand MarkAsResolvedCommand =>
            this.markAsResolvedCommand
            ??
            (this.markAsResolvedCommand = new DelegateCommand(
                async () => await this.MarkAsResolvedAsync(),
                this.CanMarkAsResolved));

        public ICommand MoveLoadunitCommand =>
            this.moveLoadunitCommand
            ??
            (this.moveLoadunitCommand = new DelegateCommand(
                async () => await this.MoveLoadunitAsync(),
                this.CanMoveLoadunit));

        public ICommand MoveToNextCommand =>
            this.moveToNextCommand
            ??
            (this.moveToNextCommand = new DelegateCommand(
                () => this.Next(),
                () => this.CanBaseExecute() &&
                      this.MachineService.MachineMode != MachineMode.Automatic &&
                      this.MachineService.MachinePower == MachinePowerState.Powered));

        public LoadingUnit SelectedLoadingUnit
        {
            get => this.selectedLoadingUnit;
            private set => this.SetProperty(ref this.selectedLoadingUnit, value);
        }

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        internal IEnumerable<BayPosition> Bay1Positions => this.MachineService.Bays?.FirstOrDefault(a => a.Number == BayNumber.BayOne)?.Positions;

        internal IEnumerable<BayPosition> Bay2Positions => this.MachineService.Bays?.FirstOrDefault(a => a.Number == BayNumber.BayTwo)?.Positions;

        internal IEnumerable<BayPosition> Bay3Positions => this.MachineService.Bays?.FirstOrDefault(a => a.Number == BayNumber.BayThree)?.Positions;

        internal bool IsBay1Double => this.MachineService.Bays?.FirstOrDefault(a => a.Number == BayNumber.BayOne)?.IsDouble ?? false;

        internal bool IsBay1External => this.MachineService.Bays?.FirstOrDefault(a => a.Number == BayNumber.BayOne)?.IsExternal ?? false;

        internal bool IsBay2Double => this.MachineService.Bays?.FirstOrDefault(a => a.Number == BayNumber.BayTwo)?.IsDouble ?? false;

        internal bool IsBay2External => this.MachineService.Bays?.FirstOrDefault(a => a.Number == BayNumber.BayTwo)?.IsExternal ?? false;

        internal bool IsBay3Double => this.MachineService.Bays?.FirstOrDefault(a => a.Number == BayNumber.BayThree)?.IsDouble ?? false;

        internal bool IsBay3External => this.MachineService.Bays?.FirstOrDefault(a => a.Number == BayNumber.BayThree)?.IsExternal ?? false;

        internal bool LUPresentInBay1Down => this.IsBay1External && this.IsBay1Double ? (this.SensorsService.Sensors.LUPresentMiddleBottomBay1 || this.SensorsService.Sensors.RobotOptionBay1) : this.IsBay1External ? (this.SensorsService.Sensors.LUPresentInBay1 || this.SensorsService.Sensors.LUPresentMiddleBottomBay1) : this.SensorsService.Sensors.LUPresentMiddleBottomBay1;

        internal bool LUPresentInBay1Up => this.IsBay1External && this.IsBay1Double ? (this.SensorsService.Sensors.LUPresentInBay1 || this.SensorsService.Sensors.TrolleyOptionBay1) : this.IsBay1External ? (this.SensorsService.Sensors.LUPresentInBay1 || this.SensorsService.Sensors.LUPresentMiddleBottomBay1) : this.SensorsService.Sensors.LUPresentInBay1;

        internal bool LUPresentInBay2Down => this.IsBay2External && this.IsBay2Double ? (this.SensorsService.Sensors.LUPresentMiddleBottomBay2 || this.SensorsService.Sensors.RobotOptionBay2) : this.IsBay2External ? (this.SensorsService.Sensors.LUPresentInBay2 || this.SensorsService.Sensors.LUPresentMiddleBottomBay2) : this.SensorsService.Sensors.LUPresentMiddleBottomBay2;

        internal bool LUPresentInBay2Up => this.IsBay2External && this.IsBay2Double ? (this.SensorsService.Sensors.LUPresentInBay2 || this.SensorsService.Sensors.TrolleyOptionBay2) : this.IsBay2External ? (this.SensorsService.Sensors.LUPresentInBay2 || this.SensorsService.Sensors.LUPresentMiddleBottomBay2) : this.SensorsService.Sensors.LUPresentInBay2;

        internal bool LUPresentInBay3Down => this.IsBay3External && this.IsBay3Double ? (this.SensorsService.Sensors.LUPresentMiddleBottomBay3 || this.SensorsService.Sensors.RobotOptionBay3) : this.IsBay3External ? (this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.LUPresentMiddleBottomBay3) : this.SensorsService.Sensors.LUPresentMiddleBottomBay3;

        internal bool LUPresentInBay3Up => this.IsBay3External && this.IsBay3Double ? (this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.TrolleyOptionBay3) : this.IsBay3External ? (this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.LUPresentMiddleBottomBay3) : this.SensorsService.Sensors.LUPresentInBay3;

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
                    case nameof(this.LuIdOnElevator):
                        if (this.SensorsService.IsLoadingUnitOnElevator &&
                            ((!this.LuIdOnElevator.HasValue) || (!this.MachineService.Loadunits.Any(l => l.Id == this.LuIdOnElevator && l.Status == LoadingUnitStatus.Undefined && l.Height != 0))))
                        {
                            var lus = string.Join(",", this.MachineService.Loadunits.Where(l => l.Status == LoadingUnitStatus.Undefined && l.Height != 0).Select(s => s.Id.ToString()));
                            return Localized.Get("ErrorsApp.InvalidUnit") + lus;
                        }

                        break;

                    case nameof(this.LuIdOnBay1Down):
                        if (this.LUPresentInBay1 &&
                            this.HasBay1PositionUpVisible &&
                            ((!this.LuIdOnBay1Down.HasValue) || (!this.MachineService.Loadunits.Any(l => l.Id == this.LuIdOnBay1Down && l.Status == LoadingUnitStatus.Undefined && l.Height != 0))))
                        {
                            var lus = string.Join(",", this.MachineService.Loadunits.Where(l => l.Status == LoadingUnitStatus.Undefined && l.Height != 0).Select(s => s.Id.ToString()));
                            return Localized.Get("ErrorsApp.InvalidUnit") + lus;
                        }

                        break;

                    case nameof(this.LuIdOnBay1Up):
                        if (this.SensorsService.Sensors.LUPresentMiddleBottomBay1 &&
                            this.HasBay1PositionDownVisible &&
                            ((!this.LuIdOnBay1Up.HasValue) || (!this.MachineService.Loadunits.Any(l => l.Id == this.LuIdOnBay1Up && l.Status == LoadingUnitStatus.Undefined && l.Height == 0))))
                        {
                            var lus = string.Join(",", this.MachineService.Loadunits.Where(l => l.Status == LoadingUnitStatus.Undefined && l.Height == 0).Select(s => s.Id.ToString()));
                            return Localized.Get("ErrorsApp.InvalidUnit") + lus;
                        }

                        break;

                    case nameof(this.LuIdOnBay2Down):
                        if (this.LUPresentInBay2 &&
                            this.HasBay2PositionUpVisible &&
                            ((!this.LuIdOnBay2Down.HasValue) || (!this.MachineService.Loadunits.Any(l => l.Id == this.LuIdOnBay2Down && l.Status == LoadingUnitStatus.Undefined && l.Height != 0))))
                        {
                            var lus = string.Join(",", this.MachineService.Loadunits.Where(l => l.Status == LoadingUnitStatus.Undefined && l.Height != 0).Select(s => s.Id.ToString()));
                            return Localized.Get("ErrorsApp.InvalidUnit") + lus;
                        }

                        break;

                    case nameof(this.LuIdOnBay2Up):
                        if (this.SensorsService.Sensors.LUPresentMiddleBottomBay2 &&
                            this.HasBay2PositionDownVisible &&
                            ((!this.LuIdOnBay2Up.HasValue) || (!this.MachineService.Loadunits.Any(l => l.Id == this.LuIdOnBay2Up && l.Status == LoadingUnitStatus.Undefined && l.Height == 0))))
                        {
                            var lus = string.Join(",", this.MachineService.Loadunits.Where(l => l.Status == LoadingUnitStatus.Undefined && l.Height == 0).Select(s => s.Id.ToString()));
                            return Localized.Get("ErrorsApp.InvalidUnit") + lus;
                        }

                        break;

                    case nameof(this.LuIdOnBay3Down):
                        if (this.LUPresentInBay3 &&
                            this.HasBay3PositionUpVisible &&
                            ((!this.LuIdOnBay3Down.HasValue) || (!this.MachineService.Loadunits.Any(l => l.Id == this.LuIdOnBay3Down && l.Status == LoadingUnitStatus.Undefined && l.Height != 0))))
                        {
                            var lus = string.Join(",", this.MachineService.Loadunits.Where(l => l.Status == LoadingUnitStatus.Undefined && l.Height != 0).Select(s => s.Id.ToString()));
                            return Localized.Get("ErrorsApp.InvalidUnit") + lus;
                        }

                        break;

                    case nameof(this.LuIdOnBay3Up):
                        if (this.SensorsService.Sensors.LUPresentMiddleBottomBay3 &&
                            this.HasBay3PositionDownVisible &&
                            ((!this.LuIdOnBay3Up.HasValue) || (!this.MachineService.Loadunits.Any(l => l.Id == this.LuIdOnBay3Up && l.Status == LoadingUnitStatus.Undefined && l.Height != 0))))
                        {
                            var lus = string.Join(",", this.MachineService.Loadunits.Where(l => l.Status == LoadingUnitStatus.Undefined && l.Height == 0).Select(s => s.Id.ToString()));
                            return Localized.Get("ErrorsApp.InvalidUnit") + lus;
                        }

                        break;
                }

                if (this.IsVisible && string.IsNullOrEmpty(this.currentError))
                {
                    // this.ClearNotifications();
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.MachineError = null;

            if (this.stepChangedToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.stepChangedToken);
                this.stepChangedToken?.Dispose();
                this.stepChangedToken = null;
            }

            if (this.loadunitsToken != null)
            {
                this.EventAggregator.GetEvent<LoadUnitsChangedPubSubEvent>().Unsubscribe(this.loadunitsToken);
                this.loadunitsToken.Dispose();
                this.loadunitsToken = null;
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

            this.UpdateStatusButtonFooter();

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                this.MachineError = await this.machineErrorsWebService.GetCurrentAsync();

                if (this.MachineError is null)
                {
                    await this.MarkAsResolvedAsync();
                }

                var stepValue = 2;

                this.ElevatorStepVisible = false;
                this.Bay1StepVisible = false;
                this.Bay2StepVisible = false;
                this.Bay3StepVisible = false;
                this.IsBay1PositionUpVisible = false;
                this.IsBay1PositionDownVisible = false;
                this.IsBay2PositionUpVisible = false;
                this.IsBay2PositionDownVisible = false;
                this.IsBay3PositionUpVisible = false;
                this.IsBay3PositionDownVisible = false;

                // Elevator
                this.LuIdOnElevator = null;
                if (this.SensorsService.IsLoadingUnitOnElevator)
                {
                    this.LuIdOnElevator = this.MachineService.Loadunits?.FirstOrDefault(l => l.Status == LoadingUnitStatus.Undefined && l.Height != 0)?.Id;
                }

                if (this.LuIdOnElevator != null)
                {
                    this.ElevatorStepText = stepValue.ToString();
                    stepValue++;
                    this.ElevatorStepVisible = true;
                }

                // Bay 1

                this.LuIdOnBay1Up = null;
                this.LuIdOnBay1Down = null;
                if (this.IsBay1Double && this.IsBay1External)
                {
                    if (this.LUPresentInBay1Up &&
                        this.HasBay1PositionUpVisible)
                    {
                        this.LuIdOnBay1Up = this.MachineService.Loadunits?.FirstOrDefault(l => l.Status == LoadingUnitStatus.Undefined && l.Height == 0)?.Id;
                        this.IsBay1PositionUpVisible = true;
                    }

                    if (this.LUPresentInBay1Down &&
                        this.HasBay1PositionDownVisible)
                    {
                        this.LuIdOnBay1Down = this.MachineService.Loadunits?.FirstOrDefault(l => l.Status == LoadingUnitStatus.Undefined && l.Height != 0)?.Id;
                        this.IsBay1PositionDownVisible = true;
                    }
                }
                else
                {
                    if (this.LUPresentInBay1 &&
                        this.HasBay1PositionUpVisible)
                    {
                        this.LuIdOnBay1Up = this.MachineService.Loadunits?.FirstOrDefault(l => l.Status == LoadingUnitStatus.Undefined && l.Height == 0)?.Id;
                        this.IsBay1PositionUpVisible = true;
                    }

                    if (this.SensorsService.Sensors.LUPresentMiddleBottomBay1 &&
                        this.HasBay1PositionDownVisible)
                    {
                        this.LuIdOnBay1Down = this.MachineService.Loadunits?.FirstOrDefault(l => l.Status == LoadingUnitStatus.Undefined && l.Height != 0)?.Id;
                        this.IsBay1PositionDownVisible = true;
                    }
                }

                if (this.IsBay1PositionUpVisible || this.IsBay1PositionDownVisible)
                {
                    this.Bay1StepText = stepValue.ToString();
                    stepValue++;
                    this.Bay1StepVisible = true;
                }

                // Bay 2
                this.LuIdOnBay2Up = null;

                if (this.IsBay2Double && this.IsBay2External)
                {
                    if (this.LUPresentInBay2Up &&
                    this.HasBay2PositionUpVisible)
                    {
                        this.LuIdOnBay2Up = this.MachineService.Loadunits?.FirstOrDefault(l => l.Status == LoadingUnitStatus.Undefined && l.Height == 0)?.Id;
                        this.IsBay2PositionUpVisible = true;
                    }

                    if (this.LUPresentInBay1Down &&
                        this.HasBay2PositionDownVisible)
                    {
                        this.LuIdOnBay2Down = this.MachineService.Loadunits?.FirstOrDefault(l => l.Status == LoadingUnitStatus.Undefined && l.Height != 0)?.Id;
                        this.IsBay2PositionDownVisible = true;
                    }
                }
                else
                {
                    if (this.LUPresentInBay2 &&
                    this.HasBay2PositionUpVisible)
                    {
                        this.LuIdOnBay2Up = this.MachineService.Loadunits?.FirstOrDefault(l => l.Status == LoadingUnitStatus.Undefined && l.Height == 0)?.Id;
                        this.IsBay2PositionUpVisible = true;
                    }

                    if (this.SensorsService.Sensors.LUPresentMiddleBottomBay2 &&
                        this.HasBay2PositionDownVisible)
                    {
                        this.LuIdOnBay2Down = this.MachineService.Loadunits?.FirstOrDefault(l => l.Status == LoadingUnitStatus.Undefined && l.Height != 0)?.Id;
                        this.IsBay2PositionDownVisible = true;
                    }
                }

                if (this.IsBay2PositionUpVisible || this.IsBay2PositionDownVisible)
                {
                    this.Bay2StepText = stepValue.ToString();
                    stepValue++;
                    this.Bay2StepVisible = true;
                }

                // Bay 3
                this.LuIdOnBay3Up = null;

                if (this.IsBay3Double && this.IsBay3External)
                {
                    if (this.LUPresentInBay3Up &&
                    this.HasBay3PositionUpVisible)
                    {
                        this.LuIdOnBay3Up = this.MachineService.Loadunits?.FirstOrDefault(l => l.Status == LoadingUnitStatus.Undefined && l.Height == 0)?.Id;
                        this.IsBay3PositionUpVisible = true;
                    }

                    if (this.LUPresentInBay3Down &&
                        this.HasBay3PositionDownVisible)
                    {
                        this.LuIdOnBay3Down = this.MachineService.Loadunits?.FirstOrDefault(l => l.Status == LoadingUnitStatus.Undefined && l.Height != 0)?.Id;
                        this.IsBay3PositionDownVisible = true;
                    }
                }
                else
                {
                }

                if (this.IsBay3PositionUpVisible || this.IsBay3PositionDownVisible)
                {
                    this.Bay3StepText = stepValue.ToString();
                    stepValue++;
                    this.Bay3StepVisible = true;
                }

                this.AutomaticStepText = stepValue.ToString();
                this.ManualStepText = stepValue.ToString();
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
            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.CanLuIdOnElevator = this.CanBaseExecute();

            this.markAsResolvedCommand?.RaiseCanExecuteChanged();
            this.moveLoadunitCommand?.RaiseCanExecuteChanged();

            this.automaticCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();
            this.moveToNextCommand?.RaiseCanExecuteChanged();
        }

        private async Task AutomaticCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                // Elevator
                if (this.SensorsService.IsLoadingUnitOnElevator &&
                    this.LuIdOnElevator.HasValue)
                {
                    await this.machineElevatorWebService.SetLoadUnitOnElevatorAsync(this.LuIdOnElevator.Value);
                }

                // Bay 1

                if (this.IsBay1Double && this.IsBay1External)
                {
                    if (this.LUPresentInBay1Up &&
                    this.HasBay1PositionUpVisible &&
                    this.LuIdOnBay1Up.HasValue)
                    {
                        var id = this.Bay1Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Up).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay1Up.Value);
                    }

                    if (this.LUPresentInBay1Down &&
                        this.HasBay1PositionDownVisible &&
                        this.LuIdOnBay1Down.HasValue)
                    {
                        var id = this.Bay1Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Down).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay1Down.Value);
                    }
                }
                else
                {
                    if (this.LUPresentInBay1 &&
                    this.HasBay1PositionUpVisible &&
                    this.LuIdOnBay1Up.HasValue)
                    {
                        var id = this.Bay1Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Up).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay1Up.Value);
                    }

                    if (this.SensorsService.Sensors.LUPresentMiddleBottomBay1 &&
                        this.HasBay1PositionDownVisible &&
                        this.LuIdOnBay1Down.HasValue)
                    {
                        var id = this.Bay1Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Down).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay1Down.Value);
                    }
                }

                // Bay 2

                if (this.IsBay2Double && this.IsBay2External)
                {
                    if (this.LUPresentInBay2Up &&
                    this.HasBay2PositionUpVisible &&
                    this.LuIdOnBay2Up.HasValue)
                    {
                        var id = this.Bay2Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Up).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay2Up.Value);
                    }

                    if (this.LUPresentInBay2Down &&
                        this.HasBay2PositionDownVisible &&
                        this.LuIdOnBay2Down.HasValue)
                    {
                        var id = this.Bay2Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Down).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay2Down.Value);
                    }
                }
                else
                {
                    if (this.LUPresentInBay2 &&
                    this.HasBay2PositionUpVisible &&
                    this.LuIdOnBay2Up.HasValue)
                    {
                        var id = this.Bay2Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Up).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay2Up.Value);
                    }

                    if (this.SensorsService.Sensors.LUPresentMiddleBottomBay2 &&
                        this.HasBay2PositionDownVisible &&
                        this.LuIdOnBay2Down.HasValue)
                    {
                        var id = this.Bay2Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Down).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay2Down.Value);
                    }
                }

                // Bay 3

                if (this.IsBay3Double && this.IsBay3External)
                {
                    if (this.LUPresentInBay3Up &&
                    this.HasBay3PositionUpVisible &&
                    this.LuIdOnBay3Up.HasValue)
                    {
                        var id = this.Bay3Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Up).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay3Up.Value);
                    }

                    if (this.LUPresentInBay3Down &&
                        this.HasBay3PositionDownVisible &&
                        this.LuIdOnBay3Down.HasValue)
                    {
                        var id = this.Bay3Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Down).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay3Down.Value);
                    }
                }
                else
                {
                    if (this.LUPresentInBay3 &&
                    this.HasBay3PositionUpVisible &&
                    this.LuIdOnBay3Up.HasValue)
                    {
                        var id = this.Bay3Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Up).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay3Up.Value);
                    }

                    if (this.SensorsService.Sensors.LUPresentMiddleBottomBay3 &&
                        this.HasBay3PositionDownVisible &&
                        this.LuIdOnBay3Down.HasValue)
                    {
                        var id = this.Bay3Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Down).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay3Down.Value);
                    }
                }

                await this.machineErrorsWebService.ResolveAllAsync();

                await this.machineModeWebService.SetAutomaticAsync();
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

        private bool CanAutomaticCommand()
        {
            return !this.IsKeyboardOpened &&
                   !this.IsMoving &&
                   this.MachineService.MachineMode != MachineMode.Automatic;
        }

        private bool CanBaseExecute()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsMoving;
        }

        private bool CanManualCommand()
        {
            return !this.IsKeyboardOpened &&
                   !this.IsMoving &&
                   this.MachineService.MachineMode != MachineMode.Automatic;
        }

        private bool CanMarkAsResolved()
        {
            return
                this.MachineError != null &&
                (this.MachineError.Code == (int)MachineErrorCode.LoadUnitMissingOnElevator ||
                 (this.MachineError.Code == (int)MachineErrorCode.LoadUnitMissingOnBay && this.MachineError.BayNumber == this.MachineService.BayNumber)) &&
                !this.IsWaitingForResponse;
        }

        private bool CanMoveLoadunit()
        {
            return
                this.CanMarkAsResolved();
        }

        private bool CanStop()
        {
            return
                this.IsMoving
                &&
                !this.IsWaitingForResponse;
        }

        private async Task ManualCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                // Elevator
                if (this.SensorsService.IsLoadingUnitOnElevator &&
                    this.LuIdOnElevator.HasValue)
                {
                    await this.machineElevatorWebService.SetLoadUnitOnElevatorAsync(this.LuIdOnElevator.Value);
                }

                // Bay 1

                if (this.IsBay1Double && this.IsBay1External)
                {
                    if (this.LUPresentInBay1Up &&
                this.HasBay1PositionUpVisible &&
                this.LuIdOnBay1Up.HasValue)
                    {
                        var id = this.Bay1Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Up).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay1Up.Value);
                    }

                    if (this.LUPresentInBay1Down &&
                        this.HasBay1PositionDownVisible &&
                        this.LuIdOnBay1Down.HasValue)
                    {
                        var id = this.Bay1Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Down).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay1Down.Value);
                    }
                }
                else
                {
                    if (this.LUPresentInBay1 &&
                this.HasBay1PositionUpVisible &&
                this.LuIdOnBay1Up.HasValue)
                    {
                        var id = this.Bay1Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Up).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay1Up.Value);
                    }

                    if (this.SensorsService.Sensors.LUPresentMiddleBottomBay1 &&
                        this.HasBay1PositionDownVisible &&
                        this.LuIdOnBay1Down.HasValue)
                    {
                        var id = this.Bay1Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Down).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay1Down.Value);
                    }
                }

                // Bay 2

                if (this.IsBay2Double && this.IsBay2External)
                {
                    if (this.LUPresentInBay2Up &&
                       this.HasBay2PositionUpVisible &&
                       this.LuIdOnBay2Up.HasValue)
                    {
                        var id = this.Bay2Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Up).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay2Up.Value);
                    }

                    if (this.LUPresentInBay2Down &&
                        this.HasBay2PositionDownVisible &&
                        this.LuIdOnBay2Down.HasValue)
                    {
                        var id = this.Bay2Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Down).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay2Down.Value);
                    }
                }
                else
                {
                    if (this.LUPresentInBay2 &&
                    this.HasBay2PositionUpVisible &&
                    this.LuIdOnBay2Up.HasValue)
                    {
                        var id = this.Bay2Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Up).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay2Up.Value);
                    }

                    if (this.SensorsService.Sensors.LUPresentMiddleBottomBay2 &&
                        this.HasBay2PositionDownVisible &&
                        this.LuIdOnBay2Down.HasValue)
                    {
                        var id = this.Bay2Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Down).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay2Down.Value);
                    }
                }

                // Bay 3

                if (this.IsBay3Double && this.IsBay3External)
                {
                    if (this.LUPresentInBay3Up &&
                    this.HasBay3PositionUpVisible &&
                    this.LuIdOnBay3Up.HasValue)
                    {
                        var id = this.Bay3Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Up).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay3Up.Value);
                    }

                    if (this.LUPresentInBay3Down &&
                        this.HasBay3PositionDownVisible &&
                        this.LuIdOnBay3Down.HasValue)
                    {
                        var id = this.Bay3Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Down).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay3Down.Value);
                    }
                }
                else
                {
                    if (this.LUPresentInBay3 &&
                    this.HasBay3PositionUpVisible &&
                    this.LuIdOnBay3Up.HasValue)
                    {
                        var id = this.Bay3Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Up).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay3Up.Value);
                    }

                    if (this.SensorsService.Sensors.LUPresentMiddleBottomBay3 &&
                        this.HasBay3PositionDownVisible &&
                        this.LuIdOnBay3Down.HasValue)
                    {
                        var id = this.Bay3Positions.Single(s => s.LocationUpDown == LoadingUnitLocation.Down).Id;
                        await this.machineBaysWebService.SetLoadUnitOnBayAsync(id, this.LuIdOnBay3Down.Value);
                    }
                }

                await this.machineErrorsWebService.ResolveAllAsync();

                await this.machineModeWebService.SetManualAsync();
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

        private async Task MoveLoadunitAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                if (this.SelectedLoadingUnit.Cell is null)
                {
                    await this.machineLoadingUnitsWebService.InsertLoadingUnitAsync(
                        LoadingUnitLocation.Elevator,
                        null,
                        this.SelectedLoadingUnit.Id);
                }
                else
                {
                    await this.machineLoadingUnitsWebService.InsertLoadingUnitAsync(
                        LoadingUnitLocation.Elevator,
                        this.SelectedLoadingUnit.Cell.Id,
                        this.SelectedLoadingUnit.Id);
                }
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

        private void Next()
        {
            if (this.CurrentStep is ErrorLoadunitMissingStepStart)
            {
                if (this.SensorsService.IsLoadingUnitOnElevator)
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnElevator);
                }
                else if ((this.SensorsService.Sensors.LUPresentInBay1 || this.SensorsService.Sensors.LUPresentMiddleBottomBay1 ||
                          this.SensorsService.Sensors.RobotOptionBay1 || this.SensorsService.Sensors.TrolleyOptionBay1) &&
                         (this.HasBay1PositionUpVisible || this.HasBay1PositionDownVisible))
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay1);
                }
                else if ((this.SensorsService.Sensors.LUPresentInBay2 || this.SensorsService.Sensors.LUPresentMiddleBottomBay2 ||
                          this.SensorsService.Sensors.RobotOptionBay2 || this.SensorsService.Sensors.TrolleyOptionBay2) &&
                         (this.HasBay2PositionUpVisible || this.HasBay2PositionDownVisible))
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay2);
                }
                else if ((this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.LUPresentMiddleBottomBay3 ||
                          this.SensorsService.Sensors.RobotOptionBay3 || this.SensorsService.Sensors.TrolleyOptionBay3) &&
                         (this.HasBay3PositionUpVisible || this.HasBay3PositionDownVisible))
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay3);
                }
                else
                {
                    if (this.sessionService.UserAccessLevel == UserAccessLevel.Operator)
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                    }
                    else
                    {
                        this.NavigationService.GoBack();
                    }
                }
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnElevator)
            {
                if ((this.SensorsService.Sensors.LUPresentInBay1 || this.SensorsService.Sensors.LUPresentMiddleBottomBay1 ||
                          this.SensorsService.Sensors.RobotOptionBay1 || this.SensorsService.Sensors.TrolleyOptionBay1) &&
                         (this.HasBay1PositionUpVisible || this.HasBay1PositionDownVisible))
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay1);
                }
                else if ((this.SensorsService.Sensors.LUPresentInBay2 || this.SensorsService.Sensors.LUPresentMiddleBottomBay2 ||
                          this.SensorsService.Sensors.RobotOptionBay2 || this.SensorsService.Sensors.TrolleyOptionBay2) &&
                         (this.HasBay2PositionUpVisible || this.HasBay2PositionDownVisible))
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay2);
                }
                else if ((this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.LUPresentMiddleBottomBay3 ||
                          this.SensorsService.Sensors.RobotOptionBay3 || this.SensorsService.Sensors.TrolleyOptionBay3) &&
                         (this.HasBay3PositionUpVisible || this.HasBay3PositionDownVisible))
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay3);
                }
                else
                {
                    if (this.sessionService.UserAccessLevel == UserAccessLevel.Operator)
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                    }
                    else
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepManualMode);
                    }
                }
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnBay1)
            {
                if ((this.SensorsService.Sensors.LUPresentInBay2 || this.SensorsService.Sensors.LUPresentMiddleBottomBay2 ||
                          this.SensorsService.Sensors.RobotOptionBay2 || this.SensorsService.Sensors.TrolleyOptionBay2) &&
                         (this.HasBay2PositionUpVisible || this.HasBay2PositionDownVisible))
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay2);
                }
                else if ((this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.LUPresentMiddleBottomBay3 ||
                          this.SensorsService.Sensors.RobotOptionBay3 || this.SensorsService.Sensors.TrolleyOptionBay3) &&
                         (this.HasBay3PositionUpVisible || this.HasBay3PositionDownVisible))
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay3);
                }
                else
                {
                    if (this.sessionService.UserAccessLevel == UserAccessLevel.Operator)
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                    }
                    else
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepManualMode);
                    }
                }
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnBay2)
            {
                if ((this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.LUPresentMiddleBottomBay3 ||
                          this.SensorsService.Sensors.RobotOptionBay3 || this.SensorsService.Sensors.TrolleyOptionBay3) &&
                         (this.HasBay3PositionUpVisible || this.HasBay3PositionDownVisible))
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay3);
                }
                else
                {
                    if (this.sessionService.UserAccessLevel == UserAccessLevel.Operator)
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                    }
                    else
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepManualMode);
                    }
                }
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnBay3)
            {
                if (this.sessionService.UserAccessLevel == UserAccessLevel.Operator)
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                }
                else
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepManualMode);
                }
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepAutomaticMode)
            {
                throw new NotSupportedException();
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepManualMode)
            {
                throw new NotSupportedException();
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

        private void OnStepChanged(StepChangedMessage e)
        {
            if (this.CurrentStep is ErrorLoadunitMissingStepStart)
            {
                if (e.Next)
                {
                    if (this.SensorsService.IsLoadingUnitOnElevator)
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnElevator);
                    }
                    else if ((this.SensorsService.Sensors.LUPresentInBay1 || this.SensorsService.Sensors.LUPresentMiddleBottomBay1) &&
                             (this.HasBay1PositionUpVisible || this.HasBay1PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay1);
                    }
                    else if ((this.SensorsService.Sensors.LUPresentInBay2 || this.SensorsService.Sensors.LUPresentMiddleBottomBay2) &&
                             (this.HasBay2PositionUpVisible || this.HasBay2PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay2);
                    }
                    else if ((this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.LUPresentMiddleBottomBay3) &&
                             (this.HasBay3PositionUpVisible || this.HasBay3PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay3);
                    }
                    else
                    {
                        if (this.sessionService.UserAccessLevel == UserAccessLevel.Operator)
                        {
                            this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                        }
                        else
                        {
                            this.CurrentStep = default(ErrorLoadunitMissingStepManualMode);
                        }
                    }
                }
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnElevator)
            {
                if (e.Next)
                {
                    if ((this.SensorsService.Sensors.LUPresentInBay1 || this.SensorsService.Sensors.LUPresentMiddleBottomBay1) &&
                             (this.HasBay1PositionUpVisible || this.HasBay1PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay1);
                    }
                    else if ((this.SensorsService.Sensors.LUPresentInBay2 || this.SensorsService.Sensors.LUPresentMiddleBottomBay2) &&
                             (this.HasBay2PositionUpVisible || this.HasBay2PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay2);
                    }
                    else if ((this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.LUPresentMiddleBottomBay3) &&
                             (this.HasBay3PositionUpVisible || this.HasBay3PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay3);
                    }
                    else
                    {
                        if (this.sessionService.UserAccessLevel == UserAccessLevel.Operator)
                        {
                            this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                        }
                        else
                        {
                            this.CurrentStep = default(ErrorLoadunitMissingStepManualMode);
                        }
                    }
                }
                else
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepStart);
                }
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnBay1)
            {
                if (e.Next)
                {
                    if ((this.SensorsService.Sensors.LUPresentInBay2 || this.SensorsService.Sensors.LUPresentMiddleBottomBay2) &&
                             (this.HasBay2PositionUpVisible || this.HasBay2PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay2);
                    }
                    else if ((this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.LUPresentMiddleBottomBay3) &&
                             (this.HasBay3PositionUpVisible || this.HasBay3PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay3);
                    }
                    else
                    {
                        if (this.sessionService.UserAccessLevel == UserAccessLevel.Operator)
                        {
                            this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                        }
                        else
                        {
                            this.CurrentStep = default(ErrorLoadunitMissingStepManualMode);
                        }
                    }
                }
                else
                {
                    if (this.SensorsService.IsLoadingUnitOnElevator)
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnElevator);
                    }
                    else
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepStart);
                    }
                }
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnBay2)
            {
                if (e.Next)
                {
                    if ((this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.LUPresentMiddleBottomBay3) &&
                             (this.HasBay3PositionUpVisible || this.HasBay3PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay3);
                    }
                    else
                    {
                        if (this.sessionService.UserAccessLevel == UserAccessLevel.Operator)
                        {
                            this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                        }
                        else
                        {
                            this.CurrentStep = default(ErrorLoadunitMissingStepManualMode);
                        }
                    }
                }
                else
                {
                    if ((this.SensorsService.Sensors.LUPresentInBay1 || this.SensorsService.Sensors.LUPresentMiddleBottomBay1) &&
                             (this.HasBay1PositionUpVisible || this.HasBay1PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay1);
                    }
                    else if (this.SensorsService.IsLoadingUnitOnElevator)
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnElevator);
                    }
                    else
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepStart);
                    }
                }
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnBay3)
            {
                if (e.Next)
                {
                    if (this.sessionService.UserAccessLevel == UserAccessLevel.Operator)
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                    }
                    else
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepManualMode);
                    }
                }
                else
                {
                    if ((this.SensorsService.Sensors.LUPresentInBay2 || this.SensorsService.Sensors.LUPresentMiddleBottomBay2) &&
                             (this.HasBay2PositionUpVisible || this.HasBay2PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay2);
                    }
                    else if ((this.SensorsService.Sensors.LUPresentInBay1 || this.SensorsService.Sensors.LUPresentMiddleBottomBay1) &&
                             (this.HasBay1PositionUpVisible || this.HasBay1PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay1);
                    }
                    else if (this.SensorsService.IsLoadingUnitOnElevator)
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnElevator);
                    }
                    else
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepStart);
                    }
                }
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepAutomaticMode)
            {
                if (!e.Next)
                {
                    if ((this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.LUPresentMiddleBottomBay3) &&
                        (this.HasBay3PositionUpVisible || this.HasBay3PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay3);
                    }
                    else if ((this.SensorsService.Sensors.LUPresentInBay2 || this.SensorsService.Sensors.LUPresentMiddleBottomBay2) &&
                             (this.HasBay2PositionUpVisible || this.HasBay2PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay2);
                    }
                    else if ((this.SensorsService.Sensors.LUPresentInBay1 || this.SensorsService.Sensors.LUPresentMiddleBottomBay1) &&
                             (this.HasBay1PositionUpVisible || this.HasBay1PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay1);
                    }
                    else if (this.SensorsService.IsLoadingUnitOnElevator)
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnElevator);
                    }
                    else
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepStart);
                    }
                }
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepManualMode)
            {
                if (!e.Next)
                {
                    if ((this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.LUPresentMiddleBottomBay3) &&
                        (this.HasBay3PositionUpVisible || this.HasBay3PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay3);
                    }
                    else if ((this.SensorsService.Sensors.LUPresentInBay2 || this.SensorsService.Sensors.LUPresentMiddleBottomBay2) &&
                             (this.HasBay2PositionUpVisible || this.HasBay2PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay2);
                    }
                    else if ((this.SensorsService.Sensors.LUPresentInBay1 || this.SensorsService.Sensors.LUPresentMiddleBottomBay1) &&
                             (this.HasBay1PositionUpVisible || this.HasBay1PositionDownVisible))
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay1);
                    }
                    else if (this.SensorsService.IsLoadingUnitOnElevator)
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnElevator);
                    }
                    else
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepStart);
                    }
                }
            }

            this.RaiseCanExecuteChanged();
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
            this.loadunitsToken = this.loadunitsToken
                 ??
                 this.EventAggregator
                     .GetEvent<LoadUnitsChangedPubSubEvent>()
                     .Subscribe(
                         m => this.RaiseCanExecuteChanged(),
                         ThreadOption.UIThread,
                         false);

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
                           this.RaisePropertyChanged(nameof(this.HasStepStart));
                           this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnElevator));
                           this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnBay1));
                           this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnBay2));
                           this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnBay3));
                           this.RaisePropertyChanged(nameof(this.HasStepAutomaticMode));
                           this.RaisePropertyChanged(nameof(this.HasStepManualMode));

                           this.RaisePropertyChanged(nameof(this.HasBay1PositionDownVisible));
                           this.RaisePropertyChanged(nameof(this.HasBay1PositionUpVisible));
                           this.RaisePropertyChanged(nameof(this.HasBay2PositionDownVisible));
                           this.RaisePropertyChanged(nameof(this.HasBay2PositionUpVisible));
                           this.RaisePropertyChanged(nameof(this.HasBay3PositionDownVisible));
                           this.RaisePropertyChanged(nameof(this.HasBay3PositionUpVisible));
                       },
                       ThreadOption.UIThread,
                       false);
        }

        private void UpdateStatusButtonFooter()
        {
            if (this.CurrentStep is ErrorLoadunitMissingStepStart)
            {
                this.ShowPrevStepSinglePage(true, false);
                this.ShowNextStepSinglePage(true, this.moveToNextCommand?.CanExecute() ?? false);
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnElevator)
            {
                this.ShowPrevStepSinglePage(true, !this.IsMoving);
                this.ShowNextStepSinglePage(true, !this.IsMoving);
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnBay1)
            {
                this.ShowPrevStepSinglePage(true, !this.IsMoving);
                this.ShowNextStepSinglePage(true, !this.IsMoving);
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnBay2)
            {
                this.ShowPrevStepSinglePage(true, !this.IsMoving);
                this.ShowNextStepSinglePage(true, !this.IsMoving);
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnBay3)
            {
                this.ShowPrevStepSinglePage(true, !this.IsMoving);
                this.ShowNextStepSinglePage(true, !this.IsMoving);
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepAutomaticMode)
            {
                this.ShowPrevStepSinglePage(true, !this.IsMoving);
                this.ShowNextStepSinglePage(true, false);
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepManualMode)
            {
                this.ShowPrevStepSinglePage(true, !this.IsMoving);
                this.ShowNextStepSinglePage(true, false);
            }

            this.ShowAbortStep(true, !this.IsMoving);

            this.RaisePropertyChanged(nameof(this.HasStepStart));
            this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnElevator));
            this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnBay1));
            this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnBay2));
            this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnBay3));
            this.RaisePropertyChanged(nameof(this.HasStepAutomaticMode));
            this.RaisePropertyChanged(nameof(this.HasStepManualMode));

            this.RaisePropertyChanged(nameof(this.HasBay1PositionDownVisible));
            this.RaisePropertyChanged(nameof(this.HasBay1PositionUpVisible));
            this.RaisePropertyChanged(nameof(this.HasBay2PositionDownVisible));
            this.RaisePropertyChanged(nameof(this.HasBay2PositionUpVisible));
            this.RaisePropertyChanged(nameof(this.HasBay3PositionDownVisible));
            this.RaisePropertyChanged(nameof(this.HasBay3PositionUpVisible));
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
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

    internal struct ErrorLoadunitMissingStepStart
    {
    }

    internal sealed class ErrorLoadunitMissingViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineErrorsWebService machineErrorsWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineModeWebService machineModeWebService;

        private DelegateCommand automaticCommand;

        private bool canLuIdOnElevator;

        private string currentError;

        private object currentStep = default(ErrorLoadunitMissingStepStart);

        private string errorTime;

        private SubscriptionToken loadunitsToken;

        private int? luIdOnBay1Down;

        private int? luIdOnBay1Up;

        private int? luIdOnBay2Down;

        private int? luIdOnBay2Up;

        private int? luIdOnBay3Down;

        private int? luIdOnBay3Up;

        private int? luIdOnElevator;

        private MachineError machineError;

        private DelegateCommand markAsResolvedCommand;

        private DelegateCommand moveLoadunitCommand;

        private DelegateCommand moveToNextCommand;

        private LoadingUnit selectedLoadingUnit;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public ErrorLoadunitMissingViewModel(
            IMachineModeWebService machineModeWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineErrorsWebService machineErrorsWebService)
            : base(Services.PresentationMode.Menu | Services.PresentationMode.Installer | Services.PresentationMode.Operator)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
            this.machineModeWebService = machineModeWebService ?? throw new ArgumentNullException(nameof(machineModeWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));

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

        public bool HasBay1PositionDownVisible => this.Bay1Positions?.Any(a => a.LocationUpDown == LoadingUnitLocation.Down) ?? false;

        public bool HasBay1PositionUpVisible => this.Bay1Positions?.Any(a => a.LocationUpDown == LoadingUnitLocation.Up) ?? false;

        public bool HasBay2PositionDownVisible => this.Bay2Positions?.Any(a => a.LocationUpDown == LoadingUnitLocation.Down) ?? false;

        public bool HasBay2PositionUpVisible => this.Bay2Positions?.Any(a => a.LocationUpDown == LoadingUnitLocation.Up) ?? false;

        public bool HasBay3PositionDownVisible => this.Bay3Positions?.Any(a => a.LocationUpDown == LoadingUnitLocation.Down) ?? false;

        public bool HasBay3PositionUpVisible => this.Bay3Positions?.Any(a => a.LocationUpDown == LoadingUnitLocation.Up) ?? false;

        public bool HasStepAutomaticMode => this.currentStep is ErrorLoadunitMissingStepAutomaticMode;

        public bool HasStepLoadunitOnBay1 => this.currentStep is ErrorLoadunitMissingStepLoadunitOnBay1;

        public bool HasStepLoadunitOnBay2 => this.currentStep is ErrorLoadunitMissingStepLoadunitOnBay2;

        public bool HasStepLoadunitOnBay3 => this.currentStep is ErrorLoadunitMissingStepLoadunitOnBay3;

        public bool HasStepLoadunitOnElevator => this.currentStep is ErrorLoadunitMissingStepLoadunitOnElevator;

        public bool HasStepStart => this.currentStep is ErrorLoadunitMissingStepStart;

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
                () =>
                {
                    if (this.CurrentStep is ErrorLoadunitMissingStepStart)
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
                            this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                        }
                    }
                    else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnElevator)
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
                            this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                        }
                    }
                    else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnBay1)
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
                            this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                        }
                    }
                    else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnBay2)
                    {
                        if ((this.SensorsService.Sensors.LUPresentInBay3 || this.SensorsService.Sensors.LUPresentMiddleBottomBay3) &&
                                 (this.HasBay3PositionUpVisible || this.HasBay3PositionDownVisible))
                        {
                            this.CurrentStep = default(ErrorLoadunitMissingStepLoadunitOnBay3);
                        }
                        else
                        {
                            this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                        }
                    }
                    else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunitOnBay3)
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                    }
                    else if (this.CurrentStep is ErrorLoadunitMissingStepAutomaticMode)
                    {
                        throw new NotSupportedException();
                    }
                },
                () => this.CanBaseExecute() &&
                      this.MachineService.MachineMode != MachineMode.Automatic &&
                      this.MachineService.MachinePower == MachinePowerState.Powered
                ));

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

                        if ((!this.LuIdOnElevator.HasValue) || (!this.MachineService.Loadunits.Any(l => l.Id == this.LuIdOnElevator && l.Status == LoadingUnitStatus.Undefined && l.Height != 0)))
                        {
                            var lus = this.MachineService.Loadunits.Where(l => l.Status == LoadingUnitStatus.Undefined && l.Height != 0).Select(s => s.Id.ToString()).Aggregate((a, b) => a + ", " + b);
                            return $"Cassetto non valido, inserire: {lus}";
                        }

                        break;

                    case nameof(this.LuIdOnBay1Down):

                        break;

                    case nameof(this.LuIdOnBay1Up):

                        break;

                    case nameof(this.LuIdOnBay2Down):

                        break;

                    case nameof(this.LuIdOnBay2Up):

                        break;

                    case nameof(this.LuIdOnBay3Down):

                        break;

                    case nameof(this.LuIdOnBay3Up):

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
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            int row = 0;

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

                this.LuIdOnElevator = null;
                if (this.SensorsService.IsLoadingUnitOnElevator)
                {
                    this.LuIdOnElevator = this.MachineService.Loadunits.First(l => l.Status == LoadingUnitStatus.Undefined && l.Height != 0).Id;
                }
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

        protected void OnStepChanged(StepChangedMessage e)
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
                        this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
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
                        this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
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
                        this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
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
                        this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
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
                    this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
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

                if (this.SensorsService.IsLoadingUnitOnElevator && this.LuIdOnElevator.HasValue)
                {
                    await this.machineElevatorWebService.SetLoadUnitOnElevatorAsync(this.LuIdOnElevator.Value);
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
                this.ErrorTime = Resources.General.Now;
            }
            else if (elapsedTime.TotalHours < 1)
            {
                this.ErrorTime = string.Format(Resources.General.MinutesAgo, elapsedTime.TotalMinutes);
            }
            else if (elapsedTime.TotalDays < 1)
            {
                this.ErrorTime = string.Format(Resources.General.HoursAgo, elapsedTime.TotalHours);
            }
            else
            {
                this.ErrorTime = string.Format(Resources.General.DaysAgo, elapsedTime.TotalDays);
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

            this.ShowAbortStep(true, !this.IsMoving);

            this.RaisePropertyChanged(nameof(this.HasStepStart));
            this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnElevator));
            this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnBay1));
            this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnBay2));
            this.RaisePropertyChanged(nameof(this.HasStepLoadunitOnBay3));
            this.RaisePropertyChanged(nameof(this.HasStepAutomaticMode));

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

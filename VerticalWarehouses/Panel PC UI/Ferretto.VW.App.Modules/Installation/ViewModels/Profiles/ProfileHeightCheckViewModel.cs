using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using LoadingUnitLocation = Ferretto.VW.MAS.AutomationService.Contracts.LoadingUnitLocation;
using ShutterMovementDirection = Ferretto.VW.MAS.AutomationService.Contracts.ShutterMovementDirection;

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

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineProfileProcedureWebService machineProfileProcedureWeb;

        private readonly IMachineShuttersWebService shuttersWebService;

        private DelegateCommand callLoadunitToBayCommand;

        private bool canLoadingUnitId;

        private DelegateCommand closeShutterCommand;

        private string currentError;

        private ProfileCheckStep currentStep;

        private DelegateCommand goToBayCommand;

        private int? loadingUnitId;

        private DelegateCommand mensurationDxCommand;

        private DelegateCommand mensurationSxCommand;

        private DelegateCommand moveToElevatorPositionCommand;

        private DelegateCommand moveToShapePositionDxCommand;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public ProfileHeightCheckViewModel(
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

            this.CurrentStep = ProfileCheckStep.Initialize;
        }

        #endregion

        #region Properties

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

        public bool HasStepElevatorPosition => this.currentStep is ProfileCheckStep.ElevatorPosition;

        public bool HasStepInitialize => this.currentStep is ProfileCheckStep.Initialize;

        public bool HasStepResultCheck => this.currentStep is ProfileCheckStep.ResultCheck;

        public bool HasStepShapePositionDx => this.currentStep is ProfileCheckStep.ShapePositionDx;

        public bool HasStepShapePositionSx => this.currentStep is ProfileCheckStep.ShapePositionSx;

        public bool HasStepTuningChainDx => this.currentStep is ProfileCheckStep.TuningChainDx;

        public bool HasStepTuningChainSx => this.currentStep is ProfileCheckStep.TuningChainSx;

        public int? LoadingUnitId
        {
            get => this.loadingUnitId;
            set => this.SetProperty(ref this.loadingUnitId, value, this.RaiseCanExecuteChanged);
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
                             !this.MachineService.Loadunits.DrawerInBayById(this.LoadingUnitId.Value)))
                        {
                            return VW.App.Resources.InstallationApp.InvalidDrawerSelected;
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
                        this.CurrentStep = ProfileCheckStep.TuningChainDx;
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
                        this.CurrentStep = ProfileCheckStep.TuningChainSx;
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
            this.closeShutterCommand?.RaiseCanExecuteChanged();
            this.goToBayCommand?.RaiseCanExecuteChanged();
            this.moveToShapePositionDxCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();
            this.mensurationSxCommand?.RaiseCanExecuteChanged();
            this.mensurationDxCommand?.RaiseCanExecuteChanged();

            this.UpdateStatusButtonFooter();
        }

        private async Task CallLoadunitToBayCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.EjectLoadingUnitAsync(this.MachineService.GetBayPositionSourceByDestination(false), this.LoadingUnitId.Value); ;
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
                   !this.SensorsService.ShutterSensors.Closed;
        }

        private bool CanGoToBayCommand()
        {
            return this.CanBaseExecute() &&
                   Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.GetValueOrDefault()) != Convert.ToInt32(this.BayPosition.Height);
        }

        private bool CanMensurationDx()
        {
            return this.CanBaseExecute();
        }

        private bool CanMensurationSx()
        {
            return this.CanBaseExecute();
        }

        private bool CanMoveToElevatorPosition()
        {
            return this.CanBaseExecute() &&
                   this.SensorsService.IsLoadingUnitInBay &&
                   string.IsNullOrEmpty(this.Error);
        }

        private bool CanMoveToShapePositionDx()
        {
            var res = this.CanBaseExecute() &&
                      Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.GetValueOrDefault()) == Convert.ToInt32(this.BayPosition.Height);

            this.ShowNextStepSinglePage(true, res);

            return res;
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
                await this.shuttersWebService.MoveToAsync(MAS.AutomationService.Contracts.ShutterPosition.Closed);
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
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, false);
                    break;

                case ProfileCheckStep.ResultCheck:
                    this.ShowPrevStepSinglePage(true, true);
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

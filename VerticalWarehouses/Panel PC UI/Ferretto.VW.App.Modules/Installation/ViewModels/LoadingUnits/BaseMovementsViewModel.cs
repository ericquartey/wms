﻿using System;
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
using Prism.Regions;
using BayNumber = Ferretto.VW.MAS.AutomationService.Contracts.BayNumber;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    public partial class BaseMovementsViewModel : BaseMainViewModel, IRegionMemberLifetime
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private readonly IMachineExternalBayWebService machineExternalBayWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineModeWebService machineModeWebService;

        private SubscriptionToken fsmExceptionToken;

        private bool isExecutingProcedure;

        private bool isExternalBayMoving;

        private bool isPositionDownSelected;

        private bool isPositionUpSelected;

        private bool isStopping;

        private int? loadingUnitId;

        private DelegateCommand loadingUnitsCommand;

        private DelegateCommand moveExtBayMovementForExtractionCommand;

        private DelegateCommand moveExtBayMovementForInsertionCommand;

        private DelegateCommand moveExtBayTowardMachineCommand;

        private DelegateCommand moveExtBayTowardOperatorCommand;

        private SubscriptionToken moveLoadingUnitToken;

        private SubscriptionToken positioningToken;

        private DelegateCommand selectBayPositionDownCommand;

        private DelegateCommand selectBayPositionUpCommand;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public BaseMovementsViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineModeWebService machineModeWebService,
            IBayManager bayManagerService,
            IMachineExternalBayWebService machineExternalBayWebService)
            : base(PresentationMode.Installer)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machineModeWebService = machineModeWebService ?? throw new ArgumentNullException(nameof(machineModeWebService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));
            this.machineExternalBayWebService = machineExternalBayWebService ?? throw new ArgumentNullException(nameof(machineExternalBayWebService));
        }

        #endregion

        #region Properties

        public int? CurrentMissionId { get; private set; }

        public ICommand ExtBayMovementForExtractionCommand =>
            this.moveExtBayMovementForExtractionCommand
            ??
            (this.moveExtBayMovementForExtractionCommand = new DelegateCommand(
                async () => await this.MoveExtBayForExtractionAsync(),
                this.CanMoveExtBayForExtraction));

        public ICommand ExtBayMovementForInsertionCommand =>
            this.moveExtBayMovementForInsertionCommand
            ??
            (this.moveExtBayMovementForInsertionCommand = new DelegateCommand(
                async () => await this.MoveExtBayForInsertionAsync(),
                this.CanMoveExtBayForInsertion));

        public ICommand ExtBayTowardMachineCommand =>
                   this.moveExtBayTowardMachineCommand
           ??
           (this.moveExtBayTowardMachineCommand = new DelegateCommand(
               async () => await this.MoveExtBayTowardMachineAsync(),
               this.CanMoveExtBayTowardMachine));

        public ICommand ExtBayTowardOperatorCommand =>
                            this.moveExtBayTowardOperatorCommand
            ??
            (this.moveExtBayTowardOperatorCommand = new DelegateCommand(
                async () => await this.MoveExtBayTowardOperatorAsync(),
                this.CanMoveExtBayTowardOperator));

        public bool HasShutter => this.MachineService.HasShutter;

        public bool HasExternalDouble => this.MachineService.Bay.IsExternal && this.MachineService.Bay.IsDouble;

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

        public bool IsExternalBayMoving
        {
            get => this.isExternalBayMoving;
            private set => this.SetProperty(ref this.isExternalBayMoving, value, this.RaiseCanExecuteChanged);
        }

        public bool IsLoadingUnitIdValid
        {
            get
            {
                if (!this.loadingUnitId.HasValue)
                {
                    return false;
                }

                return this.MachineService.Loadunits.Any(l => l.Id == this.loadingUnitId.Value);
            }
        }

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

        public bool IsStopping
        {
            get => this.isStopping;
            set => this.SetProperty(ref this.isStopping, value);
        }

        public override bool KeepAlive => true;

        public int? LoadingUnitCellId
        {
            get
            {
                if (!this.loadingUnitId.HasValue)
                {
                    return null;
                }

                return this.MachineService.Loadunits.FirstOrDefault(l => l.Id == this.loadingUnitId.Value)?.CellId;
            }
        }

        public int? LoadingUnitId
        {
            get => this.loadingUnitId;
            set
            {
                if (this.SetProperty(ref this.loadingUnitId, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand LoadingUnitsCommand =>
            this.loadingUnitsCommand
            ??
            (this.loadingUnitsCommand = new DelegateCommand(
                () => this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.CellsLoadingUnitsMenu.LOADINGUNITS,
                        data: null,
                        trackCurrentView: true),
                () => !this.IsMoving));

        public IMachineLoadingUnitsWebService MachineLoadingUnitsWebService => this.machineLoadingUnitsWebService;

        public IMachineModeWebService MachineModeWebService => this.machineModeWebService;

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

        public ICommand StartCommand =>
               this.startCommand
               ??
               (this.startCommand = new DelegateCommand(
                   async () => await this.StartAsync(),
                   this.CanStart));

        public ICommand StopCommand =>
                this.stopCommand
                ??
                (this.stopCommand = new DelegateCommand(
                    async () => await this.StopAsync(),
                    this.CanStop));

        #endregion

        #region Methods

        public virtual bool CanSelectBayPositionDown()
        {
            return !this.IsExecutingProcedure &&
                   this.IsPositionUpSelected;
        }

        public virtual bool CanSelectBayPositionUp()
        {
            return !this.IsExecutingProcedure &&
                   !this.IsPositionUpSelected;
        }

        public virtual bool CanStart()
        {
            return
                !this.IsExecutingProcedure;
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

            if (this.fsmExceptionToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<FsmExceptionMessageData>>().Unsubscribe(this.fsmExceptionToken);
                this.fsmExceptionToken?.Dispose();
                this.fsmExceptionToken = null;
            }
        }

        public MAS.AutomationService.Contracts.LoadingUnitLocation GetLoadingUnitSource(bool isPositionDownSelected)
        {
            return this.GetLoadingUnitSourceByDestination(this.MachineService.BayNumber, isPositionDownSelected);
        }

        public MAS.AutomationService.Contracts.LoadingUnitLocation GetLoadingUnitSourceByDestination(MAS.AutomationService.Contracts.BayNumber bayNumber, bool isPositionDownSelected)
        {
            if (bayNumber == MAS.AutomationService.Contracts.BayNumber.BayOne)
            {
                if (isPositionDownSelected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay1Down;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay1Up;
                }
            }

            if (bayNumber == MAS.AutomationService.Contracts.BayNumber.BayTwo)
            {
                if (isPositionDownSelected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay2Down;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay2Up;
                }
            }

            if (bayNumber == MAS.AutomationService.Contracts.BayNumber.BayThree)
            {
                if (isPositionDownSelected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay3Down;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay3Up;
                }
            }

            return MAS.AutomationService.Contracts.LoadingUnitLocation.NoLocation;
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            await base.OnAppearedAsync();
        }

        public virtual void SelectBayPositionDown()
        {
            this.IsPositionDownSelected = true;
            this.selectBayPositionDownCommand?.RaiseCanExecuteChanged();
            this.selectBayPositionUpCommand?.RaiseCanExecuteChanged();

            this.RaiseCanExecuteChanged();
        }

        public virtual void SelectBayPositionUp()
        {
            this.IsPositionUpSelected = true;
            this.selectBayPositionDownCommand?.RaiseCanExecuteChanged();
            this.selectBayPositionUpCommand?.RaiseCanExecuteChanged();

            this.RaiseCanExecuteChanged();
        }

        public virtual Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public virtual async Task StopAsync()
        {
            try
            {
                this.IsStopping = true;
                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.StopAsync(null, this.MachineService.BayNumber);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }

            this.RestoreStates();
        }

        protected virtual void Ended()
        {
            this.RestoreStates();

            this.ShowNotification(
                VW.App.Resources.Localized.Get("InstallationApp.ProcedureCompleted"),
                Services.Models.NotificationSeverity.Success);
        }

        protected virtual void NotifyError(Exception ex, string errorMessage)
        {
            this.RestoreStates();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                this.ShowNotification(errorMessage, Services.Models.NotificationSeverity.Error);
            }
            else
            {
                this.ShowNotification(ex);
            }
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            await base.OnMachinePowerChangedAsync(e);

            if (e.MachinePowerState != MachinePowerState.Powered)
            {
                this.RestoreStates();
            }
        }

        protected override async Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            await base.OnMachineStatusChangedAsync(e);

            if (!this.IsMachineMoving)
            {
                this.IsStopping = false;
            }
        }

        protected virtual void OnWaitResume()
        {
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.startCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();

            this.loadingUnitsCommand?.RaiseCanExecuteChanged();

            this.selectBayPositionDownCommand?.RaiseCanExecuteChanged();
            this.selectBayPositionUpCommand?.RaiseCanExecuteChanged();
            this.moveExtBayMovementForExtractionCommand?.RaiseCanExecuteChanged();
            this.moveExtBayMovementForInsertionCommand?.RaiseCanExecuteChanged();
            this.moveExtBayTowardOperatorCommand?.RaiseCanExecuteChanged();
            this.moveExtBayTowardMachineCommand?.RaiseCanExecuteChanged();
        }

        private bool CanBaseExecute()
        {
            switch (this.MachineService.BayNumber)
            {
                case BayNumber.BayOne:
                default:
                    return this.MachineModeService.MachineMode == MachineMode.Manual &&
                   this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                   !this.IsKeyboardOpened &&
                   !this.IsExecutingProcedure &&
                   !this.IsMoving;

                case BayNumber.BayTwo:
                    return this.MachineModeService.MachineMode == MachineMode.Manual2 &&
                   this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                   !this.IsKeyboardOpened &&
                   !this.IsExecutingProcedure &&
                   !this.IsMoving;

                case BayNumber.BayThree:
                    return this.MachineModeService.MachineMode == MachineMode.Manual3 &&
                   this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                   !this.IsKeyboardOpened &&
                   !this.IsExecutingProcedure &&
                   !this.IsMoving;
            }
        }

        private bool CanMoveExtBayForExtraction()
        {
            if (this.MachineService.Bay.IsDouble)
            {
                return false;
                //return
                //!this.IsExecutingProcedure &&
                //!this.SensorsService.BayZeroChain &&
                //!this.IsExternalBayMoving &&
                //!this.SensorsService.BEDExternalBayBottom &&
                //!this.SensorsService.BEDInternalBayTop;
            }
            else
            {
                return
                !this.IsExecutingProcedure &&
                !this.SensorsService.BayZeroChain &&
                !this.IsExternalBayMoving &&
                !this.SensorsService.IsLoadingUnitInMiddleBottomBay;
            }
        }

        private bool CanMoveExtBayForInsertion()
        {
            if (this.MachineService.Bay.IsDouble)
            {
                return false;
                //return
                //!this.IsExecutingProcedure &&
                //!this.SensorsService.BayZeroChain &&
                //!this.IsExternalBayMoving &&
                //!this.SensorsService.BEDInternalBayBottom &&
                //!this.SensorsService.BEDExternalBayTop;
            }
            else
            {
                return
                !this.IsExecutingProcedure &&
                !this.SensorsService.BayZeroChain &&
                !this.IsExternalBayMoving &&
                !this.SensorsService.IsLoadingUnitInMiddleBottomBay;
            }
        }

        private bool CanMoveExtBayTowardMachine()
        {
            if (this.MachineService.Bay.IsDouble)
            {
                return this.CanBaseExecute() &&
                        this.SensorsService.BayZeroChainUp &&
                        !this.SensorsService.BEDInternalBayBottom &&
                        !this.SensorsService.BEDExternalBayTop;
            }
            else
            {
                return
                    this.CanBaseExecute() &&
                    !this.SensorsService.BayZeroChain;
            }
        }

        private bool CanMoveExtBayTowardOperator()
        {
            if (this.MachineService.Bay.IsDouble)
            {
                return this.CanBaseExecute() &&
                        this.SensorsService.BayZeroChain &&
                        !this.SensorsService.BEDExternalBayBottom &&
                        !this.SensorsService.BEDInternalBayTop;
            }
            else
            {
                return
                    this.CanBaseExecute() &&
                    this.SensorsService.BayZeroChain;
            }
        }

        private bool CanStop()
        {
            return
                this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse;
        }

        private async Task MoveExtBayForExtractionAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                if (this.MachineService.Bay.IsDouble)
                {
                    if (this.SensorsService.BayZeroChain)
                    {
                        await this.machineExternalBayWebService.MovementForExtractionAsync(false);
                    }
                    else if (this.SensorsService.BayZeroChainUp)
                    {
                        await this.machineExternalBayWebService.MovementForExtractionAsync(true);
                    }
                }
                else
                {
                    await this.machineExternalBayWebService.MovementForExtractionAsync(false);
                }

                this.IsExternalBayMoving = true;
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

        private async Task MoveExtBayForInsertionAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                if (this.MachineService.Bay.IsDouble)
                {
                    if (this.SensorsService.BayZeroChain)
                    {
                        await this.machineExternalBayWebService.MovementForInsertionAsync(false);
                    }
                    else if (this.SensorsService.BayZeroChainUp)
                    {
                        await this.machineExternalBayWebService.MovementForInsertionAsync(true);
                    }
                }
                else
                {
                    await this.machineExternalBayWebService.MovementForInsertionAsync(false);
                }

                this.IsExternalBayMoving = true;
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

        private async Task MoveExtBayTowardMachineAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                if (this.MachineService.Bay.IsDouble)
                {
                    //var selectedBayPosition = this.SelectedBayPosition();

                    if (this.SensorsService.BayZeroChain)
                    {
                        await this.machineExternalBayWebService.MoveAssistedExternalBayAsync(ExternalBayMovementDirection.TowardMachine, true);
                    }
                    else if (this.SensorsService.BayZeroChainUp)
                    {
                        await this.machineExternalBayWebService.MoveAssistedExternalBayAsync(ExternalBayMovementDirection.TowardMachine, false);
                    }
                }
                else
                {
                    await this.machineExternalBayWebService.MoveAssistedAsync(ExternalBayMovementDirection.TowardMachine);
                }
                this.IsExternalBayMoving = true;
                this.IsExecutingProcedure = true;
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

        private async Task MoveExtBayTowardOperatorAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                if (this.MachineService.Bay.IsDouble)
                {
                    if (this.SensorsService.BayZeroChain)
                    {
                        await this.machineExternalBayWebService.MoveAssistedExternalBayAsync(ExternalBayMovementDirection.TowardOperator, true);
                    }
                    else if (this.SensorsService.BayZeroChainUp)
                    {
                        await this.machineExternalBayWebService.MoveAssistedExternalBayAsync(ExternalBayMovementDirection.TowardOperator, false);
                    }
                }
                else
                {
                    await this.machineExternalBayWebService.MoveAssistedAsync(ExternalBayMovementDirection.TowardOperator);
                }
                this.IsExternalBayMoving = true;
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

        private void OnFsmException(NotificationMessageUI<FsmExceptionMessageData> message)
        {
            var data = message?.Data as FsmExceptionMessageData;
            var ex = data?.InnerException as Exception;

            this.NotifyError(ex, data.ExceptionDescription);
        }

        private void OnMoveLoadingUnitChanged(NotificationMessageUI<MoveLoadingUnitMessageData> message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.IsExecutingProcedure = true;
                    this.RaiseCanExecuteChanged();

                    this.CurrentMissionId = (message.Data as MoveLoadingUnitMessageData).MissionId;

                    break;

                case MessageStatus.OperationWaitResume:
                    this.OnWaitResume();
                    break;

                case MessageStatus.OperationEnd:
                    if (!this.IsExecutingProcedure)
                    {
                        break;
                    }

                    this.Ended();

                    break;

                case MessageStatus.OperationStop:
                case MessageStatus.OperationFaultStop:
                case MessageStatus.OperationRunningStop:
                    this.Stopped();

                    break;

                case MessageStatus.OperationError:
                    this.IsExecutingProcedure = false;

                    break;
            }
        }

        private void OnPositioningChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    {
                        if (message.Data?.MovementMode == CommonUtils.Messages.Enumerations.MovementMode.ExtBayChain
                            || message.Data?.MovementMode == CommonUtils.Messages.Enumerations.MovementMode.ExtBayChainManual
                            )
                        {
                            this.IsExternalBayMoving = false;
                        }

                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.IsExternalBayMoving = false;
                        break;
                    }
            }
        }

        private void RestoreStates()
        {
            this.IsExecutingProcedure = false;
            this.IsExternalBayMoving = false;

            this.RaiseCanExecuteChanged();
        }

        private void Stopped()
        {
            this.RestoreStates();

            this.ShowNotification(
                Resources.Localized.Get("InstallationApp.ProcedureWasStopped"),
                Services.Models.NotificationSeverity.Warning);
        }

        private void SubscribeToEvents()
        {
            this.moveLoadingUnitToken = this.moveLoadingUnitToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<MoveLoadingUnitMessageData>>()
                    .Subscribe(
                        this.OnMoveLoadingUnitChanged,
                        ThreadOption.UIThread,
                        false);

            this.positioningToken = this.positioningToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        this.OnPositioningChanged,
                        ThreadOption.UIThread,
                        false);

            this.fsmExceptionToken = this.fsmExceptionToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<FsmExceptionMessageData>>()
                    .Subscribe(
                        this.OnFsmException,
                        ThreadOption.UIThread,
                        false);
        }

        #endregion
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;
using Axis = Ferretto.VW.MAS.AutomationService.Contracts.Axis;
using HorizontalMovementDirection = Ferretto.VW.MAS.AutomationService.Contracts.HorizontalMovementDirection;
using ShutterMovementDirection = Ferretto.VW.MAS.AutomationService.Contracts.ShutterMovementDirection;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class MovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private bool canInputCellId;

        private bool canInputHeight;

        private bool canInputLoadingUnitId;

        private bool canMoveCarouselCloseCommand;

        private bool canMoveCarouselOpenCommand;

        private bool canMoveElevatorBackwardsCommand;

        private bool canMoveElevatorDownCommand;

        private bool canMoveElevatorForwardCommand;

        private bool canMoveElevatorUpCommand;

        private bool canMoveManualExternalBayTowardMachineCommand;

        private bool canMoveManualExternalBayTowardOperatorCommand;

        private bool canShutterMoveDownCommand;

        private bool canShutterMoveUpCommand;

        private int? inputCellId;

        private double? inputHeight;

        private bool isCarouselClosing;

        private bool isCarouselOpening;

        private bool isElevatorMoving;

        private bool isExternalBayManualMovementTowardMachine;

        private bool isExternalBayManualMovementTowardOperator;

        private bool isLightActive;

        private bool isManualMovementCompleted;

        private bool isMovingElevatorBackwards;

        private bool isMovingElevatorDown;

        private bool isMovingElevatorForwards;

        private bool isMovingElevatorUp;

        private bool isPolicyBypassed;

        private DelegateCommand isPolicyBypassedCommand;

        private bool isShutterMovingDown;

        private bool isShutterMovingUp;

        private string lastActiveCommand;

        private DelegateCommand lightCommand;

        private string lightIcon;

        private DelegateCommand moveCarouselCloseCommand;

        private DelegateCommand moveCarouselOpenCommand;

        private DelegateCommand moveElevatorBackwardsCommand;

        private DelegateCommand moveElevatorDownCommand;

        private DelegateCommand moveElevatorForwardsCommand;

        private DelegateCommand moveElevatorUpCommand;

        private DelegateCommand moveExternalBayTowardMachineManualCommand;

        private DelegateCommand moveExternalBayTowardOperatorManualCommand;

        private DelegateCommand moveToCellHeightCommand;

        private DelegateCommand moveToHeightCommand;

        private SubscriptionToken sensorsTokenManual;

        private DelegateCommand shutterMoveDownCommand;

        private DelegateCommand shutterMoveUpCommand;

        #endregion

        #region Properties

        public bool CanInputCellId
        {
            get => this.canInputCellId;
            private set => this.SetProperty(ref this.canInputCellId, value);
        }

        public bool CanInputHeight
        {
            get => this.canInputHeight;
            private set => this.SetProperty(ref this.canInputHeight, value);
        }

        public bool CanInputLoadingUnitId
        {
            get => this.canInputLoadingUnitId;
            private set => this.SetProperty(ref this.canInputLoadingUnitId, value);
        }

        public bool CanMoveCarouselCloseCommand
        {
            get => this.canMoveCarouselCloseCommand;
            private set => this.SetProperty(ref this.canMoveCarouselCloseCommand, value);
        }

        public bool CanMoveCarouselOpenCommand
        {
            get => this.canMoveCarouselOpenCommand;
            private set => this.SetProperty(ref this.canMoveCarouselOpenCommand, value);
        }

        public bool CanMoveElevatorBackwards
        {
            get => this.canMoveElevatorBackwardsCommand;
            private set => this.SetProperty(ref this.canMoveElevatorBackwardsCommand, value);
        }

        public bool CanMoveElevatorDown
        {
            get => this.canMoveElevatorDownCommand;
            private set => this.SetProperty(ref this.canMoveElevatorDownCommand, value);
        }

        public bool CanMoveElevatorForwards
        {
            get => this.canMoveElevatorForwardCommand;
            private set => this.SetProperty(ref this.canMoveElevatorForwardCommand, value);
        }

        public bool CanMoveElevatorUp
        {
            get => this.canMoveElevatorUpCommand;
            private set => this.SetProperty(ref this.canMoveElevatorUpCommand, value);
        }

        public bool CanMoveManualExternalBayTowardMachineCommand
        {
            get => this.canMoveManualExternalBayTowardMachineCommand;
            private set => this.SetProperty(ref this.canMoveManualExternalBayTowardMachineCommand, value);
        }

        public bool CanMoveManualExternalBayTowardOperatorCommand
        {
            get => this.canMoveManualExternalBayTowardOperatorCommand;
            private set => this.SetProperty(ref this.canMoveManualExternalBayTowardOperatorCommand, value);
        }

        public bool CanShutterMoveDownCommand
        {
            get => this.canShutterMoveDownCommand;
            private set => this.SetProperty(ref this.canShutterMoveDownCommand, value);
        }

        public bool CanShutterMoveUpCommand
        {
            get => this.canShutterMoveUpCommand;
            private set => this.SetProperty(ref this.canShutterMoveUpCommand, value);
        }

        public int? InputCellId
        {
            get => this.inputCellId;
            set => this.SetProperty(ref this.inputCellId, value, this.InputCellIdPropertyChanged); // HACK: 2
        }

        public double? InputHeight
        {
            get => this.inputHeight;
            set => this.SetProperty(ref this.inputHeight, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCarouselClosing
        {
            get => this.isCarouselClosing;
            private set => this.SetProperty(ref this.isCarouselClosing, value);
        }

        public bool IsCarouselOpening
        {
            get => this.isCarouselOpening;
            private set => this.SetProperty(ref this.isCarouselOpening, value);
        }

        public bool IsElevatorMoving
        {
            get => this.isElevatorMoving;
            private set => this.SetProperty(ref this.isElevatorMoving, value);
        }

        public bool IsExternalBayManualMovementTowardMachine
        {
            get => this.isExternalBayManualMovementTowardMachine;
            private set => this.SetProperty(ref this.isExternalBayManualMovementTowardMachine, value);
        }

        public bool IsExternalBayManualMovementTowardOperator
        {
            get => this.isExternalBayManualMovementTowardOperator;
            private set => this.SetProperty(ref this.isExternalBayManualMovementTowardOperator, value);
        }

        public bool IsLightActive
        {
            get => this.isLightActive;
            private set => this.SetProperty(ref this.isLightActive, value);
        }

        public bool IsMovingElevatorBackwards
        {
            get => this.isMovingElevatorBackwards;
            private set => this.SetProperty(ref this.isMovingElevatorBackwards, value);
        }

        public bool IsMovingElevatorDown
        {
            get => this.isMovingElevatorDown;
            private set => this.SetProperty(ref this.isMovingElevatorDown, value);
        }

        public bool IsMovingElevatorForwards
        {
            get => this.isMovingElevatorForwards;
            private set => this.SetProperty(ref this.isMovingElevatorForwards, value);
        }

        public bool IsMovingElevatorUp
        {
            get => this.isMovingElevatorUp;
            private set => this.SetProperty(ref this.isMovingElevatorUp, value);
        }

        public bool IsPolicyBypassed
        {
            get => this.isPolicyBypassed;
            set
            {
                this.SetProperty(ref this.isPolicyBypassed, value);
                this.OnManualRaiseCanExecuteChanged();
            }
        }

        public ICommand IsPolicyBypassedCommand =>
            this.isPolicyBypassedCommand
            ??
            (this.isPolicyBypassedCommand = new DelegateCommand(() =>
            {
                if (this.IsPolicyBypassed == true)
                {
                    this.IsPolicyBypassed = false;
                    this.RaisePropertyChanged(nameof(this.IsPolicyBypassed));
                }
                else
                {
                    this.IsPolicyBypassed = true;
                    this.RaisePropertyChanged(nameof(this.IsPolicyBypassed));
                }
            }
            ));

        public bool IsShutterMovingDown
        {
            get => this.isShutterMovingDown;
            private set => this.SetProperty(ref this.isShutterMovingDown, value);
        }

        public bool IsShutterMovingUp
        {
            get => this.isShutterMovingUp;
            private set => this.SetProperty(ref this.isShutterMovingUp, value);
        }

        public ICommand LightCommand =>
            this.lightCommand
            ??
            (this.lightCommand = new DelegateCommand(
                async () => await this.LightAsync(),
                () => !this.IsMoving &&
                      (this.HealthProbeService.HealthMasStatus == Services.HealthStatus.Healthy || this.HealthProbeService.HealthMasStatus == Services.HealthStatus.Degraded)));

        public string LightIcon
        {
            get => this.lightIcon;
            private set => this.SetProperty(ref this.lightIcon, value);
        }

        public ICommand MoveCarouselCloseCommand =>
            this.moveCarouselCloseCommand
            ??
            (this.moveCarouselCloseCommand = new DelegateCommand(async () => await this.CloseCarouselAsync()));

        public ICommand MoveCarouselOpenCommand =>
            this.moveCarouselOpenCommand
            ??
            (this.moveCarouselOpenCommand = new DelegateCommand(async () => await this.OpenCarouselAsync()));

        public ICommand MoveElevatorBackwardsCommand =>
            this.moveElevatorBackwardsCommand
            ??
            (this.moveElevatorBackwardsCommand = new DelegateCommand(async () => await this.MoveElevatorBackwardsAsync()));

        public ICommand MoveElevatorDownCommand =>
            this.moveElevatorDownCommand
            ??
            (this.moveElevatorDownCommand = new DelegateCommand(async () => await this.MoveElevatorDownAsync()));

        public ICommand MoveElevatorForwardsCommand =>
            this.moveElevatorForwardsCommand
            ??
            (this.moveElevatorForwardsCommand = new DelegateCommand(async () => await this.MoveElevatorForwardsAsync()));

        public ICommand MoveElevatorUpCommand =>
            this.moveElevatorUpCommand
            ??
            (this.moveElevatorUpCommand = new DelegateCommand(async () => await this.MoveElevatorUpAsync()));

        public ICommand MoveExternalBayTowardMachineManualCommand =>
            this.moveExternalBayTowardMachineManualCommand
            ??
            (this.moveExternalBayTowardMachineManualCommand = new DelegateCommand(async () => await this.MoveManualExternalBayTowardMachineAsync()));

        public ICommand MoveExternalBayTowardOperatorManualCommand =>
            this.moveExternalBayTowardOperatorManualCommand
            ??
            (this.moveExternalBayTowardOperatorManualCommand = new DelegateCommand(async () => await this.MoveManualExternalBayTowardOperatorAsync()));

        public ICommand MoveToCellHeightCommand =>
           this.moveToCellHeightCommand
           ??
           (this.moveToCellHeightCommand = new DelegateCommand(
               async () => await this.MoveToCellHeightAsync(),
               this.CanMoveToCellHeight));

        public ICommand MoveToHeightCommand =>
            this.moveToHeightCommand
            ??
            (this.moveToHeightCommand = new DelegateCommand(
               async () => await this.MoveToHeightAsync(),
               this.CanMoveToHeight));

        public ICommand ShutterMoveDownCommand =>
            this.shutterMoveDownCommand
            ??
            (this.shutterMoveDownCommand = new DelegateCommand(async () => await this.ShutterMoveDownAsync()));

        public ICommand ShutterMoveUpCommand =>
            this.shutterMoveUpCommand
            ??
            (this.shutterMoveUpCommand = new DelegateCommand(async () => await this.ShutterMoveUpAsync()));

        #endregion

        #region Methods

        public async Task CloseCarouselAsync()
        {
            this.lastActiveCommand = "CloseCarousel";
            await this.StartMovementAsync(VerticalMovementDirection.Down);
        }

        public async Task LightAsync()
        {
            try
            {
                await this.machineBaysWebService.SetLightAsync(!this.IsLightActive);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public async Task MoveElevatorBackwardsAsync()
        {
            this.lastActiveCommand = "MoveElevatorBackOrForwards";
            await this.StartHorizontalMovementAsync(HorizontalMovementDirection.Backwards);
        }

        public async Task MoveElevatorDownAsync()
        {
            this.lastActiveCommand = "MoveElevatorDown";
            await this.StartVerticalMovementAsync(VerticalMovementDirection.Down);
        }

        public async Task MoveElevatorForwardsAsync()
        {
            this.lastActiveCommand = "MoveElevatorBackOrForwards";
            await this.StartHorizontalMovementAsync(HorizontalMovementDirection.Forwards);
        }

        public async Task MoveElevatorUpAsync()
        {
            this.lastActiveCommand = "MoveElevatorUp";
            await this.StartVerticalMovementAsync(VerticalMovementDirection.Up);
        }

        public async Task MoveManualExternalBayTowardMachineAsync()
        {
            this.lastActiveCommand = "MoveManualExternalBayTowardMachine";
            await this.StartExternalBayMovementAsync(ExternalBayMovementDirection.TowardMachine);
        }

        public async Task MoveManualExternalBayTowardOperatorAsync()
        {
            this.lastActiveCommand = "MoveManualExternalBayTowardOperator";
            await this.StartExternalBayMovementAsync(ExternalBayMovementDirection.TowardOperator);
        }

        public async Task OpenCarouselAsync()
        {
            this.lastActiveCommand = "MoveOpenCarousel";
            await this.StartMovementAsync(VerticalMovementDirection.Up);
        }

        public async Task ShutterMoveDownAsync()
        {
            this.lastActiveCommand = "ShutterMoveDown";
            await this.ManualShutterStartMovementAsync(ShutterMovementDirection.Down);
        }

        public async Task ShutterMoveUpAsync()
        {
            this.lastActiveCommand = "ShutterMoveUp";
            await this.ManualShutterStartMovementAsync(ShutterMovementDirection.Up);
        }

        private bool CanMoveToCellHeight()
        {
            return (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay || !this.HasShutter) &&
                   this.CanBaseExecute() &&
                   this.SelectedCell != null &&
                   this.moveToCellPolicy?.IsAllowed == true;
        }

        private bool CanMoveToHeight()
        {
            return (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay || !this.HasShutter) &&
                   this.CanBaseExecute() &&
                   this.InputHeight.HasValue &&
                   this.moveToHeightPolicy?.IsAllowed == true &&
                   Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.GetValueOrDefault()) != Convert.ToInt32(this.InputHeight.GetValueOrDefault());
        }

        private void CloseOperation()
        {
            this.StopMoving();
            this.isManualMovementCompleted = true;
            this.isPolicyBypassed = false;
        }

        private void InputCellIdPropertyChanged()
        {
            if (this.Cells is null)
            {
                return;
            }

            // HACK: 2
            this.selectedCell = this.inputCellId is null
                ? null
                : this.Cells.SingleOrDefault(c => c.Id == this.inputCellId);

            if (this.selectedCell != null)
            {
                this.inputHeight = this.SelectedCell.Position;

                this.InputLoadingUnitId = this.LoadingUnits.SingleOrDefault(l => l.CellId == this.selectedCell.Id)?.Id;
            }

            if (this.selectedLoadingUnit?.CellId is null)
            {
                this.loadingUnitInCell = null;
            }
            else
            {
                this.loadingUnitInCell = this.selectedLoadingUnit;
            }

            this.RaisePropertyChanged(nameof(this.InputHeight));
            this.RaisePropertyChanged(nameof(this.SelectedCell));
            this.RaisePropertyChanged(nameof(this.LoadingUnitInCell));
            this.RaisePropertyChanged(nameof(this.InputCellId));
            this.RaiseCanExecuteChanged();
        }

        private async Task ManualShutterStartMovementAsync(ShutterMovementDirection direction)
        {
            if (this.IsShutterMovingUp || this.IsShutterMovingDown)
            {
                return;
            }

            try
            {
                await this.shuttersWebService.MoveAsync(direction);
                if (direction == ShutterMovementDirection.Down)
                {
                    this.IsShutterMovingDown = true;
                }
                else
                {
                    this.IsShutterMovingUp = true;
                }

                this.IsShutterMoving = true;
            }
            catch (Exception ex)
            {
                this.CloseOperation();

                this.ShowNotification(ex);
            }
        }

        private async Task MoveToCellHeightAsync()
        {
            this.lastActiveCommand = "MoveToCellHeight";
            try
            {
                this.IsWaitingForResponse = true;

                Debug.Assert(
                    this.SelectedCell != null,
                    "The selected cell should be specified.");

                await this.machineElevatorWebService.MoveToCellAsync(
                    this.SelectedCell.Id,
                    performWeighting: this.isUseWeightControl,
                    computeElongation: true);

                this.IsElevatorMovingToCell = true;
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

        private async Task MoveToHeightAsync()
        {
            this.lastActiveCommand = "MoveToHeight";
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorWebService.MoveToVerticalPositionAsync(
                    this.InputHeight.Value,
                    this.isUseWeightControl);

                this.IsElevatorMovingToHeight = true;
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

        private void OnManualMachinePowerChanged()
        {
            this.RaiseCanExecuteChanged();

            if (!this.IsEnabled)
            {
                this.StopMoving();
            }
        }

        private void OnManualPositioningOperationChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            if (!this.IsMovementsManual)
            {
                return;
            }

            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.isManualMovementCompleted = false;
                    this.IsElevatorMoving = true;
                    break;

                case MessageStatus.OperationError:
                    this.CloseOperation();

                    break;

                case MessageStatus.OperationStop:
                case MessageStatus.OperationEnd:
                    this.CloseOperation();

                    break;
            }
        }

        private void OnManualRaiseCanExecuteChanged()
        {
            this.RaisePropertyChanged(nameof(this.InputHeight));

            this.CanInputCellId =
                this.CanBaseExecute()
                &&
                this.Cells != null;

            this.CanInputHeight = this.CanBaseExecute();

            var IsCorrectManual = false;

            switch (this.MachineService.BayNumber)
            {
                case MAS.AutomationService.Contracts.BayNumber.BayOne:
                default:
                    IsCorrectManual = this.MachineModeService.MachineMode == MAS.AutomationService.Contracts.MachineMode.Manual;
                    break;

                case MAS.AutomationService.Contracts.BayNumber.BayTwo:
                    IsCorrectManual = this.MachineModeService.MachineMode == MAS.AutomationService.Contracts.MachineMode.Manual2;
                    break;

                case MAS.AutomationService.Contracts.BayNumber.BayThree:
                    IsCorrectManual = this.MachineModeService.MachineMode == MAS.AutomationService.Contracts.MachineMode.Manual3;
                    break;
            }

            this.CanShutterMoveUpCommand = !this.IsShutterMovingDown && !(this.SensorsService?.ShutterSensors?.Open ?? false) &&
                                           !this.IsMovingElevatorBackwards && !this.IsMovingElevatorForwards && !this.IsMovingElevatorUp && !this.IsMovingElevatorDown &&
                                             ((this.SensorsService?.IsZeroChain ?? false) || this.SensorsService.IsLoadingUnitOnElevator) &&
                                           !this.IsCarouselOpening && !this.IsCarouselOpening &&
                                           !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight &&
                                           this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                                            IsCorrectManual;

            this.CanShutterMoveUpCommand = this.CanShutterMoveUpCommand || this.isPolicyBypassed;

            this.CanShutterMoveDownCommand = !this.IsShutterMovingUp && !(this.SensorsService?.ShutterSensors?.Closed ?? false) &&
                                             !this.IsMovingElevatorBackwards && !this.IsMovingElevatorForwards && !this.IsMovingElevatorUp && !this.IsMovingElevatorDown &&
                                             ((this.SensorsService?.IsZeroChain ?? false) || this.SensorsService.IsLoadingUnitOnElevator) &&
                                             !this.IsCarouselOpening && !this.IsCarouselOpening &&
                                             !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight &&
                                           this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                                       IsCorrectManual;

            this.CanShutterMoveDownCommand = this.CanShutterMoveDownCommand || this.isPolicyBypassed;

            this.CanMoveElevatorBackwards = !this.IsMovingElevatorForwards && !this.IsMovingElevatorUp && !this.IsMovingElevatorDown &&
                                           !this.IsCarouselOpening && !this.IsCarouselOpening &&
                                           !this.IsShutterMovingDown && !this.IsShutterMovingUp &&
                                           !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight &&
                                           this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                                            IsCorrectManual;

            this.CanMoveElevatorForwards = !this.IsMovingElevatorBackwards && !this.IsMovingElevatorUp && !this.IsMovingElevatorDown &&
                                           !this.IsCarouselOpening && !this.IsCarouselOpening &&
                                           !this.IsShutterMovingDown && !this.IsShutterMovingUp &&
                                           !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight &&
                                           this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                                           IsCorrectManual;

            this.CanMoveElevatorBackwards = this.CanMoveElevatorBackwards || this.isPolicyBypassed;
            this.CanMoveElevatorForwards = this.CanMoveElevatorForwards || this.isPolicyBypassed;

            this.CanMoveElevatorUp = (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay || !this.HasShutter) &&
                                     !this.IsMovingElevatorDown && !this.isMovingElevatorForwards && !this.IsMovingElevatorBackwards &&
                                     ((this.SensorsService?.IsZeroChain ?? false) || this.SensorsService.IsLoadingUnitOnElevator) &&
                                     !this.IsCarouselOpening && !this.IsCarouselOpening &&
                                     !this.IsShutterMovingDown && !this.IsShutterMovingUp &&
                                     !(this.SensorsService.IsExtraVertical && !this.SensorsService.IsZeroVertical) && // overrun
                                     !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight &&
                                     this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                                     IsCorrectManual;

            this.CanMoveElevatorUp = this.CanMoveElevatorUp || this.isPolicyBypassed;

            this.CanMoveElevatorDown = (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay || !this.HasShutter) &&
                                       !this.IsMovingElevatorUp && !this.isMovingElevatorForwards && !this.IsMovingElevatorBackwards &&
                                       ((this.SensorsService?.IsZeroChain ?? false) || this.SensorsService.IsLoadingUnitOnElevator) &&
                                       !this.IsCarouselOpening && !this.IsCarouselOpening &&
                                       !this.IsShutterMovingDown && !this.IsShutterMovingUp &&
                                       !(this.SensorsService.IsExtraVertical && this.SensorsService.IsZeroVertical) && // underrun
                                       !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight &&
                                       this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                                      IsCorrectManual;

            this.CanMoveElevatorDown = this.CanMoveElevatorDown || this.isPolicyBypassed;

            this.CanMoveCarouselCloseCommand = !this.IsCarouselOpening && this.moveCarouselDownPolicy?.IsAllowed == true &&
                                               !this.IsMovingElevatorBackwards && !this.IsMovingElevatorForwards && !this.IsMovingElevatorUp && !this.IsMovingElevatorDown &&
                                               !this.IsShutterMovingDown && !this.IsShutterMovingUp &&
                                               !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight &&
                                           this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                                       IsCorrectManual;

            this.CanMoveCarouselCloseCommand = this.CanMoveCarouselCloseCommand || this.isPolicyBypassed;

            this.CanMoveCarouselOpenCommand = !this.IsCarouselClosing && this.moveCarouselUpPolicy?.IsAllowed == true &&
                                              !this.IsMovingElevatorBackwards && !this.IsMovingElevatorForwards && !this.IsMovingElevatorUp && !this.IsMovingElevatorDown &&
                                              !this.IsShutterMovingDown && !this.IsShutterMovingUp &&
                                              !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight &&
                                           this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                                       IsCorrectManual;

            this.CanMoveCarouselOpenCommand = this.CanMoveCarouselOpenCommand || this.isPolicyBypassed;

            this.CanMoveManualExternalBayTowardOperatorCommand = !this.IsExternalBayManualMovementTowardMachine &&
                                                                 !this.IsMovingElevatorBackwards && !this.IsMovingElevatorForwards && !this.IsMovingElevatorUp && !this.IsMovingElevatorDown &&
                                                                 !this.IsShutterMovingDown && !this.IsShutterMovingUp &&
                                                                 !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight &&
                                           this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                                       IsCorrectManual;

            this.CanMoveManualExternalBayTowardOperatorCommand = this.CanMoveManualExternalBayTowardOperatorCommand || this.isPolicyBypassed;

            this.CanMoveManualExternalBayTowardMachineCommand = !this.IsExternalBayManualMovementTowardOperator &&
                                                                !this.IsMovingElevatorBackwards && !this.IsMovingElevatorForwards && !this.IsMovingElevatorUp && !this.IsMovingElevatorDown &&
                                                                !this.IsShutterMovingDown && !this.IsShutterMovingUp &&
                                                                !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight &&
                                           this.MachineModeService?.MachinePower == MachinePowerState.Powered &&
                                       IsCorrectManual;

            this.CanMoveManualExternalBayTowardMachineCommand = this.CanMoveManualExternalBayTowardMachineCommand || this.isPolicyBypassed;
        }

        private void OnManualShutterPositionChanged(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            if (!this.IsMovementsManual)
            {
                return;
            }

            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.isManualMovementCompleted = false;
                    this.IsShutterMoving = true;
                    break;

                case MessageStatus.OperationError:
                    this.CloseOperation();
                    break;

                case MessageStatus.OperationStop:
                case MessageStatus.OperationEnd:
                    this.CloseOperation();
                    break;
            }
        }

        private async Task StartExternalBayMovementAsync(ExternalBayMovementDirection direction)
        {
            if (this.IsExternalBayManualMovementTowardMachine ||
                this.IsExternalBayManualMovementTowardOperator)
            {
                return;
            }

            try
            {
                await this.machineExternalBayWebService.MoveManualAsync(direction);

                this.IsExternalBayManualMovementTowardMachine = direction is ExternalBayMovementDirection.TowardMachine;
                this.IsExternalBayManualMovementTowardOperator = direction is ExternalBayMovementDirection.TowardOperator;

                this.IsExternalBayMoving = true;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.CloseOperation();

                this.ShowNotification(ex);
            }
        }

        private async Task StartHorizontalMovementAsync(HorizontalMovementDirection direction)
        {
            if (this.IsMovingElevatorForwards || this.IsMovingElevatorBackwards)
            {
                return;
            }

            try
            {
                await this.machineElevatorWebService.MoveHorizontalManualAsync(direction);
                if (direction == HorizontalMovementDirection.Backwards)
                {
                    this.IsMovingElevatorBackwards = true;
                }
                else
                {
                    this.IsMovingElevatorForwards = true;
                }

                this.IsElevatorMoving = true;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.CloseOperation();

                this.ShowNotification(ex);
            }
        }

        private async Task StartMovementAsync(VerticalMovementDirection direction)
        {
            if (this.IsCarouselClosing || this.IsCarouselOpening)
            {
                return;
            }

            try
            {
                await this.machineCarouselWebService.MoveManualAsync(direction);

                this.IsCarouselClosing = direction is VerticalMovementDirection.Down;
                this.IsCarouselOpening = direction is VerticalMovementDirection.Up;

                this.IsCarouselMoving = true;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.CloseOperation();

                this.ShowNotification(ex);
            }
        }

        private async Task StartVerticalMovementAsync(VerticalMovementDirection direction)
        {
            if (this.IsMovingElevatorUp || this.IsMovingElevatorDown)
            {
                return;
            }

            try
            {
                await this.machineElevatorWebService.MoveVerticalManualAsync(direction);
                if (direction == VerticalMovementDirection.Down)
                {
                    this.IsMovingElevatorDown = true;
                }
                else
                {
                    this.IsMovingElevatorUp = true;
                }

                this.IsElevatorMoving = true;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.CloseOperation();

                this.ShowNotification(ex);
            }
        }

        private void StopMoving()
        {
            this.IsShutterMovingUp = false;
            this.IsShutterMovingDown = false;
            this.IsShutterMoving = false;
            this.IsMovingElevatorUp = false;
            this.IsMovingElevatorDown = false;
            this.IsMovingElevatorForwards = false;
            this.IsMovingElevatorBackwards = false;
            this.IsElevatorMoving = false;
            this.IsCarouselClosing = false;
            this.IsCarouselOpening = false;
            this.IsCarouselMoving = false;
            this.IsElevatorMovingToCell = false;
            this.IsElevatorMovingToBay = false;
            this.IsElevatorMovingToHeight = false;
            this.IsElevatorMovingToLoadingUnit = false;
            this.IsTuningBay = false;
            this.IsTuningChain = false;
            this.IsTuningExtBay = false;
            this.IsBusyLoadingFromBay = false;
            this.IsVerticalCalibration = false;
            this.IsBusyLoadingFromCell = false;
            this.IsBusyUnloadingToBay = false;
            this.IsBusyUnloadingToCell = false;
            this.IsExecutingProcedure = false;
            this.IsExternalBayManualMovementTowardOperator = false;
            this.IsExternalBayManualMovementTowardMachine = false;
            this.IsExternalBayMoving = false;

            this.IsPolicyBypassed = false;
            this.RaisePropertyChanged(nameof(this.IsPolicyBypassed));
        }

        private void WriteInfo(Axis? axisMovement)
        {
            if (axisMovement.HasValue && axisMovement == Axis.Vertical)
            {
                this.ShowNotification(Localized.Get("InstallationApp.VerticalAxisMovementInProgress"), Services.Models.NotificationSeverity.Info);
            }
            else if (axisMovement.HasValue && axisMovement == Axis.Horizontal)
            {
                this.ShowNotification(Localized.Get("InstallationApp.HorizontalAxisMovementInProgress"), Services.Models.NotificationSeverity.Info);
            }
        }

        #endregion
    }
}

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
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

        private bool canShutterMoveDownCommand;

        private bool canShutterMoveUpCommand;

        private bool isCarouselClosing;

        private bool isCarouselOpening;

        private bool isCompleted;

        private bool isElevatorMoving;

        private bool isMovingElevatorBackwards;

        private bool isMovingElevatorDown;

        private bool isMovingElevatorForwards;

        private bool isMovingElevatorUp;

        private bool isShutterMovingDown;

        private bool isShutterMovingUp;

        private DelegateCommand moveCarouselCloseCommand;

        private DelegateCommand moveCarouselOpenCommand;

        private DelegateCommand moveElevatorBackwardsCommand;

        private DelegateCommand moveElevatorDownCommand;

        private DelegateCommand moveElevatorForwardsCommand;

        private DelegateCommand moveElevatorUpCommand;

        private DelegateCommand moveToCellHeightCommand;

        private DelegateCommand moveToHeightCommand;

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

        public bool IsCarouselClosing
        {
            get => this.isCarouselClosing;
            private set => this.SetProperty(ref this.isCarouselClosing, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCarouselOpening
        {
            get => this.isCarouselOpening;
            private set => this.SetProperty(ref this.isCarouselOpening, value, this.RaiseCanExecuteChanged);
        }

        public bool IsElevatorMoving
        {
            get => this.isElevatorMoving;
            private set => this.SetProperty(ref this.isElevatorMoving, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMovingElevatorBackwards
        {
            get => this.isMovingElevatorBackwards;
            private set => this.SetProperty(ref this.isMovingElevatorBackwards, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMovingElevatorDown
        {
            get => this.isMovingElevatorDown;
            private set => this.SetProperty(ref this.isMovingElevatorDown, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMovingElevatorForwards
        {
            get => this.isMovingElevatorForwards;
            private set => this.SetProperty(ref this.isMovingElevatorForwards, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMovingElevatorUp
        {
            get => this.isMovingElevatorUp;
            private set => this.SetProperty(ref this.isMovingElevatorUp, value, this.RaiseCanExecuteChanged);
        }

        public bool IsShutterMovingDown
        {
            get => this.isShutterMovingDown;
            private set
            {
                if (this.SetProperty(ref this.isShutterMovingDown, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsShutterMovingUp
        {
            get => this.isShutterMovingUp;
            private set
            {
                if (this.SetProperty(ref this.isShutterMovingUp, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
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
            await this.StartMovementAsync(VerticalMovementDirection.Down);
        }

        public async Task MoveElevatorBackwardsAsync()
        {
            await this.StartHorizontalMovementAsync(HorizontalMovementDirection.Backwards);
        }

        public async Task MoveElevatorDownAsync()
        {
            await this.StartVerticalMovementAsync(VerticalMovementDirection.Down);
        }

        public async Task MoveElevatorForwardsAsync()
        {
            await this.StartHorizontalMovementAsync(HorizontalMovementDirection.Forwards);
        }

        public async Task MoveElevatorUpAsync()
        {
            await this.StartVerticalMovementAsync(VerticalMovementDirection.Up);
        }

        public async Task OnManualAppearedAsync()
        {
            this.isCompleted = false;
        }

        public async Task OpenCarouselAsync()
        {
            await this.StartMovementAsync(VerticalMovementDirection.Up);
        }

        public async Task ShutterMoveDownAsync()
        {
            await this.ManualShutterStartMovementAsync(ShutterMovementDirection.Down);
        }

        public async Task ShutterMoveUpAsync()
        {
            await this.ManualShutterStartMovementAsync(ShutterMovementDirection.Up);
        }

        protected void OnManualErrorStatusChanged()
        {
            if (!(this.MachineError is null))
            {
                this.StopMoving();
            }
        }

        protected void OnManualMachinePowerChanged()
        {
            this.RaiseCanExecuteChanged();

            if (!this.IsEnabled)
            {
                this.StopMoving();
            }
        }

        protected void OnManualRaiseCanExecuteChanged()
        {
            this.CanInputCellId =
                !this.IsKeyboardOpened
                &&
                this.cells != null
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsMoving
                &&
                !this.IsWaitingForResponse;

            this.CanInputHeight =
                !this.IsKeyboardOpened
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsMoving
                &&
                !this.IsWaitingForResponse;

            this.CanShutterMoveUpCommand = !this.IsShutterMovingDown && !(this.SensorsService?.ShutterSensors?.Open ?? false) &&
                                           !this.IsMovingElevatorBackwards && !this.IsMovingElevatorForwards && !this.IsMovingElevatorUp && !this.IsMovingElevatorDown &&
                                           !this.IsCarouselOpening && !this.IsCarouselOpening &&
                                           !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight;

            this.CanShutterMoveDownCommand = !this.IsShutterMovingUp && !(this.SensorsService?.ShutterSensors?.Closed ?? false) &&
                                             !this.IsMovingElevatorBackwards && !this.IsMovingElevatorForwards && !this.IsMovingElevatorUp && !this.IsMovingElevatorDown &&
                                             !this.IsCarouselOpening && !this.IsCarouselOpening &&
                                             !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight;

            this.CanMoveElevatorBackwards = !this.IsMovingElevatorForwards && !this.IsMovingElevatorUp && !this.IsMovingElevatorDown &&
                                             !this.IsCarouselOpening && !this.IsCarouselOpening &&
                                             !this.IsShutterMovingDown && !this.IsShutterMovingUp &&
                                             !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight;
            this.CanMoveElevatorForwards = !this.IsMovingElevatorBackwards && !this.IsMovingElevatorUp && !this.IsMovingElevatorDown &&
                                             !this.IsCarouselOpening && !this.IsCarouselOpening &&
                                             !this.IsShutterMovingDown && !this.IsShutterMovingUp &&
                                             !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight;

            this.CanMoveElevatorUp = !this.IsMovingElevatorDown && !this.isMovingElevatorForwards && !this.IsMovingElevatorBackwards && (this.SensorsService?.IsZeroChain ?? false) &&
                                             !this.IsCarouselOpening && !this.IsCarouselOpening &&
                                             !this.IsShutterMovingDown && !this.IsShutterMovingUp &&
                                             !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight;
            this.CanMoveElevatorDown = !this.IsMovingElevatorUp && !this.isMovingElevatorForwards && !this.IsMovingElevatorBackwards && (this.SensorsService?.IsZeroChain ?? false) &&
                                             !this.IsCarouselOpening && !this.IsCarouselOpening &&
                                             !this.IsShutterMovingDown && !this.IsShutterMovingUp &&
                                             !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight;

            this.CanMoveCarouselCloseCommand = !this.IsCarouselOpening && this.moveCarouselDownPolicy?.IsAllowed == true &&
                                             !this.IsMovingElevatorBackwards && !this.IsMovingElevatorForwards && !this.IsMovingElevatorUp && !this.IsMovingElevatorDown &&
                                             !this.IsShutterMovingDown && !this.IsShutterMovingUp &&
                                             !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight;
            this.CanMoveCarouselOpenCommand = !this.IsCarouselClosing && this.moveCarouselUpPolicy?.IsAllowed == true &&
                                             !this.IsMovingElevatorBackwards && !this.IsMovingElevatorForwards && !this.IsMovingElevatorUp && !this.IsMovingElevatorDown &&
                                             !this.IsShutterMovingDown && !this.IsShutterMovingUp &&
                                             !this.IsElevatorMovingToCell && !this.IsElevatorMovingToHeight;

            this.moveToCellHeightCommand?.RaiseCanExecuteChanged();
            this.moveToHeightCommand?.RaiseCanExecuteChanged();
        }

        private bool CanMoveToCellHeight()
        {
            return
                !this.IsKeyboardOpened
                &&
                this.SelectedCell != null
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                this.moveToCellPolicy?.IsAllowed == true;
        }

        private bool CanMoveToHeight()
        {
            return
                !this.IsKeyboardOpened
                &&
                this.InputHeight != null
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving;
        }

        private void CloseOperation()
        {
            this.StopMoving();
            this.isCompleted = true;
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
            catch (System.Exception ex)
            {
                this.CloseOperation();

                this.ShowNotification(ex);
            }
        }

        private async Task MoveToCellHeightAsync()
        {
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
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorWebService.MoveToVerticalPositionAsync(
                    this.InputHeight.Value,
                    this.isUseWeightControl,
                    false);

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

        private async Task OnManualPositioningOperationChangedAsync(NotificationMessageUI<PositioningMessageData> message)
        {
            if (!this.IsMovementsManual)
            {
                return;
            }

            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.isCompleted = false;
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

        private void OnManualShutterPositionChanged(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            if (!this.IsMovementsManual)
            {
                return;
            }

            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.isCompleted = false;
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
            catch (System.Exception ex)
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
            catch (System.Exception ex)
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
            catch (System.Exception ex)
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
        }

        private void WriteInfo(Axis? axisMovement)
        {
            if (axisMovement.HasValue && axisMovement == Axis.Vertical)
            {
                this.ShowNotification("Movimento asse verticale in corso...", Services.Models.NotificationSeverity.Info);
            }
            else if (axisMovement.HasValue && axisMovement == Axis.Horizontal)
            {
                this.ShowNotification("Movimento asse orizzontale in corso...", Services.Models.NotificationSeverity.Info);
            }
        }

        #endregion
    }
}

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;
using Prism.Regions;
using ShutterMovementDirection = Ferretto.VW.MAS.AutomationService.Contracts.ShutterMovementDirection;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class MovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private bool canExecuteShutterMoveDownCommand;

        private bool canExecuteShutterMoveUpCommand;

        private bool canInputCellId;

        private bool canInputHeight;

        private bool canInputLoadingUnitId;

        private bool isCompleted;

        private bool isShutterMovingDown;

        private bool isShutterMovingUp;

        private DelegateCommand moveToCellHeightCommand;

        private DelegateCommand moveToHeightCommand;

        private DelegateCommand shutterMoveDownCommand;

        private DelegateCommand shutterMoveUpCommand;

        #endregion

        #region Properties

        public bool CanExecuteShutterMoveDownCommand
        {
            get => this.canExecuteShutterMoveDownCommand;
            private set => this.SetProperty(ref this.canExecuteShutterMoveDownCommand, value);
        }

        public bool CanExecuteShutterMoveUpCommand
        {
            get => this.canExecuteShutterMoveUpCommand;
            private set => this.SetProperty(ref this.canExecuteShutterMoveUpCommand, value);
        }

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

        public async Task OnManualAppearedAsync()
        {
            this.isCompleted = false;
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
                !this.IsMoving
                &&
                !this.IsWaitingForResponse;

            this.CanInputHeight =
                !this.IsKeyboardOpened
                &&
                !this.IsMoving
                &&
                !this.IsWaitingForResponse;

            this.CanExecuteShutterMoveUpCommand = !this.IsShutterMovingDown && !(this.SensorsService?.ShutterSensors?.Open ?? false);
            this.CanExecuteShutterMoveDownCommand = !this.IsShutterMovingUp && !(this.SensorsService?.ShutterSensors?.Closed ?? false);
        }

        private bool CanMoveToCellHeight()
        {
            return
               !this.IsKeyboardOpened
                &&
                this.SelectedCell != null
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
                !this.IsWaitingForResponse
                &&
                !this.IsMoving;
        }

        private void CloseOperation()
        {
            this.StopMoving();
            this.IsShutterMoving = false;
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
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingToCell = false;

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
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingToHeight = false;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
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
                    this.ShowNotification("Movimento serranda in corso...", Services.Models.NotificationSeverity.Info);
                    this.isCompleted = false;
                    this.IsShutterMoving = true;
                    break;

                case MessageStatus.OperationExecuting:
                    if (!this.isCompleted)
                    {
                        this.ShowNotification("Movimento serranda in corso...", Services.Models.NotificationSeverity.Info);
                    }

                    break;

                case MessageStatus.OperationError:
                    this.ShowNotification(message.Description, Services.Models.NotificationSeverity.Error);
                    this.CloseOperation();
                    break;

                case MessageStatus.OperationStop:
                case MessageStatus.OperationEnd:
                    this.ClearNotifications();
                    this.CloseOperation();
                    break;
            }
        }

        private void StopMoving()
        {
            this.IsShutterMovingUp = false;
            this.IsShutterMovingDown = false;
            this.IsShutterMoving = false;
        }

        #endregion
    }
}

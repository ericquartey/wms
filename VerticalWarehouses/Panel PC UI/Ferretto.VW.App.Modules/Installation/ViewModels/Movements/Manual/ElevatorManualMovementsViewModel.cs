using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;
using HorizontalMovementDirection = Ferretto.VW.MAS.AutomationService.Contracts.HorizontalMovementDirection;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class ElevatorManualMovementsViewModel : BaseManualMovementsViewModel
    {
        #region Fields

        private bool canExecuteMoveBackwardsCommand;

        private bool canExecuteMoveDownCommand;

        private bool canExecuteMoveForwardCommand;

        private bool canExecuteMoveUpCommand;

        private bool isCompleted;

        private bool isMovingBackwards;

        private bool isMovingDown;

        private bool isMovingForwards;

        private bool isMovingUp;

        private DelegateCommand moveBackwardsCommand;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveForwardsCommand;

        private DelegateCommand moveUpCommand;

        private SubscriptionToken positioningToken;

        #endregion

        #region Constructors

        public ElevatorManualMovementsViewModel(
            IMachineElevatorWebService elevatorWebService,
            IMachineSensorsWebService machineSensorsWebService,
            IHealthProbeService healthProbeService,
            IBayManager bayManager)
            : base(elevatorWebService,
                   machineSensorsWebService,
                   healthProbeService,
                   bayManager)
        {
        }

        #endregion

        #region Properties

        public bool CanMoveBackwards
        {
            get => this.canExecuteMoveBackwardsCommand;
            private set => this.SetProperty(ref this.canExecuteMoveBackwardsCommand, value);
        }

        public bool CanMoveDown
        {
            get => this.canExecuteMoveDownCommand;
            private set => this.SetProperty(ref this.canExecuteMoveDownCommand, value);
        }

        public bool CanMoveForwards
        {
            get => this.canExecuteMoveForwardCommand;
            private set => this.SetProperty(ref this.canExecuteMoveForwardCommand, value);
        }

        public bool CanMoveUp
        {
            get => this.canExecuteMoveUpCommand;
            private set => this.SetProperty(ref this.canExecuteMoveUpCommand, value);
        }

        public bool IsMovingBackwards
        {
            get => this.isMovingBackwards;
            private set => this.SetProperty(ref this.isMovingBackwards, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMovingDown
        {
            get => this.isMovingDown;
            private set => this.SetProperty(ref this.isMovingDown, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMovingForwards
        {
            get => this.isMovingForwards;
            private set => this.SetProperty(ref this.isMovingForwards, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMovingUp
        {
            get => this.isMovingUp;
            private set => this.SetProperty(ref this.isMovingUp, value, this.RaiseCanExecuteChanged);
        }

        public ICommand MoveBackwardsCommand =>
            this.moveBackwardsCommand
            ??
            (this.moveBackwardsCommand = new DelegateCommand(async () => await this.MoveBackwardsAsync()));

        public ICommand MoveDownCommand =>
            this.moveDownCommand
            ??
            (this.moveDownCommand = new DelegateCommand(async () => await this.MoveDownAsync()));

        public ICommand MoveForwardsCommand =>
            this.moveForwardsCommand
            ??
            (this.moveForwardsCommand = new DelegateCommand(async () => await this.MoveForwardsAsync()));

        public ICommand MoveUpCommand =>
            this.moveUpCommand
            ??
            (this.moveUpCommand = new DelegateCommand(async () => await this.MoveUpAsync()));

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.positioningToken?.Dispose();
            this.positioningToken = null;

            base.Disappear();
        }

        public async Task MoveBackwardsAsync()
        {
            this.DisableAllExceptThis();

            await this.StartHorizontalMovementAsync(HorizontalMovementDirection.Backwards);
        }

        public async Task MoveDownAsync()
        {
            this.DisableAllExceptThis();

            await this.StartVerticalMovementAsync(VerticalMovementDirection.Down);
        }

        public async Task MoveForwardsAsync()
        {
            this.DisableAllExceptThis();

            await this.StartHorizontalMovementAsync(HorizontalMovementDirection.Forwards);
        }

        public async Task MoveUpAsync()
        {
            this.DisableAllExceptThis();

            await this.StartVerticalMovementAsync(VerticalMovementDirection.Up);
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.positioningToken = this.positioningToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        this.OnElevatorPositionChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data?.AxisMovement == Axis.Vertical ||
                             m.Data?.AxisMovement == Axis.Horizontal);

            this.isCompleted = false;

            this.RaiseCanExecuteChanged();
        }

        protected override void OnErrorStatusChanged()
        {
            //if (!this.IsEnabled)
            if (!(this.MachineError is null))
            {
                this.StopMoving();
                this.IsStopping = false;
            }
        }

        protected override void OnMachinePowerChanged()
        {
            this.RaiseCanExecuteChanged();

            if (!this.IsEnabled)
            {
                this.StopMoving();
                this.IsStopping = false;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.CanMoveBackwards = !this.IsMovingForwards && !this.IsMovingUp && !this.IsMovingDown && !this.IsStopping;
            this.CanMoveForwards = !this.IsMovingBackwards && !this.IsMovingUp && !this.IsMovingDown && !this.IsStopping;
            this.CanMoveUp = !this.IsMovingDown && !this.isMovingForwards && !this.IsMovingBackwards && !this.IsStopping;
            this.CanMoveDown = !this.IsMovingUp && !this.isMovingForwards && !this.IsMovingBackwards && !this.IsStopping;
        }

        protected override async Task StopMovementAsync()
        {
            // In caso di fine operazione
            if (this.isCompleted)
            {
                return;
            }

            try
            {
                await this.MachineElevatorService.StopAsync();
                this.IsStopping = true;
            }
            catch (System.Exception ex)
            {
                this.CloseOperation();
                this.ShowNotification(ex);
            }
        }

        private void CloseOperation()
        {
            this.StopMoving();
            this.IsStopping = false;
            this.EnableAll();
            this.isCompleted = true;
        }

        private void OnElevatorPositionChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.WriteInfo(message.Data?.AxisMovement);
                    this.isCompleted = false;

                    break;

                case MessageStatus.OperationExecuting:
                    if (!this.isCompleted)
                    {
                        this.WriteInfo(message.Data?.AxisMovement);
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

        private async Task StartHorizontalMovementAsync(HorizontalMovementDirection direction)
        {
            if (this.IsMovingForwards || this.IsMovingBackwards)
            {
                return;
            }

            try
            {
                await this.MachineElevatorService.MoveHorizontalManualAsync(direction);
                if (direction == HorizontalMovementDirection.Backwards)
                {
                    this.IsMovingBackwards = true;
                }
                else
                {
                    this.IsMovingForwards = true;
                }
            }
            catch (System.Exception ex)
            {
                this.CloseOperation();

                this.ShowNotification(ex);
            }
        }

        private async Task StartVerticalMovementAsync(VerticalMovementDirection direction)
        {
            if (this.IsMovingUp || this.IsMovingDown)
            {
                return;
            }

            try
            {
                await this.MachineElevatorService.MoveVerticalManualAsync(direction);
                if (direction == VerticalMovementDirection.Down)
                {
                    this.IsMovingDown = true;
                }
                else
                {
                    this.IsMovingUp = true;
                }
            }
            catch (System.Exception ex)
            {
                this.CloseOperation();

                this.ShowNotification(ex);
            }
        }

        private void StopMoving()
        {
            this.IsMovingUp = false;
            this.IsMovingDown = false;
            this.IsMovingForwards = false;
            this.IsMovingBackwards = false;
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

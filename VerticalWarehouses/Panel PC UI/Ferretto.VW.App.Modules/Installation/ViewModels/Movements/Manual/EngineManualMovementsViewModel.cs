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
    internal sealed class EngineManualMovementsViewModel : BaseManualMovementsViewModel
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

        public EngineManualMovementsViewModel(
            IMachineElevatorWebService elevatorWebService,
            IBayManager bayManager)
            : base(elevatorWebService, bayManager)
        {
        }

        #endregion

        #region Properties

        public bool CanExecuteMoveBackwardsCommand
        {
            get => this.canExecuteMoveBackwardsCommand;
            private set => this.SetProperty(ref this.canExecuteMoveBackwardsCommand, value);
        }

        public bool CanExecuteMoveDownCommand
        {
            get => this.canExecuteMoveDownCommand;
            private set => this.SetProperty(ref this.canExecuteMoveDownCommand, value);
        }

        public bool CanExecuteMoveForwardsCommand
        {
            get => this.canExecuteMoveForwardCommand;
            private set => this.SetProperty(ref this.canExecuteMoveForwardCommand, value);
        }

        public bool CanExecuteMoveUpCommand
        {
            get => this.canExecuteMoveUpCommand;
            private set => this.SetProperty(ref this.canExecuteMoveUpCommand, value);
        }

        public bool IsMovingBackwards
        {
            get => this.isMovingBackwards;
            private set
            {
                if (this.SetProperty(ref this.isMovingBackwards, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsMovingDown
        {
            get => this.isMovingDown;
            private set
            {
                if (this.SetProperty(ref this.isMovingDown, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsMovingForwards
        {
            get => this.isMovingForwards;
            private set
            {
                if (this.SetProperty(ref this.isMovingForwards, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsMovingUp
        {
            get => this.isMovingUp;
            private set
            {
                if (this.SetProperty(ref this.isMovingUp, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
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
            base.Disappear();

            this.positioningToken?.Dispose();
            this.positioningToken = null;
        }

        public async Task MoveBackwardsAsync()
        {
            this.IsMovingBackwards = true;
            this.IsMovingForwards = false;
            this.IsMovingDown = false;
            this.IsMovingUp = false;

            this.DisableAllExceptThis();

            await this.StartMovementAsync(HorizontalMovementDirection.Backwards);
        }

        public async Task MoveDownAsync()
        {
            this.IsMovingDown = true;
            this.IsMovingUp = false;
            this.IsMovingBackwards = false;
            this.IsMovingForwards = false;

            this.DisableAllExceptThis();

            await this.StartMovementAsync(VerticalMovementDirection.Down);
        }

        public async Task MoveForwardsAsync()
        {
            this.IsMovingForwards = true;
            this.IsMovingBackwards = false;
            this.IsMovingDown = false;
            this.IsMovingUp = false;

            this.DisableAllExceptThis();

            await this.StartMovementAsync(HorizontalMovementDirection.Forwards);
        }

        public async Task MoveUpAsync()
        {
            this.IsMovingUp = true;
            this.IsMovingDown = false;
            this.IsMovingForwards = false;
            this.IsMovingBackwards = false;

            this.DisableAllExceptThis();

            await this.StartMovementAsync(VerticalMovementDirection.Up);
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
                        false);

            this.isCompleted = false;

            this.RaiseCanExecuteChanged();
        }

        protected override async Task OnMachineModeChangedAsync(MachineModeChangedEventArgs e)
        {
            await base.OnMachineModeChangedAsync(e);

            if (!this.IsEnabled)
            {
                this.StopMoving();
                this.IsStopping = false;
            }
        }

        protected override void OnMachinePowerChanged()
        {
            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.CanExecuteMoveBackwardsCommand = !this.IsMovingForwards && !this.IsMovingUp && !this.IsMovingDown && !this.IsStopping;
            this.CanExecuteMoveForwardsCommand = !this.IsMovingBackwards && !this.IsMovingUp && !this.IsMovingDown && !this.IsStopping;
            this.CanExecuteMoveUpCommand = !this.IsMovingDown && !this.isMovingForwards && !this.IsMovingBackwards && !this.IsStopping;
            this.CanExecuteMoveDownCommand = !this.IsMovingUp && !this.isMovingForwards && !this.IsMovingBackwards && !this.IsStopping;
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
                this.IsStopping = true;
                await this.MachineElevatorService.StopAsync();
            }
            catch (System.Exception ex)
            {
                this.CloseOperation();
                this.ShowNotification(ex);
            }
            finally
            {
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
            // metterlo nel filtro della subscription
            if (message.Data?.AxisMovement != Axis.Vertical && message.Data?.AxisMovement != Axis.Horizontal)
            {
                return;
            }

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

        private async Task StartMovementAsync(HorizontalMovementDirection direction)
        {
            try
            {
                await this.MachineElevatorService.MoveHorizontalManualAsync(direction);
            }
            catch (System.Exception ex)
            {
                this.CloseOperation();

                this.ShowNotification(ex);
            }
        }

        private async Task StartMovementAsync(VerticalMovementDirection direction)
        {
            try
            {
                await this.MachineElevatorService.MoveVerticalAsync(direction);
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
                this.ShowNotification("Movimento asse verticale in corso..", Services.Models.NotificationSeverity.Info);
            }
            else if (axisMovement.HasValue && axisMovement == Axis.Horizontal)
            {
                this.ShowNotification("Movimento asse orizzontale in corso..", Services.Models.NotificationSeverity.Info);
            }
        }

        #endregion
    }
}

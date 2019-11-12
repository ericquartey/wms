using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class EngineManualMovementsViewModel : BaseManualMovementsViewModel
    {
        #region Fields

        private bool canExecuteMoveBackwardsCommand;

        private bool canExecuteMoveDownCommand;

        private bool canExecuteMoveForwardCommand;

        private bool canExecuteMoveUpCommand;

        private bool isMovingBackwards;

        private bool isMovingDown;

        private bool isMovingForwards;

        private bool isMovingUp;

        private bool isStopping;

        private DelegateCommand moveBackwardsCommand;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveForwardsCommand;

        private DelegateCommand moveUpCommand;

        #endregion

        #region Constructors

        public EngineManualMovementsViewModel(
            IMachineElevatorWebService elevatorWebService,
            IBayManager bayManager)
            : base(elevatorWebService, bayManager)
        {
            this.RefreshCanExecuteCommands();
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
                    this.RefreshCanExecuteCommands();
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
                    this.RefreshCanExecuteCommands();
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
                    this.RefreshCanExecuteCommands();
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
                    this.RefreshCanExecuteCommands();
                }
            }
        }

        public bool IsStopping
        {
            get => this.isStopping;
            private set
            {
                if (this.SetProperty(ref this.isStopping, value))
                {
                    this.RefreshCanExecuteCommands();
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

        protected override async Task OnMachineModeChangedAsync(MachineModeChangedEventArgs e)
        {
            await base.OnMachineModeChangedAsync(e);

            if (!this.IsEnabled)
            {
                this.StopMoving();
            }
        }

        protected override void OnMachinePowerChanged()
        {
            this.RefreshCanExecuteCommands();
        }

        protected override async Task StopMovementAsync()
        {
            this.IsStopping = true;

            try
            {
                await this.MachineElevatorService.StopAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.StopMoving();
                this.IsStopping = false;
                this.EnableAll();
            }
        }

        private void RefreshCanExecuteCommands()
        {
            this.CanExecuteMoveBackwardsCommand = !this.IsMovingForwards && !this.IsMovingUp && !this.IsMovingDown && !this.IsStopping;
            this.CanExecuteMoveForwardsCommand = !this.IsMovingBackwards && !this.IsMovingUp && !this.IsMovingDown && !this.IsStopping;
            this.CanExecuteMoveUpCommand = !this.IsMovingDown && !this.isMovingForwards && !this.IsMovingBackwards && !this.IsStopping;
            this.CanExecuteMoveDownCommand = !this.IsMovingUp && !this.isMovingForwards && !this.IsMovingBackwards && !this.IsStopping;
        }

        private async Task StartMovementAsync(HorizontalMovementDirection direction)
        {
            try
            {
                await this.MachineElevatorService.MoveHorizontalManualAsync(direction);
            }
            catch (System.Exception ex)
            {
                this.StopMoving();

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
                this.StopMoving();

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

        #endregion
    }
}

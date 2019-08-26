using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class VerticalEngineManualMovementsViewModel : BaseManualMovementsViewModel
    {
        #region Fields

        private bool canExecuteMoveDownCommand;

        private bool canExecuteMoveUpCommand;

        private bool isMovingDown;

        private bool isMovingUp;

        private bool isStopping;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveUpCommand;

        #endregion

        #region Constructors

        public VerticalEngineManualMovementsViewModel(
            IMachineElevatorService machineElevatorService,
            IBayManager bayManager)
            : base(machineElevatorService, bayManager)
        {
            if (machineElevatorService is null)
            {
                throw new System.ArgumentNullException(nameof(machineElevatorService));
            }

            this.MachineElevatorService = machineElevatorService;

            this.RefreshCanExecuteCommands();
        }

        #endregion

        #region Properties

        public bool CanExecuteMoveDownCommand
        {
            get => this.canExecuteMoveDownCommand;
            private set => this.SetProperty(ref this.canExecuteMoveDownCommand, value);
        }

        public bool CanExecuteMoveUpCommand
        {
            get => this.canExecuteMoveUpCommand;
            private set => this.SetProperty(ref this.canExecuteMoveUpCommand, value);
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

        public IMachineElevatorService MachineElevatorService { get; }

        public ICommand MoveDownCommand =>
            this.moveDownCommand
            ??
            (this.moveDownCommand = new DelegateCommand(async () => await this.MoveDownAsync()));

        public ICommand MoveUpCommand =>
            this.moveUpCommand
            ??
            (this.moveUpCommand = new DelegateCommand(async () => await this.MoveUpAsync()));

        #endregion

        #region Methods

        public async Task MoveDownAsync()
        {
            this.IsMovingDown = true;
            this.IsMovingUp = false;

            await this.StartMovementAsync(VerticalMovementDirection.Down);
        }

        public async Task MoveUpAsync()
        {
            this.IsMovingUp = true;
            this.IsMovingDown = false;

            await this.StartMovementAsync(VerticalMovementDirection.Up);
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.IsBackNavigationAllowed = true;

            try
            {
                this.CurrentPosition = await this.MachineElevatorService.GetVerticalPositionAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected override async Task StopMovementAsync()
        {
            try
            {
                this.IsStopping = true;

                await this.MachineElevatorService.StopAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsMovingDown = false;
                this.IsMovingUp = false;
                this.IsStopping = false;
            }
        }

        private void RefreshCanExecuteCommands()
        {
            this.CanExecuteMoveUpCommand = !this.IsMovingDown && !this.IsStopping;
            this.CanExecuteMoveDownCommand = !this.IsMovingUp && !this.IsStopping;
        }

        private async Task StartMovementAsync(VerticalMovementDirection direction)
        {
            try
            {
                await this.MachineElevatorService.MoveVerticalAsync(direction);
            }
            catch (System.Exception ex)
            {
                this.IsMovingUp = false;
                this.IsMovingDown = false;

                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}

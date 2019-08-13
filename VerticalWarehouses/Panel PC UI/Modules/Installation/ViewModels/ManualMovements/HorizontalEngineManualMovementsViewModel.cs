using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class HorizontalAxisManualMovementsViewModel : BaseManualMovementsViewModel
    {
        #region Fields

        private bool canExecuteMoveBackwardsCommand;

        private bool canExecuteMoveForwardCommand;

        private bool isMovingBackwards;

        private bool isMovingForwards;

        private bool isStopping;

        private DelegateCommand moveBackwardsCommand;

        private DelegateCommand moveForwardsCommand;

        #endregion

        #region Constructors

        public HorizontalAxisManualMovementsViewModel(
            IMachineElevatorService machineElevatorService,
            IMachineHomingService homingService)
            : base(homingService)
        {
            if (machineElevatorService == null)
            {
                throw new System.ArgumentNullException(nameof(machineElevatorService));
            }

            this.MachineElevatorService = machineElevatorService;

            this.RefreshCanExecuteCommands();
        }

        #endregion

        #region Properties

        public bool CanExecuteMoveBackwardsCommand
        {
            get => this.canExecuteMoveBackwardsCommand;
            set => this.SetProperty(ref this.canExecuteMoveBackwardsCommand, value);
        }

        public bool CanExecuteMoveForwardsCommand
        {
            get => this.canExecuteMoveForwardCommand;
            set => this.SetProperty(ref this.canExecuteMoveForwardCommand, value);
        }

        public bool IsMovingBackwards
        {
            get => this.isMovingBackwards;
            set
            {
                if (this.SetProperty(ref this.isMovingBackwards, value))
                {
                    this.RefreshCanExecuteCommands();
                }
            }
        }

        public bool IsMovingForwards
        {
            get => this.isMovingForwards;
            set
            {
                if (this.SetProperty(ref this.isMovingForwards, value))
                {
                    this.RefreshCanExecuteCommands();
                }
            }
        }

        public bool IsStopping
        {
            get => this.isStopping;
            set
            {
                if (this.SetProperty(ref this.isStopping, value))
                {
                    this.RefreshCanExecuteCommands();
                }
            }
        }

        public IMachineElevatorService MachineElevatorService { get; }

        public DelegateCommand MoveBackwardsCommand =>
            this.moveBackwardsCommand
            ??
            (this.moveBackwardsCommand = new DelegateCommand(async () => await this.MoveBackwardsAsync()));

        public DelegateCommand MoveForwardButtonCommand =>
            this.moveForwardsCommand
            ??
            (this.moveForwardsCommand = new DelegateCommand(async () => await this.MoveForwardsAsync()));

        #endregion

        #region Methods

        protected override async Task StopMovementAsync()
        {
            this.IsStopping = true;

            try
            {
                await this.MachineElevatorService.StopAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowError(ex);
            }
            finally
            {
                this.IsMovingForwards = false;
                this.IsMovingBackwards = false;
                this.IsStopping = false;
            }
        }

        private async Task MoveBackwardsAsync()
        {
            this.IsMovingBackwards = true;
            this.IsMovingForwards = false;

            await this.StartMovementAsync(-1);
        }

        private async Task MoveForwardsAsync()
        {
            this.IsMovingForwards = true;
            this.IsMovingBackwards = false;

            await this.StartMovementAsync(1);
        }

        private void RefreshCanExecuteCommands()
        {
            this.CanExecuteMoveBackwardsCommand = !this.IsMovingForwards && !this.IsStopping;
            this.CanExecuteMoveForwardsCommand = !this.IsMovingBackwards && !this.IsStopping;
        }

        private async Task StartMovementAsync(decimal displacement)
        {
            try
            {
                await this.MachineElevatorService.MoveHorizontalAsync(
                    new ElevatorMovementParameters
                    {
                        MovementType = MovementType.Relative,
                        SpeedPercentage = 0,
                        Displacement = displacement
                    });
            }
            catch (System.Exception ex)
            {
                this.IsMovingForwards = false;
                this.IsMovingBackwards = false;

                this.ShowError(ex);
            }
        }

        #endregion
    }
}

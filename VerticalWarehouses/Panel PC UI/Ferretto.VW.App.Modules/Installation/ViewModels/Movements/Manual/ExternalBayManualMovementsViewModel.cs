using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class ExternalBayManualMovementsViewModel : BaseManualMovementsViewModel
    {
        #region Fields

        private readonly IMachineBaysWebService machineBaysWebService;

        private bool canExecuteMoveBackwardsCommand;

        private bool canExecuteMoveForwardCommand;

        private bool isMovingBackwards;

        private bool isMovingForwards;

        private bool isStopping;

        private DelegateCommand moveBackwardsCommand;

        private DelegateCommand moveForwardsCommand;

        #endregion

        #region Constructors

        public ExternalBayManualMovementsViewModel(
            IMachineElevatorWebService elevatorWebService,
            IMachineBaysWebService machineBaysWebService,
            IBayManager bayManager)
            : base(elevatorWebService, bayManager)
        {
            this.machineBaysWebService = machineBaysWebService ?? throw new System.ArgumentNullException(nameof(machineBaysWebService));

            this.RefreshCanExecuteCommands();
        }

        #endregion

        #region Properties

        public bool CanExecuteMoveBackwardsCommand
        {
            get => this.canExecuteMoveBackwardsCommand;
            private set => this.SetProperty(ref this.canExecuteMoveBackwardsCommand, value);
        }

        public bool CanExecuteMoveForwardsCommand
        {
            get => this.canExecuteMoveForwardCommand;
            private set => this.SetProperty(ref this.canExecuteMoveForwardCommand, value);
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

        public ICommand MoveForwardsCommand =>
            this.moveForwardsCommand
            ??
            (this.moveForwardsCommand = new DelegateCommand(async () => await this.MoveForwardsAsync()));

        #endregion

        #region Methods

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
                this.IsMovingForwards = false;
                this.IsMovingBackwards = false;
                this.IsStopping = false;
                this.EnableAll();
            }
        }

        private async Task MoveBackwardsAsync()
        {
            this.IsMovingBackwards = true;
            this.IsMovingForwards = false;

            this.DisableAllExceptThis();

            await this.StartMovementAsync(HorizontalMovementDirection.Backwards);
        }

        private async Task MoveForwardsAsync()
        {
            this.IsMovingForwards = true;
            this.IsMovingBackwards = false;

            this.DisableAllExceptThis();

            await this.StartMovementAsync(HorizontalMovementDirection.Forwards);
        }

        private void RefreshCanExecuteCommands()
        {
            this.CanExecuteMoveBackwardsCommand = !this.IsMovingForwards && !this.IsStopping;
            this.CanExecuteMoveForwardsCommand = !this.IsMovingBackwards && !this.IsStopping;
        }

        private async Task StartMovementAsync(HorizontalMovementDirection direction)
        {
            try
            {
                await this.machineBaysWebService.MoveAsync(direction);
            }
            catch (System.Exception ex)
            {
                this.IsMovingForwards = false;
                this.IsMovingBackwards = false;

                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}

using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ShutterPosition = Ferretto.VW.MAS.AutomationService.Contracts.ShutterPosition;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class ShutterEngineManualMovementsViewModel : BaseManualMovementsViewModel
    {
        #region Fields

        private readonly IMachineShuttersWebService shuttersWebService;

        private bool canExecuteMoveDownCommand;

        private bool canExecuteMoveUpCommand;

        private bool isMovingDown;

        private bool isMovingUp;

        private bool isStopping;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveUpCommand;

        #endregion

        #region Constructors

        public ShutterEngineManualMovementsViewModel(
            IMachineShuttersWebService shuttersWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IBayManager bayManager)
            : base(machineElevatorWebService, bayManager)
        {
            if (shuttersWebService is null)
            {
                throw new System.ArgumentNullException(nameof(shuttersWebService));
            }

            this.shuttersWebService = shuttersWebService;
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

        public ICommand MoveDownCommand =>
            this.moveDownCommand
            ??
            (this.moveDownCommand = new DelegateCommand(async () => await this.MoveDownAsync()));

        public ICommand MoveUpCommand =>
            this.moveUpCommand
            ??
            (this.moveUpCommand = new DelegateCommand(async () => await this.MoveUpAsync()));

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            /*
             * Avoid unsubscribing in case of navigation to error page.
             * We may need to review this behaviour.
             *
            this.subscriptionToken?.Dispose();
            this.subscriptionToken = null;
            */
        }

        public async Task MoveDownAsync()
        {
            this.IsMovingDown = true;
            this.IsMovingUp = false;

            this.DisableAllExceptThis();

            await this.StartMovementAsync(ShutterMovementDirection.Down);
        }

        public async Task MoveUpAsync()
        {
            this.IsMovingUp = true;
            this.IsMovingDown = false;

            this.DisableAllExceptThis();

            await this.StartMovementAsync(ShutterMovementDirection.Up);
        }

        protected override void OnMachinePowerChanged()
        {
            this.RefreshCanExecuteCommands();
        }

        protected override async Task StopMovementAsync()
        {
            try
            {
                this.IsStopping = true;

                await this.shuttersWebService.StopAsync();
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
                this.EnableAll();
            }
        }

        private void RefreshCanExecuteCommands()
        {
            this.CanExecuteMoveUpCommand = !this.IsMovingDown && !this.IsStopping;
            this.CanExecuteMoveDownCommand = !this.IsMovingUp && !this.IsStopping;
        }

        private async Task StartMovementAsync(ShutterMovementDirection direction)
        {
            try
            {
                await this.shuttersWebService.MoveAsync(direction);
            }
            catch (System.Exception ex)
            {
                this.IsMovingDown = false;
                this.IsMovingUp = false;

                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}

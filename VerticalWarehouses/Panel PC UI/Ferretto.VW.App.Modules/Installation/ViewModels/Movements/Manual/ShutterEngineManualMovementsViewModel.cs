using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ShutterPosition = Ferretto.VW.MAS.AutomationService.Contracts.ShutterPosition;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ShutterEngineManualMovementsViewModel : BaseManualMovementsViewModel
    {
        #region Fields

        private readonly IMachineShuttersService shuttersService;

        private bool canExecuteMoveDownCommand;

        private bool canExecuteMoveUpCommand;

        private ShutterPosition? currentPosition;

        private bool isMovingDown;

        private bool isMovingUp;

        private bool isStopping;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveUpCommand;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public ShutterEngineManualMovementsViewModel(
            IMachineShuttersService shuttersService,
            IMachineElevatorService machineElevatorService,
            IBayManager bayManager)
            : base(machineElevatorService, bayManager)
        {
            if (shuttersService is null)
            {
                throw new System.ArgumentNullException(nameof(shuttersService));
            }

            this.shuttersService = shuttersService;
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

        public ShutterPosition? CurrentPosition
        {
            get => this.currentPosition;
            private set => this.SetProperty(ref this.currentPosition, value);
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

            if (this.subscriptionToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }
        }

        public async Task MoveDownAsync()
        {
            this.IsMovingDown = true;
            this.IsMovingUp = false;

            this.DisableAllExceptThis();

            await this.StartMovementAsync(MAS.AutomationService.Contracts.ShutterMovementDirection.Down);
        }

        public async Task MoveUpAsync()
        {
            this.IsMovingUp = true;
            this.IsMovingDown = false;

            this.DisableAllExceptThis();

            await this.StartMovementAsync(MAS.AutomationService.Contracts.ShutterMovementDirection.Up);
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.subscriptionToken = this.EventAggregator
              .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
              .Subscribe(
                  message => this.CurrentPosition = (ShutterPosition?)message?.Data?.ShutterPosition,
                  ThreadOption.UIThread,
                  false);
            try
            {
                this.CurrentPosition = await this.shuttersService.GetShutterPositionAsync();
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

                await this.shuttersService.StopAsync();
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

        private async Task StartMovementAsync(MAS.AutomationService.Contracts.ShutterMovementDirection direction)
        {
            try
            {
                await this.shuttersService.MoveAsync(direction);
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

using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ShutterEngineManualMovementsViewModel : BaseManualMovementsViewModel
    {
        #region Fields

        private readonly IMachineShutterService shutterService;

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
            IMachineShutterService shutterService,
            IMachineHomingService homingService)
            : base(homingService)
        {
            if (shutterService == null)
            {
                throw new System.ArgumentNullException(nameof(shutterService));
            }

            this.shutterService = shutterService;

            this.RefreshCanExecuteCommands();
        }

        #endregion

        #region Properties

        public bool CanExecuteMoveDownCommand
        {
            get => this.canExecuteMoveDownCommand;
            set => this.SetProperty(ref this.canExecuteMoveDownCommand, value);
        }

        public bool CanExecuteMoveUpCommand
        {
            get => this.canExecuteMoveUpCommand;
            set => this.SetProperty(ref this.canExecuteMoveUpCommand, value);
        }

        public new ShutterPosition? CurrentPosition
        {
            get => this.currentPosition;
            set => this.SetProperty(ref this.currentPosition, value);
        }

        public bool IsMovingDown
        {
            get => this.isMovingDown;
            set
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
            set
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
            set
            {
                if (this.SetProperty(ref this.isStopping, value))
                {
                    this.RefreshCanExecuteCommands();
                }
            }
        }

        public DelegateCommand MoveDownCommand =>
            this.moveDownCommand
            ??
            (this.moveDownCommand = new DelegateCommand(async () => await this.MoveDownAsync()));

        public DelegateCommand MoveUpCommand =>
            this.moveUpCommand
            ??
            (this.moveUpCommand = new DelegateCommand(async () => await this.MoveUpAsync()));

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public async Task MoveDownAsync()
        {
            this.IsMovingDown = true;
            this.IsMovingUp = false;

            var messageData = new ShutterPositioningMovementMessageDataDto
            {
                BayNumber = 1,
                ShutterPositionMovement = MAS.AutomationService.Contracts.ShutterMovementDirection.Down
            };

            await this.StartMovementAsync(messageData);
        }

        public async Task MoveUpAsync()
        {
            this.IsMovingUp = true;
            this.IsMovingDown = false;

            var messageData = new ShutterPositioningMovementMessageDataDto
            {
                BayNumber = 1,
                ShutterPositionMovement = MAS.AutomationService.Contracts.ShutterMovementDirection.Up
            };

            await this.StartMovementAsync(messageData);
        }

        public override async Task OnNavigatedAsync()
        {
            this.subscriptionToken = this.EventAggregator
              .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
              .Subscribe(
                  message => this.CurrentPosition = message?.Data?.ShutterPosition,
                  ThreadOption.PublisherThread,
                  false);

            await base.OnNavigatedAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.subscriptionToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }

            base.Dispose(disposing);
        }

        protected override async Task StopMovementAsync()
        {
            try
            {
                this.IsStopping = true;

                await this.shutterService.StopAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowError(ex);
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

        private async Task StartMovementAsync(ShutterPositioningMovementMessageDataDto messageData)
        {
            try
            {
                await this.shutterService.ExecutePositioningAsync(messageData);
            }
            catch (System.Exception ex)
            {
                this.IsMovingDown = false;
                this.IsMovingUp = false;

                this.ShowError(ex);
            }
        }

        #endregion
    }
}

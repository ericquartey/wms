using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ShutterMovementDirection = Ferretto.VW.MAS.AutomationService.Contracts.ShutterMovementDirection;
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

        private bool isCompleted;

        private bool isMovingDown;

        private bool isMovingUp;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveUpCommand;

        private SubscriptionToken shutterPositionToken;

        #endregion

        #region Constructors

        public ShutterEngineManualMovementsViewModel(
            IMachineShuttersWebService shuttersWebService,
            IMachineElevatorWebService elevatorWebService,
            IMachineSensorsWebService machineSensorsWebService,
            IHealthProbeService healthProbeService,
            IBayManager bayManager)
            : base(elevatorWebService,
                   machineSensorsWebService,
                   healthProbeService,
                   bayManager)
        {
            if (shuttersWebService is null)
            {
                throw new System.ArgumentNullException(nameof(shuttersWebService));
            }

            this.shuttersWebService = shuttersWebService;
            this.RaiseCanExecuteChanged();
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

            this.shutterPositionToken?.Dispose();
            this.shutterPositionToken = null;
        }

        public async Task MoveDownAsync()
        {
            this.DisableAllExceptThis();

            await this.StartMovementAsync(ShutterMovementDirection.Down);
        }

        public async Task MoveUpAsync()
        {
            this.DisableAllExceptThis();

            await this.StartMovementAsync(ShutterMovementDirection.Up);
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.shutterPositionToken = this.shutterPositionToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                    .Subscribe(
                        this.OnShutterPositionChanged,
                        ThreadOption.UIThread,
                        false);

            this.isCompleted = false;
        }

        protected override void OnErrorStatusChanged()
        {
            // if (!this.IsEnabled)
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

            this.CanExecuteMoveUpCommand = !this.IsMovingDown && !this.IsStopping;
            this.CanExecuteMoveDownCommand = !this.IsMovingUp && !this.IsStopping;
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
                await this.shuttersWebService.StopAsync();
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

        private void OnShutterPositionChanged(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.ShowNotification("Movimento serranda in corso...", Services.Models.NotificationSeverity.Info);
                    this.isCompleted = false;
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

        private async Task StartMovementAsync(ShutterMovementDirection direction)
        {
            if (this.IsMovingUp || this.IsMovingDown)
            {
                return;
            }

            try
            {
                await this.shuttersWebService.MoveAsync(direction);
                if (direction == ShutterMovementDirection.Down)
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
        }

        #endregion
    }
}

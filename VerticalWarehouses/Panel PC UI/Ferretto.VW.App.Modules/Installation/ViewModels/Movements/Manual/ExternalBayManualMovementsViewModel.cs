using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;
using HorizontalMovementDirection = Ferretto.VW.MAS.AutomationService.Contracts.HorizontalMovementDirection;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class ExternalBayManualMovementsViewModel : BaseManualMovementsViewModel
    {
        #region Fields

        private readonly IMachineBaysWebService machineBaysWebService;

        private bool canExecuteMoveBackwardsCommand;

        private bool canExecuteMoveForwardCommand;

        private bool isCompleted;

        private bool isMovingBackwards;

        private bool isMovingForwards;

        private DelegateCommand moveBackwardsCommand;

        private DelegateCommand moveForwardsCommand;

        private SubscriptionToken positioningToken;

        #endregion

        #region Constructors

        public ExternalBayManualMovementsViewModel(
            IMachineElevatorWebService elevatorWebService,
            IMachineBaysWebService machineBaysWebService,
            IBayManager bayManager)
            : base(elevatorWebService, bayManager)
        {
            this.machineBaysWebService = machineBaysWebService ?? throw new System.ArgumentNullException(nameof(machineBaysWebService));

            this.RaiseCanExecuteChanged();
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

        public override void Disappear()
        {
            base.Disappear();

            this.positioningToken?.Dispose();
            this.positioningToken = null;
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
                        m => m.Data?.AxisMovement == Axis.BayChain);

            this.isCompleted = false;
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
            this.CanExecuteMoveBackwardsCommand = !this.IsMovingForwards && !this.IsStopping;
            this.CanExecuteMoveForwardsCommand = !this.IsMovingBackwards && !this.IsStopping;
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
                this.IsMovingForwards = false;
                this.IsMovingBackwards = false;
                this.IsStopping = false;
                this.EnableAll();
            }
        }

        private void CloseOperation()
        {
            this.StopMoving();
            this.IsStopping = false;
            this.EnableAll();
            this.isCompleted = true;
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

        private void OnElevatorPositionChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.ShowNotification("Movimento baia in corso...", Services.Models.NotificationSeverity.Info);
                    this.isCompleted = false;

                    break;

                case MessageStatus.OperationExecuting:
                    if (!this.isCompleted)
                    {
                        this.ShowNotification("Movimento baia in corso...", Services.Models.NotificationSeverity.Info);
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
                await this.machineBaysWebService.MoveAsync(direction);
            }
            catch (System.Exception ex)
            {
                this.CloseOperation();

                this.ShowNotification(ex);
            }
        }

        private void StopMoving()
        {
            this.IsMovingForwards = false;
            this.IsMovingBackwards = false;
        }

        #endregion
    }
}

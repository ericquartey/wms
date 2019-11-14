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
    internal sealed class CarouselManualMovementsViewModel : BaseManualMovementsViewModel
    {
        #region Fields

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private bool canExecuteCloseCommand;

        private bool canExecuteOpenCommand;

        private DelegateCommand closeCommand;

        private bool isClosing;

        private bool isCompleted;

        private bool isOpening;

        private DelegateCommand openCommand;

        private SubscriptionToken positioningToken;

        #endregion

        #region Constructors

        public CarouselManualMovementsViewModel(
            IMachineCarouselWebService machineCarouselWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IBayManager bayManagerService)
            : base(machineElevatorWebService, bayManagerService)
        {
            this.machineCarouselWebService = machineCarouselWebService;

            this.RaiseCanExecuteChanged();
        }

        #endregion

        #region Properties

        public bool CanExecuteCloseCommand
        {
            get => this.canExecuteCloseCommand;
            private set => this.SetProperty(ref this.canExecuteCloseCommand, value);
        }

        public bool CanExecuteOpenCommand
        {
            get => this.canExecuteOpenCommand;
            private set => this.SetProperty(ref this.canExecuteOpenCommand, value);
        }

        public ICommand CloseCommand =>
            this.closeCommand
            ??
            (this.closeCommand = new DelegateCommand(async () => await this.CloseCarouselAsync()));

        public bool IsClosing
        {
            get => this.isClosing;
            private set
            {
                if (this.SetProperty(ref this.isClosing, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsOpening
        {
            get => this.isOpening;
            private set
            {
                if (this.SetProperty(ref this.isOpening, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand OpenCommand =>
            this.openCommand
            ??
            (this.openCommand = new DelegateCommand(async () => await this.OpenCarouselAsync()));

        #endregion

        #region Methods

        public async Task CloseCarouselAsync()
        {
            this.IsClosing = true;

            this.DisableAllExceptThis();

            await this.StartMovementAsync(HorizontalMovementDirection.Backwards);
        }

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
                        false);

            this.isCompleted = false;
        }

        public async Task OpenCarouselAsync()
        {
            this.IsOpening = true;

            this.DisableAllExceptThis();

            await this.StartMovementAsync(HorizontalMovementDirection.Forwards);
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

            this.CanExecuteCloseCommand = !this.IsOpening && !this.IsStopping;
            this.CanExecuteOpenCommand = !this.IsClosing && !this.IsStopping;
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

                await this.machineCarouselWebService.StopAsync();
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
            if (message.Data?.AxisMovement != Axis.BayChain)
            {
                return;
            }

            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.ShowNotification("Movimento giostra in corso..", Services.Models.NotificationSeverity.Info);
                    this.isCompleted = false;

                    break;

                case MessageStatus.OperationExecuting:
                    if (!this.isCompleted)
                    {
                        this.ShowNotification("Movimento giostra in corso..", Services.Models.NotificationSeverity.Info);
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
                await this.machineCarouselWebService.MoveManualAsync(direction);
            }
            catch (System.Exception ex)
            {
                this.CloseOperation();

                this.ShowNotification(ex);
            }
        }

        private void StopMoving()
        {
            this.IsClosing = false;
            this.IsOpening = false;
        }

        #endregion
    }
}

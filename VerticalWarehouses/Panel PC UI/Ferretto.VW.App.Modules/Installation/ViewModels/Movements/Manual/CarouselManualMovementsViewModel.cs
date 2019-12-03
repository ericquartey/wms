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
            IMachineElevatorWebService elevatorWebService,
            IMachineSensorsWebService machineSensorsWebService,
            IHealthProbeService healthProbeService,
            IBayManager bayManager)
            : base(
                elevatorWebService,
                machineSensorsWebService,
                healthProbeService,
                bayManager)
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
            (this.closeCommand = new DelegateCommand(async () => await this.MoveCarouselDownAsync()));

        public bool IsClosing
        {
            get => this.isClosing;
            private set => this.SetProperty(ref this.isClosing, value, this.RaiseCanExecuteChanged);
        }

        public bool IsOpening
        {
            get => this.isOpening;
            private set => this.SetProperty(ref this.isOpening, value, this.RaiseCanExecuteChanged);
        }

        public ICommand OpenCommand =>
            this.openCommand
            ??
            (this.openCommand = new DelegateCommand(async () => await this.OpenCarouselAsync()));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.positioningToken?.Dispose();
            this.positioningToken = null;
        }

        public async Task MoveCarouselDownAsync()
        {
            this.DisableAllExceptThis();

            await this.StartMovementAsync(VerticalMovementDirection.Down);
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

        public async Task OpenCarouselAsync()
        {
            this.DisableAllExceptThis();

            await this.StartMovementAsync(VerticalMovementDirection.Up);
        }

        protected override void OnErrorStatusChanged()
        {
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
            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.ShowNotification("Movimento giostra in corso...", Services.Models.NotificationSeverity.Info);
                    this.isCompleted = false;

                    break;

                case MessageStatus.OperationExecuting:
                    if (!this.isCompleted)
                    {
                        this.ShowNotification("Movimento giostra in corso...", Services.Models.NotificationSeverity.Info);
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

        private async Task StartMovementAsync(VerticalMovementDirection direction)
        {
            if (this.IsClosing || this.IsOpening)
            {
                return;
            }

            try
            {
                await this.machineCarouselWebService.MoveManualAsync(direction);

                this.IsClosing = direction is VerticalMovementDirection.Down;
                this.IsOpening = direction is VerticalMovementDirection.Up;
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

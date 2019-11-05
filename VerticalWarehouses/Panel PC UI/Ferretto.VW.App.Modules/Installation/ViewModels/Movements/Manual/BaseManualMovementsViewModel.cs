using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal abstract class BaseManualMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private Bay bay;

        private int bayNumber;

        private double? currentBayChainPosition;

        private double? currentHorizontalPosition;

        private double? currentVerticalPosition;

        private SubscriptionToken movementsSubscriptionToken;

        private SubscriptionToken notificationUIsubscriptionToken;

        private DelegateCommand stopMovementCommand;

        #endregion

        #region Constructors

        protected BaseManualMovementsViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IBayManager bayManagerService)
            : base(PresentationMode.Installer)
        {
            this.MachineElevatorService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));
        }

        #endregion

        #region Properties

        public int BayNumber
        {
            get => this.bayNumber;
            protected set => this.SetProperty(ref this.bayNumber, value);
        }

        public double? CurrentBayChainPosition
        {
            get => this.currentBayChainPosition;
            protected set => this.SetProperty(ref this.currentBayChainPosition, value);
        }

        public double? CurrentHorizontalPosition
        {
            get => this.currentHorizontalPosition;
            protected set => this.SetProperty(ref this.currentHorizontalPosition, value);
        }

        public double? CurrentVerticalPosition
        {
            get => this.currentVerticalPosition;
            protected set => this.SetProperty(ref this.currentVerticalPosition, value);
        }

        public DelegateCommand StopMovementCommand =>
            this.stopMovementCommand
            ??
            (this.stopMovementCommand = new DelegateCommand(async () => await this.StopMovementAsync()));

        protected IMachineElevatorWebService MachineElevatorService { get; }

        #endregion

        #region Methods

        public void DisableAllExceptThis()
        {
            var name = this.GetType().ToString();
            this.EventAggregator
                .GetEvent<ManualMovementsChangedPubSubEvent>()
                .Publish(new ManualMovementsChangedMessage(name));
        }

        public override void Disappear()
        {
            base.Disappear();

            /*
             * Avoid unsubscribing in case of navigation to error page.
             * We may need to review this behaviour.
             *
            this.notificationUIsubscriptionToken?.Dispose();
            this.notificationUIsubscriptionToken = null;

            this.movementsSubscriptionToken?.Dispose();
            this.movementsSubscriptionToken = null;
            */
        }

        public void EnableAll()
        {
            this.EventAggregator
               .GetEvent<ManualMovementsChangedPubSubEvent>()
               .Publish(new ManualMovementsChangedMessage(null));
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            this.SubscribeToEvents();

            await this.RetrieveCurrentPositionAsync();

            await base.OnAppearedAsync();

            this.EnableAll();
        }

        protected virtual void EnabledChanged(ManualMovementsChangedMessage message)
        {
            if (string.IsNullOrEmpty(message.ViewModelName))
            {
                this.IsEnabled = true;
                return;
            }

            var name = this.GetType().ToString();
            if (!name.Equals(message.ViewModelName))
            {
                this.IsEnabled = false;
            }
        }

        protected abstract Task StopMovementAsync();

        private void OnElevatorPositionChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            switch (message.Data.AxisMovement)
            {
                case CommonUtils.Messages.Enumerations.Axis.Horizontal:
                    if (message.Data.MovementMode < CommonUtils.Messages.Enumerations.MovementMode.BayChain)
                    {
                        this.CurrentHorizontalPosition = message.Data.CurrentPosition ?? this.CurrentHorizontalPosition;
                    }
                    else
                    {
                        this.CurrentBayChainPosition = message.Data.CurrentPosition ?? this.CurrentBayChainPosition;
                    }

                    break;

                case CommonUtils.Messages.Enumerations.Axis.Vertical:
                    this.CurrentVerticalPosition = message.Data.CurrentPosition ?? this.CurrentVerticalPosition;
                    break;

                case CommonUtils.Messages.Enumerations.Axis.BayChain:
                    this.CurrentBayChainPosition = message?.Data?.CurrentPosition ?? this.CurrentBayChainPosition;
                    break;

                default:
                    break;
            }
        }

        private async Task RetrieveCurrentPositionAsync()
        {
            try
            {
                this.bay = await this.bayManagerService.GetBayAsync();
                this.BayNumber = (int)this.bay.Number;

                this.CurrentVerticalPosition = await this.MachineElevatorService.GetVerticalPositionAsync();
                this.CurrentHorizontalPosition = await this.MachineElevatorService.GetHorizontalPositionAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void SubscribeToEvents()
        {
            this.notificationUIsubscriptionToken = this.notificationUIsubscriptionToken
                            ??
                            this.EventAggregator
                                .GetEvent<NotificationEventUI<PositioningMessageData>>()
                                .Subscribe(
                                    this.OnElevatorPositionChanged,
                                    ThreadOption.UIThread,
                                    false,
                                    m => m.Data != null);

            this.movementsSubscriptionToken = this.movementsSubscriptionToken
                ??
                this.EventAggregator
                    .GetEvent<ManualMovementsChangedPubSubEvent>()
                    .Subscribe(
                        this.EnabledChanged,
                        ThreadOption.UIThread,
                        false,
                        message => message != null);
        }

        #endregion
    }
}

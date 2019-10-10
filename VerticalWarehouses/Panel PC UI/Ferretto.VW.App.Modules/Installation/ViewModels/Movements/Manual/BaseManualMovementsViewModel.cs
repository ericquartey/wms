using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public abstract class BaseManualMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private double? currentBayChainPosition;

        private double? currentHorizontalPosition;

        private double? currentVerticalPosition;

        private SubscriptionToken movementsSubscriptionToken;

        private SubscriptionToken notificationUIsubscriptionToken;

        private DelegateCommand stopMovementCommand;

        #endregion

        #region Constructors

        protected BaseManualMovementsViewModel(
            IMachineElevatorService machineElevatorService,
            IBayManager bayManagerService)
            : base(PresentationMode.Installer)
        {
            if (machineElevatorService is null)
            {
                throw new ArgumentNullException(nameof(machineElevatorService));
            }

            if (bayManagerService is null)
            {
                throw new ArgumentNullException(nameof(bayManagerService));
            }

            this.MachineElevatorService = machineElevatorService;
            this.bayManagerService = bayManagerService;
        }

        #endregion

        #region Properties

        public int BayNumber => (int)this.bayManagerService.Bay.Number;

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

        protected IMachineElevatorService MachineElevatorService { get; }

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

            if (this.notificationUIsubscriptionToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Unsubscribe(this.notificationUIsubscriptionToken);
                this.notificationUIsubscriptionToken = null;
            }

            if (this.movementsSubscriptionToken != null)
            {
                this.EventAggregator
                 .GetEvent<ManualMovementsChangedPubSubEvent>()
                 .Unsubscribe(this.movementsSubscriptionToken);

                this.movementsSubscriptionToken = null;
            }
        }

        public void EnableAll()
        {
            this.EventAggregator
               .GetEvent<ManualMovementsChangedPubSubEvent>()
               .Publish(new ManualMovementsChangedMessage(null));
        }

        public virtual void EnabledChanged(ManualMovementsChangedMessage message)
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

        public override async Task OnNavigatedAsync()
        {
            this.IsBackNavigationAllowed = true;

            this.notificationUIsubscriptionToken = this.EventAggregator
              .GetEvent<NotificationEventUI<PositioningMessageData>>()
              .Subscribe(
                  this.UpdatePositions,
                  ThreadOption.UIThread,
                  false);

            this.movementsSubscriptionToken = this.EventAggregator
             .GetEvent<ManualMovementsChangedPubSubEvent>()
             .Subscribe(
                 this.EnabledChanged,
                 ThreadOption.UIThread,
                 false);

            await this.RetrieveCurrentPositionAsync();

            await base.OnNavigatedAsync();

            this.EnableAll();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            this.RetrieveCurrentPositionAsync();
        }

        protected abstract Task StopMovementAsync();

        private async Task RetrieveCurrentPositionAsync()
        {
            try
            {
                this.CurrentVerticalPosition = await this.MachineElevatorService.GetVerticalPositionAsync();
                this.CurrentHorizontalPosition = await this.MachineElevatorService.GetHorizontalPositionAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void UpdatePositions(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message is null
                ||
                message.Data is null)
            {
                return;
            }

            switch (message.Data.AxisMovement)
            {
                case CommonUtils.Messages.Enumerations.Axis.None:
                    break;

                case CommonUtils.Messages.Enumerations.Axis.Horizontal:
                    if (message.Data.MovementMode < CommonUtils.Messages.Enumerations.MovementMode.BayChain)
                    {
                        this.CurrentHorizontalPosition = message?.Data?.CurrentPosition ?? this.CurrentHorizontalPosition;
                    }
                    else
                    {
                        this.CurrentBayChainPosition = message?.Data?.CurrentPosition ?? this.CurrentBayChainPosition;
                    }
                    break;

                case CommonUtils.Messages.Enumerations.Axis.Vertical:
                    this.CurrentVerticalPosition = message?.Data?.CurrentPosition ?? this.CurrentVerticalPosition;
                    break;

                case CommonUtils.Messages.Enumerations.Axis.HorizontalAndVertical:
                    break;

                default:
                    break;
            }
        }

        #endregion
    }
}

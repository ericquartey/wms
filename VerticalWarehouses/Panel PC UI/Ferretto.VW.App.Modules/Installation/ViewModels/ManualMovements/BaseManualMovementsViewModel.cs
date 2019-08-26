using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils;
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

        private readonly IMachineElevatorService machineElevatorService;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private decimal? currentPosition;

        private DelegateCommand stopMovementCommand;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        protected BaseManualMovementsViewModel(
            IMachineElevatorService machineElevatorService,
            IBayManager bayManagerService)
            : base(PresentationMode.Installer)
        {
            if (machineElevatorService is null)
            {
                throw new System.ArgumentNullException(nameof(machineElevatorService));
            }

            if (bayManagerService is null)
            {
                throw new System.ArgumentNullException(nameof(bayManagerService));
            }

            this.machineElevatorService = machineElevatorService;
            this.bayManagerService = bayManagerService;

            this.InitializeNavigationMenu();
        }

        #endregion

        #region Properties

        public int BayNumber => this.bayManagerService.Bay.Number;

        public decimal? CurrentPosition
        {
            get => this.currentPosition;
            protected set => this.SetProperty(ref this.currentPosition, value);
        }

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        public DelegateCommand StopMovementCommand =>
            this.stopMovementCommand
            ??
            (this.stopMovementCommand = new DelegateCommand(async () => await this.StopMovementAsync()));

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

        public override async Task OnNavigatedAsync()
        {
            this.subscriptionToken = this.EventAggregator
              .GetEvent<NotificationEventUI<PositioningMessageData>>()
              .Subscribe(
                  message => this.CurrentPosition = message?.Data?.CurrentPosition,
                  ThreadOption.UIThread,
                  false);

            await this.RetrieveCurrentPositionAsync();

            await base.OnNavigatedAsync();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            this.RetrieveCurrentPositionAsync();
        }

        protected abstract Task StopMovementAsync();

        private void InitializeNavigationMenu()
        {
            this.menuItems.Add(
               new NavigationMenuItem(
                   Utils.Modules.Installation.ManualMovements.VERTICALENGINE,
                   nameof(Utils.Modules.Installation),
                   VW.App.Resources.InstallationApp.VerticalAxis,
                   trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.ManualMovements.HORIZONTALENGINE,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.HorizontalAxis,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.ManualMovements.SHUTTER,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Shutter,
                    trackCurrentView: false));

            if (this.bayManagerService.Bay.Type == BayType.Carousel
                ||
                this.bayManagerService.Bay.Type == BayType.ExternalCarousel)
            {
                this.menuItems.Add(
                    new NavigationMenuItem(
                        Utils.Modules.Installation.ManualMovements.CAROUSEL,
                        nameof(Utils.Modules.Installation),
                        VW.App.Resources.InstallationApp.Carousel,
                        trackCurrentView: false));
            }
        }

        private async Task RetrieveCurrentPositionAsync()
        {
            try
            {
                this.CurrentPosition = await this.machineElevatorService.GetVerticalPositionAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}

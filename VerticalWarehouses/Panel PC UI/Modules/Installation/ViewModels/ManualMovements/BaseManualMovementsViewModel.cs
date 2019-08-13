using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public abstract class BaseManualMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineHomingService homingService;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private decimal? currentPosition;

        private DelegateCommand stopMovementCommand;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        protected BaseManualMovementsViewModel(IMachineHomingService homingService)
            : base(Services.PresentationMode.Installator)
        {
            if (homingService == null)
            {
                throw new System.ArgumentNullException(nameof(homingService));
            }

            this.homingService = homingService;

            this.InitializeNavigationMenu();
        }

        #endregion

        #region Properties

        public decimal? CurrentPosition
        {
            get => this.currentPosition;
            set => this.SetProperty(ref this.currentPosition, value);
        }

        public BindingList<NavigationMenuItem> MenuItems => this.menuItems;

        public DelegateCommand StopMovementCommand =>
            this.stopMovementCommand
            ??
            (this.stopMovementCommand = new DelegateCommand(async () => await this.StopMovementAsync()));

        #endregion

        #region Methods

        public override async Task OnNavigatedAsync()
        {
            this.subscriptionToken = this.EventAggregator
              .GetEvent<NotificationEventUI<PositioningMessageData>>()
              .Subscribe(
                  message => this.CurrentPosition = message?.Data?.CurrentPosition,
                  ThreadOption.PublisherThread,
                  false);

            await this.homingService.NotifyCurrentAxisAxisAsync();

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

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.ManualMovements.CAROUSEL,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Carousel,
                    trackCurrentView: false));
        }

        #endregion
    }
}

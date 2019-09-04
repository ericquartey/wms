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
    public class SemiAutoMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private decimal? currentPosition;

        private DelegateCommand stopMovementCommand;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public SemiAutoMovementsViewModel(
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

            this.MachineElevatorService = machineElevatorService;
            this.bayManagerService = bayManagerService;
        }

        #endregion

        #region Properties

        public int BayNumber => this.bayManagerService.Bay.Number;

        public decimal? CurrentPosition
        {
            get => this.currentPosition;
            protected set => this.SetProperty(ref this.currentPosition, value);
        }

        protected IMachineElevatorService MachineElevatorService { get; }

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
            this.IsBackNavigationAllowed = true;

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

        private bool HasCarousel(Bay bay)
        {
            return
                bay.Type == BayType.Carousel
                ||
                bay.Type == BayType.ExternalCarousel;
        }

        private bool IsExternal(Bay bay)
        {
            return
                bay.Type == BayType.ExternalCarousel
                ||
                bay.Type == BayType.ExternalDouble
                ||
                bay.Type == BayType.ExternalSingle;
        }

        private async Task RetrieveCurrentPositionAsync()
        {
            try
            {
                this.CurrentPosition = await this.MachineElevatorService.GetVerticalPositionAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}

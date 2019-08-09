using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class BasePositioningViewModel : BaseManualMovementsViewModel
    {
        #region Fields

        private readonly IPositioningMachineService positioningService;

        private decimal? currentPosition;

        private DelegateCommand stopMovementCommand;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        protected BasePositioningViewModel(IPositioningMachineService positioningService)
        {
            if (positioningService == null)
            {
                throw new System.ArgumentNullException(nameof(positioningService));
            }

            this.positioningService = positioningService;
        }

        #endregion

        #region Properties

        public decimal? CurrentPosition
        {
            get => this.currentPosition;
            set => this.SetProperty(ref this.currentPosition, value);
        }

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

        protected async Task StartPositioningAsync(MovementMessageDataDto messageData)
        {
            try
            {
                await this.positioningService.ExecuteAsync(messageData);
            }
            catch (System.Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private async Task StopMovementAsync()
        {
            try
            {
                await this.positioningService.StopAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowError(ex);
            }
        }

        #endregion
    }
}

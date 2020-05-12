using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationShutdown : BasePresentationViewModel
    {
        #region Fields

        private readonly INavigationService navigationService;

        private readonly ISessionService sessionService;

        #endregion

        #region Constructors

        public PresentationShutdown(
            INavigationService navigationService,
            ISessionService sessionService)
            : base(PresentationTypes.Shutdown)
        {
            this.navigationService = navigationService;
            this.sessionService = sessionService;
        }

        #endregion

        #region Methods

        public override Task ExecuteAsync()
        {
            this.navigationService.IsBusy = true;

            var requestAccepted = this.sessionService.Shutdown();
            if (requestAccepted)
            {
                this.EventAggregator
                    .GetEvent<PresentationNotificationPubSubEvent>()
                    .Publish(new PresentationNotificationMessage(Resources.Localized.Get("General.ShuttingDown"), Services.Models.NotificationSeverity.Info)); // TODO localize string
            }
            else
            {
                this.navigationService.IsBusy = false;
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}

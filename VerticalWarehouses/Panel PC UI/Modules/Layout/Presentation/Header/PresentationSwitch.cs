using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationSwitch : BasePresentation
    {
        #region Fields

        private readonly INavigationService navigationService;

        private readonly ISessionService sessionService;

        private readonly IThemeService themeService;

        #endregion

        #region Constructors

        public PresentationSwitch(
            INavigationService navigationService,
              ISessionService sessionService,
              IThemeService themeService)
        {
            this.navigationService = navigationService;
            this.sessionService = sessionService;
            this.themeService = themeService;
            this.Type = PresentationTypes.Switch;
        }

        #endregion

        #region Properties

        public bool IsDarkThemeActive => this.themeService.ActiveTheme == ApplicationTheme.Dark;

        #endregion

        #region Methods

        public override void Execute()
        {
            this.navigationService.SetBusy(true);

            var requestAccepted = this.sessionService.Shutdown();
            if (requestAccepted)
            {
                this.EventAggregator
                    .GetEvent<PresentationChangedPubSubEvent>()
                    .Publish(new PresentationChangedMessage("Shutting down ..."));
            }
        }

        #endregion
    }
}

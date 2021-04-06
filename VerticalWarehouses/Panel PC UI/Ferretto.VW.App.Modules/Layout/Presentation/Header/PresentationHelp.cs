using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Prism.Regions;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationHelp : BasePresentationViewModel
    {
        #region Fields

        private readonly INavigationService navigationService;

        private readonly IRegionManager regionManager;

        private readonly ISessionService sessionService;

        #endregion

        #region Constructors

        public PresentationHelp(
            IRegionManager regionManager,
            INavigationService navigationService,
            ISessionService sessionService)
            : base(PresentationTypes.Help)
        {
            this.regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }

        #endregion

        #region Methods

        public override Task ExecuteAsync()
        {
            //this.navigationService.Appear(
            //   nameof(Utils.Modules.Installation),
            //   $"Help{this.GetActiveView()}",
            //   null,
            //   trackCurrentView: true);

            if(this.sessionService.UserAccessLevel > MAS.AutomationService.Contracts.UserAccessLevel.Operator)
            {

                this.navigationService.Appear(
                   nameof(Utils.Modules.Installation),
                   Utils.Modules.Installation.RELEASE,
                   null,
                   trackCurrentView: true);
            }

            return Task.CompletedTask;
        }

        private string GetActiveView()
        {
            var activeView = this.regionManager.Regions[Utils.Modules.Layout.REGION_MAINCONTENT].ActiveViews.FirstOrDefault();
            return activeView?.GetType()?.Name;
        }

        #endregion
    }
}

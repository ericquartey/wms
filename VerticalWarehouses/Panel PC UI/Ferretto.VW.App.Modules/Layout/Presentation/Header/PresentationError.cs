using System;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationError : BasePresentationViewModel
    {
        #region Fields

        private readonly INavigationService navigationService;

        #endregion

        #region Constructors

        public PresentationError(INavigationService navigationService)
            : base(PresentationTypes.Error)
        {
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        }

        #endregion

        #region Methods

        public override Task ExecuteAsync()
        {
            this.navigationService.Appear(
                nameof(Utils.Modules.Errors),
                Utils.Modules.Errors.ERRORDETAILSVIEW,
                data: null,
                trackCurrentView: true);

            return Task.CompletedTask;
        }

        #endregion
    }
}

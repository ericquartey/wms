using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;

namespace Ferretto.VW.App.Modules.Layout
{
    public class PresentationLogged : BasePresentation
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly INavigationService navigationService;

        private string userName;

        #endregion

        #region Constructors

        public PresentationLogged(
            IAuthenticationService authenticationService,
            INavigationService navigationService)
        {
            if (authenticationService == null)
            {
                throw new System.ArgumentNullException(nameof(authenticationService));
            }

            if (navigationService == null)
            {
                throw new System.ArgumentNullException(nameof(navigationService));
            }

            this.authenticationService = authenticationService;
            this.navigationService = navigationService;
            this.Type = PresentationTypes.Logged;

            this.UserName = this.authenticationService.UserName;
            this.authenticationService.UserAuthenticated += this.AuthenticationService_UserAuthenticated;
        }

        #endregion

        #region Properties

        public string UserName
        {
            get => this.userName;
            set => this.SetProperty(ref this.userName, value);
        }

        #endregion

        #region Methods

        public override void Execute()
        {
            this.navigationService.Appear(nameof(Utils.Modules.Login), Utils.Modules.Login.LOGIN);
        }

        private void AuthenticationService_UserAuthenticated(object sender, UserAuthenticatedEventArgs e)
        {
            this.UserName = this.authenticationService.UserName;
        }

        #endregion
    }
}

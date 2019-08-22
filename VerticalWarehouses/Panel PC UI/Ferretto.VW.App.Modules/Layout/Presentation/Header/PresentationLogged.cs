using System.Threading.Tasks;
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
            : base(PresentationTypes.Logged)
        {
            if (authenticationService is null)
            {
                throw new System.ArgumentNullException(nameof(authenticationService));
            }

            if (navigationService is null)
            {
                throw new System.ArgumentNullException(nameof(navigationService));
            }

            this.authenticationService = authenticationService;
            this.navigationService = navigationService;
            this.Type = PresentationTypes.Logged;

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

        public override Task ExecuteAsync()
        {
            this.navigationService.Appear(nameof(Utils.Modules.Login), Utils.Modules.Login.LOGIN, false);

            return Task.CompletedTask;
        }

        private void AuthenticationService_UserAuthenticated(object sender, UserAuthenticatedEventArgs e)
        {
            this.UserName = this.authenticationService.UserName;
        }

        #endregion
    }
}

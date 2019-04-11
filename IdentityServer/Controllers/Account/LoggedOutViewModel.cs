namespace Ferretto.IdentityServer
{
    public class LoggedOutViewModel
    {
        #region Properties

        public bool AutomaticRedirectAfterSignOut { get; set; } = false;

        public string ClientName { get; set; }

        public string ExternalAuthenticationScheme { get; set; }

        public string LogoutId { get; set; }

        public string PostLogoutRedirectUri { get; set; }

        public string SignOutIframeUrl { get; set; }

        public bool TriggerExternalSignout => this.ExternalAuthenticationScheme != null;

        #endregion
    }
}

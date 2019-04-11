namespace Ferretto.IdentityServer
{
    public class LogoutViewModel : LogoutInputModel
    {
        #region Properties

        public bool ShowLogoutPrompt { get; set; } = true;

        #endregion
    }
}

namespace Ferretto.WMS.IdentityServer
{
    public class ProcessConsentResult
    {
        #region Properties

        public string ClientId { get; set; }

        public bool HasValidationError => this.ValidationError != null;

        public bool IsRedirect => this.RedirectUri != null;

        public string RedirectUri { get; set; }

        public bool ShowView => this.ViewModel != null;

        public string ValidationError { get; set; }

        public ConsentViewModel ViewModel { get; set; }

        #endregion
    }
}

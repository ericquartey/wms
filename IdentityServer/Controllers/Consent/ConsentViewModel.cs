using System.Collections.Generic;

namespace Ferretto.WMS.IdentityServer
{
    public class ConsentViewModel : ConsentInputModel
    {
        #region Properties

        public bool AllowRememberConsent { get; set; }

        public string ClientLogoUrl { get; set; }

        public string ClientName { get; set; }

        public string ClientUrl { get; set; }

        public IEnumerable<ScopeViewModel> IdentityScopes { get; set; }

        public IEnumerable<ScopeViewModel> ResourceScopes { get; set; }

        #endregion
    }
}

using System.Collections.Generic;

namespace Ferretto.WMS.IdentityServer
{
    public class ConsentInputModel
    {
        #region Properties

        public string Button { get; set; }

        public bool RememberConsent { get; set; }

        public string ReturnUrl { get; set; }

        public IEnumerable<string> ScopesConsented { get; set; }

        #endregion
    }
}

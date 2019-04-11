using System;
using System.Collections.Generic;

namespace Ferretto.IdentityServer
{
    public class GrantsViewModel
    {
        #region Properties

        public IEnumerable<GrantViewModel> Grants { get; set; }

        #endregion
    }

    public class GrantViewModel
    {
        #region Properties

        public IEnumerable<string> ApiGrantNames { get; set; }

        public string ClientId { get; set; }

        public string ClientLogoUrl { get; set; }

        public string ClientName { get; set; }

        public string ClientUrl { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Expires { get; set; }

        public IEnumerable<string> IdentityGrantNames { get; set; }

        #endregion
    }
}

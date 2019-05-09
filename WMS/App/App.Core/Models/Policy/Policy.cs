using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.App.Core.Models
{
    public class Policy : IPolicy
    {
        #region Properties

        public bool IsAllowed { get; set; }

        public string Name { get; set; }

        public string Reason { get; set; }

        public PolicyType Type { get; set; }

        #endregion
    }
}

using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public class Policy : IPolicy
    {
        #region Properties

        public bool IsAllowed { get; set; }

        public string Name { get; set; }

        public string Reason { get; set; }

        public PolicyType Type { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            var policyAction = this.IsAllowed ? "allow" : "deny";
            return $"{this.Type}: {policyAction} {this.Name}";
        }

        #endregion
    }
}

using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public class Policy : IPolicy
    {
        #region Fields

        private readonly List<string> errorMessages = new List<string>();

        #endregion

        #region Properties

        public bool IsAllowed => !this.errorMessages.Any();

        public string Name { get; set; }

        public string Reason
        {
            get
            {
                var reason = string.Join(System.Environment.NewLine, this.errorMessages);
                return !string.IsNullOrEmpty(reason) ? reason : null;
            }
        }

        public PolicyType Type { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            var policyAction = this.IsAllowed ? "allow" : "deny";
            return $"{this.Type}: {policyAction} {this.Name}";
        }

        public void AddErrorMessage(string errorMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                this.errorMessages.Add(errorMessage);
            }
        }

        #endregion
    }
}

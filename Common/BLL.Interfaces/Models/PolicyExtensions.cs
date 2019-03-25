using System.Linq;

namespace Ferretto.Common.BLL.Interfaces.Models
{
    public static class PolicyExtensions
    {
        #region Methods

        public static bool CanDelete<TPolicy>(this IPolicyDescriptor<TPolicy> policyDescriptor)
            where TPolicy : IPolicy
        {
            if (policyDescriptor == null)
            {
                throw new System.ArgumentNullException(nameof(policyDescriptor));
            }

            return policyDescriptor.Policies
                .Any(p => p.IsAllowed == true
                    && p.Name == CommonPolicies.Delete.ToString()
                    && p.Type == PolicyType.Operation);
        }

        public static string GetCanDeleteReason<TPolicy>(this IPolicyDescriptor<TPolicy> policyDescriptor)
            where TPolicy : IPolicy
        {
            if (policyDescriptor == null)
            {
                throw new System.ArgumentNullException(nameof(policyDescriptor));
            }

            return policyDescriptor.Policies
                .SingleOrDefault(p => p.Name == CommonPolicies.Delete.ToString()
                    && p.Type == PolicyType.Operation)?.Reason;
        }

        #endregion
    }
}

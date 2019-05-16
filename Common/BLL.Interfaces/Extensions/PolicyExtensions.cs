using System.Collections.Generic;
using System.Linq;

namespace Ferretto.Common.BLL.Interfaces.Models
{
    public static class PolicyExtensions
    {
        #region Methods

        public static bool CanCreate<TPolicy>(this IPolicyDescriptor<TPolicy> policyDescriptor)
            where TPolicy : IPolicy
        {
            return policyDescriptor.CanExecuteOperation(nameof(CommonPolicies.Create));
        }

        public static bool CanDelete<TPolicy>(this IPolicyDescriptor<TPolicy> policyDescriptor)
            where TPolicy : IPolicy
        {
            return policyDescriptor.CanExecuteOperation(nameof(CommonPolicies.Delete));
        }

        public static bool CanExecuteOperation<TPolicy>(
            this IPolicyDescriptor<TPolicy> policyDescriptor,
            string operationName)
            where TPolicy : IPolicy
        {
            if (policyDescriptor == null)
            {
                throw new System.ArgumentNullException(nameof(policyDescriptor));
            }

            if (operationName == null)
            {
                throw new System.ArgumentNullException(nameof(operationName));
            }

            if (policyDescriptor.Policies == null
                || policyDescriptor.Policies.Any() == false)
            {
                return true;
            }

            return policyDescriptor.Policies
                .Any(p => EqualityComparer<TPolicy>.Default.Equals(p, default(TPolicy)) == false
                    && p.IsAllowed
                    && p.Name == operationName
                    && p.Type == PolicyType.Operation);
        }

        public static bool CanUpdate<TPolicy>(this IPolicyDescriptor<TPolicy> policyDescriptor)
            where TPolicy : IPolicy
        {
            return policyDescriptor.CanExecuteOperation(nameof(CommonPolicies.Update));
        }

        public static string GetCanCreateReason<TPolicy>(this IPolicyDescriptor<TPolicy> policyDescriptor)
            where TPolicy : IPolicy
        {
            return policyDescriptor.GetCanExecuteOperationReason(nameof(CommonPolicies.Create));
        }

        public static string GetCanDeleteReason<TPolicy>(this IPolicyDescriptor<TPolicy> policyDescriptor)
            where TPolicy : IPolicy
        {
            return policyDescriptor.GetCanExecuteOperationReason(nameof(CommonPolicies.Delete));
        }

        public static string GetCanExecuteOperationReason<TPolicy>(
            this IPolicyDescriptor<TPolicy> policyDescriptor,
            string operationName)
            where TPolicy : IPolicy
        {
            if (policyDescriptor == null)
            {
                throw new System.ArgumentNullException(nameof(policyDescriptor));
            }

            if (operationName == null)
            {
                throw new System.ArgumentNullException(nameof(operationName));
            }

            return policyDescriptor.Policies?
                .SingleOrDefault(p => EqualityComparer<TPolicy>.Default.Equals(p, default(TPolicy)) == false
                    && p.Name == operationName
                    && p.Type == PolicyType.Operation)?.Reason;
        }

        public static string GetCanUpdateReason<TPolicy>(this IPolicyDescriptor<TPolicy> policyDescriptor)
            where TPolicy : IPolicy
        {
            return policyDescriptor.GetCanExecuteOperationReason(nameof(CommonPolicies.Update));
        }

        #endregion
    }
}

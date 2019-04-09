using System.Linq;

namespace Ferretto.Common.BLL.Interfaces.Models
{
    public static class PolicyExtensions
    {
        #region Methods

        public static bool CanCreate<TPolicy>(this IPolicyDescriptor<TPolicy> policyDescriptor)
            where TPolicy : IPolicy
        {
            return policyDescriptor.CanExecuteOperation(CommonPolicies.Create.ToString());
        }

        public static bool CanDelete<TPolicy>(this IPolicyDescriptor<TPolicy> policyDescriptor)
            where TPolicy : IPolicy
        {
            return policyDescriptor.CanExecuteOperation(CommonPolicies.Delete.ToString());
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

            return policyDescriptor.Policies
                .Any(p => p != null && p.IsAllowed
                    && p.Name == operationName
                    && p.Type == PolicyType.Operation);
        }

        public static bool CanUpdate<TPolicy>(this IPolicyDescriptor<TPolicy> policyDescriptor)
            where TPolicy : IPolicy
        {
            return policyDescriptor.CanExecuteOperation(CommonPolicies.Update.ToString());
        }

        public static string GetCanCreateReason<TPolicy>(this IPolicyDescriptor<TPolicy> policyDescriptor)
            where TPolicy : IPolicy
        {
            return policyDescriptor.GetCanExecuteOperationReason(CommonPolicies.Create.ToString());
        }

        public static string GetCanDeleteReason<TPolicy>(this IPolicyDescriptor<TPolicy> policyDescriptor)
            where TPolicy : IPolicy
        {
            return policyDescriptor.GetCanExecuteOperationReason(CommonPolicies.Delete.ToString());
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

            return policyDescriptor.Policies
                .SingleOrDefault(p => p != null && p.Name == operationName
                    && p.Type == PolicyType.Operation)?.Reason;
        }

        public static string GetCanUpdateReason<TPolicy>(this IPolicyDescriptor<TPolicy> policyDescriptor)
            where TPolicy : IPolicy
        {
            return policyDescriptor.GetCanExecuteOperationReason(CommonPolicies.Update.ToString());
        }

        #endregion
    }
}

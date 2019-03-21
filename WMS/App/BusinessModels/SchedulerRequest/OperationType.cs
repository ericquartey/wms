using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public enum OperationType
    {
        [Display(Name = "")]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.OperationInsert), ResourceType = typeof(BusinessObjects))]
        Insertion = 'I',

        [Display(Name = nameof(BusinessObjects.OperationWithdraw), ResourceType = typeof(BusinessObjects))]
        Withdrawal = 'W',

        [Display(Name = nameof(BusinessObjects.OperationReplace), ResourceType = typeof(BusinessObjects))]
        Replacement = 'R',

        [Display(Name = nameof(BusinessObjects.OperationReorder), ResourceType = typeof(BusinessObjects))]
        Reorder = 'O'
    }
}

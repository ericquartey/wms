using System.ComponentModel.DataAnnotations;

namespace Ferretto.Common.Resources.Enums
{
    public enum OperationType
    {
        [Display(Name = nameof(BusinessObjects.EnumNotSpecified), ResourceType = typeof(BusinessObjects))]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.Put), ResourceType = typeof(BusinessObjects))]
        Put = 'U',

        [Display(Name = nameof(BusinessObjects.Pick), ResourceType = typeof(BusinessObjects))]
        Pick = 'P',

        [Display(Name = nameof(BusinessObjects.OperationReplace), ResourceType = typeof(BusinessObjects))]
        Replacement = 'R',

        [Display(Name = nameof(BusinessObjects.OperationReorder), ResourceType = typeof(BusinessObjects))]
        Reorder = 'O',
    }
}

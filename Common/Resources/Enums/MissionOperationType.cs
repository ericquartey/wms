using System.ComponentModel.DataAnnotations;

namespace Ferretto.Common.Resources.Enums
{
    public enum MissionOperationType
    {
        [Display(Name = nameof(BusinessObjects.EnumNotSpecified), ResourceType = typeof(BusinessObjects))]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.MissionTypeBypass), ResourceType = typeof(BusinessObjects))]
        Bypass = 'B',

        [Display(Name = nameof(BusinessObjects.Inventory), ResourceType = typeof(BusinessObjects))]
        Inventory = 'I',

        [Display(Name = nameof(BusinessObjects.Pick), ResourceType = typeof(BusinessObjects))]
        Pick = 'P',

        [Display(Name = nameof(BusinessObjects.Put), ResourceType = typeof(BusinessObjects))]
        Put = 'T',

        [Display(Name = nameof(BusinessObjects.MissionTypeReorder), ResourceType = typeof(BusinessObjects))]
        Reorder = 'O',

        [Display(Name = nameof(BusinessObjects.MissionTypeReplace), ResourceType = typeof(BusinessObjects))]
        Replace = 'R',
    }
}

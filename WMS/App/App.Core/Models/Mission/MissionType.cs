using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public enum MissionType
    {
        [Display(Name = "")]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.MissionTypeBypass), ResourceType = typeof(BusinessObjects))]
        Bypass = 'B',

        [Display(Name = nameof(BusinessObjects.MissionTypeInventory), ResourceType = typeof(BusinessObjects))]
        Inventory = 'I',

        [Display(Name = nameof(BusinessObjects.MissionTypePick), ResourceType = typeof(BusinessObjects))]
        Pick = 'P',

        [Display(Name = nameof(BusinessObjects.MissionTypePut), ResourceType = typeof(BusinessObjects))]
        Put = 'T',

        [Display(Name = nameof(BusinessObjects.MissionTypeReorder), ResourceType = typeof(BusinessObjects))]
        Reorder = 'O',

        [Display(Name = nameof(BusinessObjects.MissionTypeReplace), ResourceType = typeof(BusinessObjects))]
        Replace = 'R'
    }
}

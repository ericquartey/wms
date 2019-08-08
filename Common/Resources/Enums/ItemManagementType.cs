using System.ComponentModel.DataAnnotations;

namespace Ferretto.Common.Resources.Enums
{
    public enum ItemManagementType
    {
        [Display(Name = nameof(BusinessObjects.EnumNotSpecified), ResourceType = typeof(BusinessObjects))]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.ItemManagementTypeFIFO), ResourceType = typeof(BusinessObjects))]
        FIFO = 'F',

        [Display(Name = nameof(BusinessObjects.ItemManagementTypeVolume), ResourceType = typeof(BusinessObjects))]
        Volume = 'V',
    }
}

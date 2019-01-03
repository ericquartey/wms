using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public enum ItemManagementType
    {
        [Display(Name = nameof(BusinessObjects.ItemManagementTypeFIFO), ResourceType = typeof(BusinessObjects))]
        FIFO = 'F',

        [Display(Name = nameof(BusinessObjects.ItemManagementTypeVolume), ResourceType = typeof(BusinessObjects))]
        Volume = 'V'
    }
}

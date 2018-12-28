using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public enum ItemManagementType
    {
        [Display(Name = nameof(BusinessObjects.TypeFIFO), ResourceType = typeof(BusinessObjects))]
        FIFO = 'F',

        [Display(Name = nameof(BusinessObjects.TypeVolume), ResourceType = typeof(BusinessObjects))]
        Volume = 'V'
    }
}

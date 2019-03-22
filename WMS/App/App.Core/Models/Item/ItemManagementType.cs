using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public enum ItemManagementType
    {
        [Display(Name = "")]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.ItemManagementTypeFIFO), ResourceType = typeof(BusinessObjects))]
        FIFO = 'F',

        [Display(Name = nameof(BusinessObjects.ItemManagementTypeVolume), ResourceType = typeof(BusinessObjects))]
        Volume = 'V'
    }
}

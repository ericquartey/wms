using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public enum ItemListStatus
    {
        [Display(Name = "")]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.ItemListStatusWaiting), ResourceType = typeof(BusinessObjects))]
        Waiting = 'W',

        [Display(Name = nameof(BusinessObjects.ItemListStatusExecuting), ResourceType = typeof(BusinessObjects))]
        Executing = 'E',

        [Display(Name = nameof(BusinessObjects.ItemListStatusCompleted), ResourceType = typeof(BusinessObjects))]
        Completed = 'C',

        [Display(Name = nameof(BusinessObjects.ItemListStatusIncomplete), ResourceType = typeof(BusinessObjects))]
        Incomplete = 'I',

        [Display(Name = nameof(BusinessObjects.ItemListStatusSuspended), ResourceType = typeof(BusinessObjects))]
        Suspended = 'S',
    }
}

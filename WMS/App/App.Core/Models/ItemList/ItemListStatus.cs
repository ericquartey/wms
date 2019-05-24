using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public enum ItemListStatus
    {
        [Display(Name = nameof(BusinessObjects.ItemListStatusWaiting), ResourceType = typeof(BusinessObjects))]
        Waiting = 'W',

        [Display(Name = nameof(BusinessObjects.ItemListStatusNew), ResourceType = typeof(BusinessObjects))]
        New = 'N',

        [Display(Name = nameof(BusinessObjects.ItemListStatusReady), ResourceType = typeof(BusinessObjects))]
        Ready = 'R',

        [Display(Name = nameof(BusinessObjects.ItemListStatusExecuting), ResourceType = typeof(BusinessObjects))]
        Executing = 'X',

        [Display(Name = nameof(BusinessObjects.ItemListStatusError), ResourceType = typeof(BusinessObjects))]
        Error = 'E',

        [Display(Name = nameof(BusinessObjects.ItemListStatusCompleted), ResourceType = typeof(BusinessObjects))]
        Completed = 'C',

        [Display(Name = nameof(BusinessObjects.ItemListStatusIncomplete), ResourceType = typeof(BusinessObjects))]
        Incomplete = 'I',

        [Display(Name = nameof(BusinessObjects.ItemListStatusSuspended), ResourceType = typeof(BusinessObjects))]
        Suspended = 'S',
    }
}

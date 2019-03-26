using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public enum ItemListRowStatus
    {
        [Display(Name = "")]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusNew), ResourceType = typeof(BusinessObjects))]
        New = 'N',

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusWaiting), ResourceType = typeof(BusinessObjects))]
        Waiting = 'W',

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusExecuting), ResourceType = typeof(BusinessObjects))]
        Executing = 'X',

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusCompleted), ResourceType = typeof(BusinessObjects))]
        Completed = 'C',

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusIncomplete), ResourceType = typeof(BusinessObjects))]
        Incomplete = 'I',

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusSuspended), ResourceType = typeof(BusinessObjects))]
        Suspended = 'S',

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusError), ResourceType = typeof(BusinessObjects))]
        Error = 'E',
    }
}

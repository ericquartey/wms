using System.ComponentModel.DataAnnotations;

namespace Ferretto.Common.Resources.Enums
{
    // Stato di Riga di Lista Articoli
    public enum ItemListRowStatus
    {
        [Display(Name = nameof(BusinessObjects.EnumNotSpecified), ResourceType = typeof(BusinessObjects))]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusNew), ResourceType = typeof(BusinessObjects))]
        New = 'N',

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusReady), ResourceType = typeof(BusinessObjects))]
        Ready = 'R',

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusWaiting), ResourceType = typeof(BusinessObjects))]
        Waiting = 'W',

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusExecuting), ResourceType = typeof(BusinessObjects))]
        Executing = 'X',

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusCompleted), ResourceType = typeof(BusinessObjects))]
        Completed = 'C',

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusError), ResourceType = typeof(BusinessObjects))]
        Error = 'E',

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusIncomplete), ResourceType = typeof(BusinessObjects))]
        Incomplete = 'I',

        [Display(Name = nameof(BusinessObjects.ItemListRowStatusSuspended), ResourceType = typeof(BusinessObjects))]
        Suspended = 'S',
    }
}

using System.ComponentModel.DataAnnotations;

namespace Ferretto.Common.Resources.Enums
{
    public enum SchedulerRequestStatus
    {
        [Display(Name = nameof(BusinessObjects.EnumNotSpecified), ResourceType = typeof(BusinessObjects))]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.SchedulerRequestStatusNew), ResourceType = typeof(BusinessObjects))]
        New = 'N',

        [Display(Name = nameof(BusinessObjects.SchedulerRequestStatusCompleted), ResourceType = typeof(BusinessObjects))]
        Completed = 'C',
    }
}

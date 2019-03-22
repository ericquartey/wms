using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public enum MissionStatus
    {
        [Display(Name = "")]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.MissionStatusNew), ResourceType = typeof(BusinessObjects))]
        New = 'N',

        [Display(Name = nameof(BusinessObjects.MissionStatusWaiting), ResourceType = typeof(BusinessObjects))]
        Waiting = 'W',

        [Display(Name = nameof(BusinessObjects.MissionStatusExecuting), ResourceType = typeof(BusinessObjects))]
        Executing = 'X',

        [Display(Name = nameof(BusinessObjects.MissionStatusCompleted), ResourceType = typeof(BusinessObjects))]
        Completed = 'C',

        [Display(Name = nameof(BusinessObjects.MissionStatusError), ResourceType = typeof(BusinessObjects))]
        Error = 'E',

        [Display(Name = nameof(BusinessObjects.MissionStatusIncomplete), ResourceType = typeof(BusinessObjects))]
        Incomplete = 'I'
    }
}

using System.ComponentModel.DataAnnotations;

namespace Ferretto.Common.Resources.Enums
{
    public enum MissionStatus
    {
        [Display(Name = nameof(BusinessObjects.EnumNotSpecified), ResourceType = typeof(BusinessObjects))]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.MissionStatusNew), ResourceType = typeof(BusinessObjects))]
        New = 'N',

        [Display(Name = nameof(BusinessObjects.MissionStatusExecuting), ResourceType = typeof(BusinessObjects))]
        Executing = 'X',

        [Display(Name = nameof(BusinessObjects.MissionStatusCompleted), ResourceType = typeof(BusinessObjects))]
        Completed = 'C',

        [Display(Name = nameof(BusinessObjects.MissionStatusError), ResourceType = typeof(BusinessObjects))]
        Error = 'E',

        [Display(Name = nameof(BusinessObjects.MissionStatusIncomplete), ResourceType = typeof(BusinessObjects))]
        Incomplete = 'I',
    }
}

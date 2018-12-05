using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public enum MachineStatus
    {
        [Display(Name = nameof(BusinessObjects.MachineStatusAutomatic), ResourceType = typeof(BusinessObjects))]
        Automatic,

        [Display(Name = nameof(BusinessObjects.MachineStatusManual), ResourceType = typeof(BusinessObjects))]
        Manual,

        [Display(Name = nameof(BusinessObjects.MachineStatusFaulted), ResourceType = typeof(BusinessObjects))]
        Faulted,

        [Display(Name = nameof(BusinessObjects.MachineStatusOffline), ResourceType = typeof(BusinessObjects))]
        Offline
    }
}

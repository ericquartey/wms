using System.ComponentModel.DataAnnotations;

namespace Ferretto.Common.Resources.Enums
{
    public enum MachineStatus
    {
        [Display(Name = nameof(BusinessObjects.EnumNotSpecified), ResourceType = typeof(BusinessObjects))]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.MachineStatusAutomatic), ResourceType = typeof(BusinessObjects))]
        Automatic = 'A',

        [Display(Name = nameof(BusinessObjects.MachineStatusManual), ResourceType = typeof(BusinessObjects))]
        Manual = 'M',

        [Display(Name = nameof(BusinessObjects.MachineStatusError), ResourceType = typeof(BusinessObjects))]
        Error = 'E',

        [Display(Name = nameof(BusinessObjects.MachineStatusOffline), ResourceType = typeof(BusinessObjects))]
        Offline = 'O',
    }
}

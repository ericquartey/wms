using System.ComponentModel.DataAnnotations;

namespace Ferretto.Common.Resources.Enums
{
    public enum MaintenanceStatus
    {
        [Display(Name = nameof(BusinessObjects.EnumNotSpecified), ResourceType = typeof(BusinessObjects))]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.MachineMaintenanceStatusValid), ResourceType = typeof(BusinessObjects))]
        Valid = 'V',

        [Display(Name = nameof(BusinessObjects.MachineMaintenanceStatusExpiring), ResourceType = typeof(BusinessObjects))]
        Expiring = 'G',

        [Display(Name = nameof(BusinessObjects.MachineMaintenanceStatusExpired), ResourceType = typeof(BusinessObjects))]
        Expired = 'X',
    }
}

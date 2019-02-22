using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public enum MaintenanceStatus
    {
        [Display(Name = nameof(BusinessObjects.MachineMaintenanceStatusValid), ResourceType = typeof(BusinessObjects))]
        Valid = 'V',

        [Display(Name = nameof(BusinessObjects.MachineMaintenanceStatusExpiring), ResourceType = typeof(BusinessObjects))]
        Expiring = 'G',

        [Display(Name = nameof(BusinessObjects.MachineMaintenanceStatusExpired), ResourceType = typeof(BusinessObjects))]
        Expired = 'X',
    }
}

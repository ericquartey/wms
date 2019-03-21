using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public enum MachineStatus
    {
        [Display(Name = "")]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.MachineStatusAutomatic), ResourceType = typeof(BusinessObjects))]
        Automatic = 'A',

        [Display(Name = nameof(BusinessObjects.MachineStatusManual), ResourceType = typeof(BusinessObjects))]
        Manual = 'M',

        [Display(Name = nameof(BusinessObjects.MachineStatusError), ResourceType = typeof(BusinessObjects))]
        Error = 'E',

        [Display(Name = nameof(BusinessObjects.MachineStatusOffline), ResourceType = typeof(BusinessObjects))]
        Offline = 'O'
    }
}

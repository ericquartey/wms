using System.ComponentModel.DataAnnotations;
using Ferretto.VW.App.Installation.Attributes;

namespace Ferretto.VW.App.Installation.Resources
{
    public enum InstallatorMenus
    {
        [View(Utils.Modules.Installation.Sensors.BAYS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Sensors)]
        [Display(Description = "Stato sensori")]
        BaySensors
    }
}

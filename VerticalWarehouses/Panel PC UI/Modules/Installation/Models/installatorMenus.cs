using System.ComponentModel.DataAnnotations;
using Ferretto.VW.App.Installation.Attributes;

namespace Ferretto.VW.App.Installation.Resources
{
    public enum InstallatorMenus
    {
        [View(Utils.Modules.Installation.VERTICALAXISCALIBRATION, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installator)]
        [Display(Description = "Origine asse verticale")]
        VerticalAxisCalibration,

        [View(Utils.Modules.Installation.VERTICALOFFSETCALIBRATION, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installator)]
        [Display(Description = "Calibrazione offset asse verticale")]
        VerticalOffsetCalibration,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Sensors)]
        [Display(Description = "Stato sensori")]
        SensorsState,

        [View(Utils.Modules.Installation.ManualMovements.VERTICALENGINE, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Movimenti manuali a bassa velocità")]
        VerticalEngineManualMovements,

        [View(Utils.Modules.Installation.SHUTTERENDURANCETEST, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Test Serranda")]
        ShutterEnduranceTest,

        //To DO remove after implementing PpcControl for navigation of sub menus
        None
    }
}

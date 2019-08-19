using System.ComponentModel.DataAnnotations;
using Ferretto.VW.App.Installation.Attributes;

namespace Ferretto.VW.App.Installation.Resources
{
    public enum InstallationMenus
    {
        [View(Utils.Modules.Installation.SHUTTERENDURANCETEST, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installator)]
        [Display(Description = "Test serranda")]
        BayShutter,

        [View(Utils.Modules.Installation.VERTICALORIGINCALIBRATION, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installator)]
        [Display(Description = "Calibrazione origine asse verticale")]
        VerticalOriginCalibration,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installator)]
        [Display(Description = "Rodaggio cinghia")]
        BeltBurnishing,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installator)]
        [Display(Description = "Calibrazione risoluzione asse verticale")]
        VerticalResolution,

        [View(Utils.Modules.Installation.VERTICALOFFSETCALIBRATION, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installator)]
        [Display(Description = "Calibrazione offset asse verticale")]
        VerticalOffsetCalibration,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installator)]
        [Display(Description = "Controllo baia")]
        BayCheck,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installator)]
        [Display(Description = "Controllo Pannelli Mensola")]
        PanelsCheck,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installator)]
        [Display(Description = "Barriera misura altezza")]
        BayShape,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installator)]
        [Display(Description = "Controllo Peso")]
        WeightMeasurement,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installator)]
        [Display(Description = "Inserimento primo cassetto")]
        BayFirstLoadingUnit,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installator)]
        [Display(Description = "Inserimento cassetti")]
        AllLoadingUnits,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installator)]
        [Display(Description = "Salva e ripristina")]
        SaveRestore,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Sensors)]
        [Display(Description = "Stato sensori")]
        SensorsState,

        [View(Utils.Modules.Installation.ManualMovements.VERTICALENGINE, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Movimenti manuali a bassa velocità")]
        VerticalEngineManualMovements,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Controllo quote celle")]
        CellsHeightCheck,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Modifica blocco celle")]
        CellsBlockTuning,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Test di Imbarco/Sbarco")]
        HorizontalHoming,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Puntatore Laser")]
        BayLaser,

        //To DO remove after implementing PpcControl for navigation of sub menus
        None
    }
}

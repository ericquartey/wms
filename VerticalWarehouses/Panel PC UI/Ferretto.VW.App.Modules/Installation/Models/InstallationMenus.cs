using System.ComponentModel.DataAnnotations;

using Ferretto.VW.App.Installation.Attributes;
using Ferretto.VW.App.Modules.Installation.Models;

namespace Ferretto.VW.App.Installation.Resources
{
    public enum InstallationMenus
    {
        [View(Utils.Modules.Installation.SHUTTERENDURANCETEST, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Test serranda")]
        BayShutter,

        [View(Utils.Modules.Installation.VERTICALORIGINCALIBRATION, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Calibrazione origine asse verticale")]
        VerticalOriginCalibration,

        [View(Utils.Modules.Installation.BELTBURNISHING, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Rodaggio cinghia")]
        BeltBurnishing,

        [View(Utils.Modules.Installation.VerticalResolutionCalibration.STEP1, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Calibrazione risoluzione asse verticale")]
        VerticalResolutionCalibration,

        [View(Utils.Modules.Installation.VerticalOffsetCalibration.STEP1, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Calibrazione offset asse verticale")]
        VerticalOffsetCalibration,

        [View(Utils.Modules.Installation.Bays.BAYHEIGHTCHECK, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Controllo baia")]
        BayCheck,

        [View(Utils.Modules.Installation.CELLPANELSCHECK, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Controllo Pannelli Mensola")]
        CellPanelsCheck,

        [View(Utils.Modules.Installation.SHUTTTERHEIGHTCONTROL, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Barriera misura altezza")]
        BayShape,

        [View(Utils.Modules.Installation.Elevator.WeightCheck.STEP1, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Controllo Peso")]
        WeightMeasurement,

        [View(Utils.Modules.Installation.LOADFIRSTDRAWER, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Inserimento primo cassetto")]
        BayFirstLoadingUnit,

        [View(Utils.Modules.Installation.Parameters.PARAMETERS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Parametri")]
        Parameters,

        [View(Utils.Modules.Installation.Sensors.SECURITY, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Stato sensori")]
        SensorsState,

        [View(Utils.Modules.Installation.ManualMovements.MANUALMOVEMENTS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Movimenti manuali a bassa velocità")]
        ManualMovements,

        [View(Utils.Modules.Installation.SEMIAUTOMOVEMENTS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Movimenti semi-automatici")]
        SemiAutoMovements,

        [View(Utils.Modules.Installation.WEIGHTANALYSIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Analisi profilo peso")]
        WeightAnalysis,

        [View(Utils.Modules.Installation.CellsHeightCheck.STEP1, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Controllo quote celle")]
        CellsHeightCheck,

        [View(Utils.Modules.Installation.CELLSSIDECONTROL, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Modifica blocco celle")]
        CellsBlockTuning,

        [View(Utils.Modules.Installation.Bays.DEPOSITANDPICKUPTEST, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Test di Imbarco/Sbarco")]
        HorizontalHoming,

        //[View(Utils.Modules.Installation.INSTALLATORMENU, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        //[Display(Description = "Puntatore Laser")]
        //BayLaser,

        [View(Utils.Modules.Installation.CellsLoadingUnitsMenu.MENU, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Celle e Cassetti")]
        CellsLoadingUnits,

        [View(Utils.Modules.Installation.LoadingUnits.LOADINGUNITFROMBAYTOCELL, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Inserimento cassetti da baia in cella")]
        MovementsFromBayToCell,

        [View(Utils.Modules.Installation.LoadingUnits.LOADINGUNITFROMBAYTOBAY, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Spostamento cassetti da baia a baia")]
        MovementsFromBayToBay,

        [View(Utils.Modules.Installation.LoadingUnits.LOADINGUNITFROMCELLTOCELL, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Spostamento cassetti da cella a cella")]
        MovemetsFromCellToCell,

        [View(Utils.Modules.Installation.LoadingUnits.LOADINGUNITFROMCELLTOBAY, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Spostamento cassetti da cella in baia")]
        MovemetsFromCellToBay,

        None,
    }
}

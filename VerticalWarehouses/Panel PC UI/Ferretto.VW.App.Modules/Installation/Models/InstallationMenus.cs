﻿using System.ComponentModel.DataAnnotations;

using Ferretto.VW.App.Installation.Attributes;

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

        [View(Utils.Modules.Installation.SHUTTTER1HEIGHTCONTROL, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Barriera misura altezza")]
        BayShape,

        [View(Utils.Modules.Installation.Elevator.WeightCheck.STEP1, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Controllo Peso")]
        WeightMeasurement,

        [View(Utils.Modules.Installation.LOADFIRSTDRAWER, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Inserimento primo cassetto")]
        BayFirstLoadingUnit,

        [View(Utils.Modules.Installation.LoadingUnits.LOADINGUNITFROMBAYTOCELL, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Inserimento cassetti")]
        AllLoadingUnits,

        [View(Utils.Modules.Installation.SAVERESTORECONFIG, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Installer)]
        [Display(Description = "Salva e ripristina")]
        SaveRestore,

        [View(Utils.Modules.Installation.Sensors.VERTICALAXIS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Sensors)]
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

        [View(Utils.Modules.Installation.INSTALLATORMENU, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Puntatore Laser")]
        BayLaser,

        [View(Utils.Modules.Installation.Parameters.PARAMETERS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Others)]
        [Display(Description = "Parametri")]
        Parameters,

        None,
    }
}

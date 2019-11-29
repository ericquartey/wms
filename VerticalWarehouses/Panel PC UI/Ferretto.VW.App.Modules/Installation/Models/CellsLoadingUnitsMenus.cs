using System.ComponentModel.DataAnnotations;

using Ferretto.VW.App.Installation.Attributes;

namespace Ferretto.VW.App.Installation.Resources
{
    public enum CellsLoadingUnitsMenus
    {
        [View(Utils.Modules.Installation.CellsLoadingUnitsMenu.CELLES, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Cell)]
        [Display(Description = "Celle")]
        Cells,

        [View(Utils.Modules.Installation.CELLPANELSCHECK, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Cell)]
        [Display(Description = "Controllo Pannelli")]
        CellPanelsCheck,

        [View(Utils.Modules.Installation.CellsHeightCheck.STEP1, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Cell)]
        [Display(Description = "Modifica quote celle")]
        CellsHeightCheck,

        [View(Utils.Modules.Installation.CELLSSIDECONTROL, nameof(Utils.Modules.Installation), InstallatorMenuTypes.Cell)]
        [Display(Description = "Modifica blocco celle")]
        CellsBlockTuning,

        [View(Utils.Modules.Installation.CellsLoadingUnitsMenu.LOADINGUNITS, nameof(Utils.Modules.Installation), InstallatorMenuTypes.LoadingUnit)]
        [Display(Description = "Cassetti")]
        LoadingUnits,

        [View(Utils.Modules.Installation.LoadingUnits.LOADINGUNITFROMBAYTOCELL, nameof(Utils.Modules.Installation), InstallatorMenuTypes.LoadingUnit)]
        [Display(Description = "Carica cassetto")]
        MovementsFromBayToCell,

        [View(Utils.Modules.Installation.LoadingUnits.LOADINGUNITFROMCELLTOCELL, nameof(Utils.Modules.Installation), InstallatorMenuTypes.LoadingUnit)]
        [Display(Description = "Sposta cassetto")]
        MovemetsFromCellToCell,

        [View(Utils.Modules.Installation.LoadingUnits.LOADINGUNITFROMCELLTOBAY, nameof(Utils.Modules.Installation), InstallatorMenuTypes.LoadingUnit)]
        [Display(Description = "Estrai cassetto")]
        MovemetsFromCellToBay,

        None,
    }
}

using System.ComponentModel.DataAnnotations;

using Ferretto.VW.App.Operator.Attributes;

namespace Ferretto.VW.App.Operator.Resources
{
    public enum OperatorMenus
    {
        [View(Utils.Modules.Operator.EMPTY, nameof(Utils.Modules.Operator), OperatorMenuTypes.Operator)]
        [Display(Description = "Operation su cassetto")]
        DrawerOperation,

        [View(Utils.Modules.Operator.EMPTY, nameof(Utils.Modules.Operator), OperatorMenuTypes.Operator)]
        [Display(Description = "Ricerca articolo")]
        ItemSearch,

        [View(Utils.Modules.Operator.EMPTY, nameof(Utils.Modules.Operator), OperatorMenuTypes.Operator)]
        [Display(Description = "Liste in attesa")]
        WaitLists,

        [View(Utils.Modules.Operator.EMPTY, nameof(Utils.Modules.Operator), OperatorMenuTypes.Operator)]
        [Display(Description = "Altro")]
        Olther,

        None,
    }
}
